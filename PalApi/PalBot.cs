using System.Threading.Tasks;

namespace PalApi
{
    using Delegates;
    using DependencyInjection;
    using Networking;
    using Networking.Handling;
    using Networking.Mapping;
    using Networking.Watcher;
    using SubProfile;
    using Types;
    using Utilities;
    using Plugins;
    using System;

    public interface IPalBot : IPalBotSenders
    {
        event VoidCarrier OnDisconnected;
        event ExceptionCarrier OnException;
        event PacketCarrier OnPacketReceived;
        event PacketCarrier OnPacketSent;
        event PacketCarrier OnUnhandledPacket;
        event StringCarrier OnLoginFailed;
        event MessageCarrier OnMessage;

        string Email { get; }
        string Password { get; }
        AuthStatus Status { get; }
        DeviceType Device { get; }
        bool SpamFilter { get; }
        ExtendedUser Profile { get; }

        Task<bool> Write(IPacket packet);
        Task<bool> Write(IPacketMap packet);

        IPalBot Disconnected(Action action);
        IPalBot LoginFailed(Action<string> action);
        IPalBot Error(Action<Exception, string> action);
        IPalBot MessageReceived(Action<IPalBot, Message> action);
        IPalBot CouldNotConnect(Action action);
    }

    public partial class PalBot : IPalBot
    {
        public static string DefaultHost = "im.palringo.com";
        public static int DefaultPort = 12345;

        public event VoidCarrier OnDisconnected = delegate { };
        public event ExceptionCarrier OnException = delegate { };
        public event PacketCarrier OnPacketReceived = delegate { };
        public event PacketCarrier OnPacketSent = delegate { };
        public event PacketCarrier OnUnhandledPacket = delegate { };
        public event StringCarrier OnLoginFailed = delegate { };
        public event MessageCarrier OnMessage = delegate { };

        public string Email { get; private set; }
        public string Password { get; private set; }
        public AuthStatus Status { get; private set; }
        public DeviceType Device { get; private set; }
        public bool SpamFilter { get; private set; }
        public IRoleManager RoleManager { get; }
        public ISubProfiling SubProfiling { get; }

        public ExtendedUser Profile => SubProfiling.Profile;

        private IPacketSerializer packetSerializer;
        private IPacketDeserializer packetDeserializer;
        private IPacketMapper packetMapper;
        private IPacketWatcher packetWatcher;
        private IPacketTemplates packetTemplates;
        private IZLibCompression compression;
        private IAuthenticationUtility authentication;
        private IPacketHandlerHub handlerHub;
        private IPluginManager pluginManager;
        private NetworkClient _client;

        private Action<string> _loginFailed;
        private Action _disconnected;
        private Action _couldntConnect;
        private Action<Exception, string> _error;
        private Action<IPalBot, Message> _message;

        public PalBot(IPacketSerializer packetSerializer,
            IPacketDeserializer packetDeserializer,
            IPacketMapper packetMapper,
            IPacketWatcher packetWatcher,
            IPacketTemplates packetTemplates,
            IZLibCompression compression,
            IAuthenticationUtility authentication,
            IPacketHandlerHub handlerHub,
            ISubProfiling subProfiling,
            IPluginManager pluginManager,
            IRoleManager roleManager)
        {
            this.packetSerializer = packetSerializer;
            this.packetDeserializer = packetDeserializer;
            this.packetMapper = packetMapper;
            this.packetWatcher = packetWatcher;
            this.packetTemplates = packetTemplates;
            this.compression = compression;
            this.authentication = authentication;
            this.handlerHub = handlerHub;
            this.SubProfiling = subProfiling;
            this.pluginManager = pluginManager;
            this.RoleManager = roleManager;

            _client = new NetworkClient(DefaultHost, DefaultPort);
            _client.OnDisconnected += (c) => _disconnected?.Invoke();
            _client.OnDisconnected += (c) => OnDisconnected();
            _client.OnException += (e, n) => _error?.Invoke(e, n);
            _client.OnException += (e, n) => OnException(e, n);
            _client.OnDataReceived += (c, b) => this.packetDeserializer.ReadPacket(c, b);

            this.pluginManager.OnException += (e, n) => OnException(e, n);
            this.pluginManager.OnException += (e, n) => _error?.Invoke(e, n);
            this.pluginManager.OnMessage += (b, m) => OnMessage(b, m);
            this.pluginManager.OnMessage += (b, m) => _message?.Invoke(b, m);
            this.packetSerializer.OnException += (e, n) => OnException(e, n);
            this.packetSerializer.OnException += (e, n) => _error?.Invoke(e, n);
            this.handlerHub.OnException += (e, n) => OnException(e, n);
            this.handlerHub.OnException += (e, n) => _error?.Invoke(e, n);
            this.packetWatcher.OnException += (e, n) => OnException(e, n);
            this.packetWatcher.OnException += (e, n) => _error?.Invoke(e, n);
            this.SubProfiling.OnException += (e, n) => OnException(e, n);
            this.SubProfiling.OnException += (e, n) => _error?.Invoke(e, n);
            this.packetDeserializer.OnException += (e, n) => OnException(e, n);
            this.packetDeserializer.OnException += (e, n) => _error?.Invoke(e, n);

            this.packetDeserializer.OnPacketParsed += (c, p) => PacketReceived(p);
        }
        
        public async Task<bool> Write(IPacket packet)
        {
            if (!_client?.Connected ?? false)
                return false;

            var serialized = packetSerializer.Serialize(packet);

            bool worked = false;

            foreach (var data in serialized)
            {
                worked = await _client.WriteData(data);
            }

            if (worked)
                OnPacketSent(packet);

            return worked;
        }

        public async Task<bool> Write(IPacketMap map)
        {
            var packet = packetMapper.Unmap(map);
            if (packet == null)
                return false;

            return await Write(packet);
        }

        private void PacketReceived(IPacket packet)
        {
            if (packet["COMPRESSION"] == "1" && packet.Payload != null)
            {
                packet.Payload = compression.Decompress(packet.Payload);
            }

            var map = packetMapper.Map(packet);
            if (map == null) //Unhandled Packet
            {
                OnUnhandledPacket(packet);
                return;
            }


            //Notify any attached processes that a packet has been received.
            OnPacketReceived(packet.Clone());

            //Process any packet handlers on the packet
            handlerHub.ProcessPacket(this, map);

            //Process any watchs that are on the packet
            packetWatcher.Process(map);

            //Process any plugins
            pluginManager.Process(this, map);
        }
        
        public IPalBot Disconnected(Action action)
        {
            this._disconnected = action;
            return this;
        }

        public IPalBot LoginFailed(Action<string> action)
        {
            this._loginFailed = action;
            return this;
        }

        public IPalBot Error(Action<Exception, string> action)
        {
            this._error = action;
            return this;
        }

        public IPalBot MessageReceived(Action<IPalBot, Message> action)
        {
            this._message = action;
            return this;
        }

        public IPalBot CouldNotConnect(Action action)
        {
            _couldntConnect = action;
            return this;
        }

        public static MapHandler<IPalBot> Mapper()
        {
            return new MapHandler<IPalBot>()
                .AllOf<IPacketHandler>()
                .AllOf<IPlugin>()
                .AllOf<IRole>()
                .Use<IReflectionUtility, ReflectionUtility>((c, i) => new ReflectionUtility(c));
        }

        public static IPalBot Create()
        {
            return Mapper().Build();
        }
    }
}
