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

    public interface IPalBot : IPalBotSenders
    {
        event VoidCarrier OnDisconnected;
        event ExceptionCarrier OnException;
        event PacketCarrier OnPacketReceived;
        event PacketCarrier OnPacketSent;
        event PacketCarrier OnUnhandledPacket;
        event StringCarrier OnLoginFailed;

        string Email { get; }
        string Password { get; }
        AuthStatus Status { get; }
        DeviceType Device { get; }
        bool SpamFilter { get; }
        ExtendedUser Profile { get; }

        Task<bool> Write(IPacket packet);
        Task<bool> Write(IPacketMap packet);
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

        public string Email { get; private set; }
        public string Password { get; private set; }
        public AuthStatus Status { get; private set; }
        public DeviceType Device { get; private set; }
        public bool SpamFilter { get; private set; }
        public IRoleManager RoleManager => roleManager;
        public ISubProfiling SubProfiling => subProfiling;
        public ExtendedUser Profile => SubProfiling.Profile;

        private IPacketSerializer packetSerializer;
        private IPacketDeserializer packetDeserializer;
        private IPacketMapper packetMapper;
        private IPacketWatcher packetWatcher;
        private IPacketTemplates packetTemplates;
        private IZLibCompression compression;
        private IAuthenticationUtility authentication;
        private IPacketHandlerHub handlerHub;
        private ISubProfiling subProfiling;
        private IPluginManager pluginManager;
        private IRoleManager roleManager;

        private NetworkClient _client;

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
            this.subProfiling = subProfiling;
            this.pluginManager = pluginManager;
            this.roleManager = roleManager;

            _client = new NetworkClient(DefaultHost, DefaultPort);
            _client.OnDisconnected += (c) => OnDisconnected();
            _client.OnException += (e, n) => OnException(e, n);
            _client.OnDataReceived += (c, b) => this.packetDeserializer.ReadPacket(c, b);

            this.pluginManager.OnException += (e, n) => OnException(e, n);
            this.packetSerializer.OnException += (e, n) => OnException(e, n);
            this.handlerHub.OnException += (e, n) => OnException(e, n);
            this.packetWatcher.OnException += (e, n) => OnException(e, n);
            this.subProfiling.OnException += (e, n) => OnException(e, n);
            this.packetDeserializer.OnException += (e, n) => OnException(e, n);

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
