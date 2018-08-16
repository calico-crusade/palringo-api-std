using System;
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
    using PacketTypes;
    using Plugins;
    using Plugins.Linguistics;
    using Plugins.Linguistics.StorageType;
    using Plugins.Roles;

    public interface IPalBot : IPalBotLinguistics
    {
        string Email { get; }
        string Password { get; }
        AuthStatus Status { get; }
        DeviceType Device { get; }
        bool SpamFilter { get; }
        ExtendedUser Profile { get; }
        bool EnablePlugins { get; }
        string[] Groupings { get; }
        LinguisticsEngine Languages { get; }
        IBroadcastUtility On { get; }

        Task<bool> Write(IPacket packet);
        Task<bool> Write(IPacketMap packet);

        IPalBot Disconnected(Action action);
        IPalBot LoginFailed(Action<string> action);
        IPalBot Error(Action<Exception, string> action);
        IPalBot MessageReceived(Action<IPalBot, Message> action);
        IPalBot CouldNotConnect(Action action);

        IPalBot SetGroupings(params string[] groupings);

        IPalBot LanguagesFromJson(string filePath);
        IPalBot LanguagesFromFlatFile(string filePath);
        IPalBot LanguagesFrom(ILocalizationStorage storage);
    }

    public partial class PalBot : IPalBot
    {
        public static string DefaultHost = "im.palringo.com";
        public static int DefaultPort = 12345;
        
        public string Email { get; private set; }
        public string Password { get; private set; }
        public AuthStatus Status { get; private set; }
        public DeviceType Device { get; private set; }
        public bool SpamFilter { get; private set; }
        public bool EnablePlugins { get; private set; }
        public string[] Groupings { get; private set; }
        public LinguisticsEngine Languages { get; private set; }
        public IBroadcastUtility On { get; private set; }
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
        private BroadcastUtility broadcast;

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
            IRoleManager roleManager,
            IBroadcastUtility broadcast)
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
            this.On = broadcast;
            this.broadcast = (BroadcastUtility)broadcast;

            _client = new NetworkClient(DefaultHost, DefaultPort);
            _client.OnDisconnected += (c) => _disconnected?.Invoke();
            _client.OnDisconnected += (c) => ((BroadcastUtility)On).BroadcastDisconnected();
            _client.OnException += (e, n) => _error?.Invoke(e, n);
            _client.OnException += (e, n) => ((BroadcastUtility)On).BroadcastException(e, n);
            _client.OnDataReceived += (c, b) => this.packetDeserializer.ReadPacket(c, b);

            On.Exception += (e, n) => _error?.Invoke(e, n);
            On.Message += (b, m) => _message?.Invoke(b, m);
            this.broadcast.PacketParsed += (c, p) => PacketReceived(p);
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
                ((BroadcastUtility)On).BroadcastPacketSent(packet);

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
                ((BroadcastUtility)On).BroadcastUnhandledPacket(packet);
                return;
            }


            //Notify any attached processes that a packet has been received.
            ((BroadcastUtility)On).BroadcastPacketReceived(packet.Clone());

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

        public IPalBot SetGroupings(params string[] groupings)
        {
            this.Groupings = groupings;
            return this;
        }

        public IPalBot LanguagesFromJson(string filePath)
        {
            return LanguagesFrom(LocalizationStorageJson.Create(filePath));
        }

        public IPalBot LanguagesFromFlatFile(string filePath)
        {
            return LanguagesFrom(LocalizationStorageFlatFile.Create(filePath));
        }

        public IPalBot LanguagesFrom(ILocalizationStorage storage)
        {
            Languages = new LinguisticsEngine(storage);
            storage.OnError += (e, n) => broadcast.BroadcastException(e, n);
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
