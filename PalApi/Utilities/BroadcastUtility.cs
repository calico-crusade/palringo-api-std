using System;

namespace PalApi.Utilities
{
    using Delegates;
    using Networking;
    using PacketTypes;

    public interface IBroadcastUtility
    {
        event MappedPacketCarrier<Throttle> Throttle;
        event MappedPacketCarrier<AdminAction> AdminAction;
        event MappedPacketCarrier<GroupUpdate> GroupUpdate;
        event MappedPacketCarrier<LoginFailed> LoginFailed;
        event MappedPacketCarrier<Message> Message;
        event VoidCarrier Disconnected;
        event ExceptionCarrier Exception;
        event PacketCarrier PacketSent;
        event PacketCarrier PacketReceived;
        event PacketCarrier UnhandledPacket;
    }

    public class BroadcastUtility : IBroadcastUtility
    {
        #region Throttle
        public event MappedPacketCarrier<Throttle> Throttle = delegate { };
        public void BroadcastThrottle(IPalBot bot, Throttle throttle) => Throttle(bot, throttle);
        #endregion

        #region Admin Action
        public event MappedPacketCarrier<AdminAction> AdminAction = delegate { };
        public void BroadcastAdminAction(IPalBot bot, AdminAction action) => AdminAction(bot, action);
        #endregion

        #region Group Update
        public event MappedPacketCarrier<GroupUpdate> GroupUpdate = delegate { };
        public void BroadcastGroupUpdate(IPalBot bot, GroupUpdate update) => GroupUpdate(bot, update);
        #endregion

        #region Login Failed
        public event MappedPacketCarrier<LoginFailed> LoginFailed = delegate { };
        public void BroadcastLoginFailed(IPalBot bot, LoginFailed failed) => LoginFailed(bot, failed);
        #endregion

        #region Message
        public event MappedPacketCarrier<Message> Message = delegate { };
        public void BroadcastMessage(IPalBot bot, Message message) => Message(bot, message);
        #endregion

        #region Disconnected
        public event VoidCarrier Disconnected = delegate { };
        public void BroadcastDisconnected() => Disconnected();
        #endregion

        #region Exception
        public event ExceptionCarrier Exception = delegate { };
        public void BroadcastException(Exception exception, string note) => Exception(exception, note);
        #endregion

        #region Packet Sent
        public event PacketCarrier PacketSent = delegate { };
        public void BroadcastPacketSent(IPacket packet) => PacketSent(packet);
        #endregion

        #region Packet Received
        public event PacketCarrier PacketReceived = delegate { };
        public void BroadcastPacketReceived(IPacket packet) => PacketReceived(packet);
        #endregion

        #region Unhanled Packet
        public event PacketCarrier UnhandledPacket = delegate { };
        public void BroadcastUnhandledPacket(IPacket packet) => UnhandledPacket(packet);
        #endregion

        #region Packet Parsed
        public event NetworkPacketCarrier PacketParsed = delegate { };
        public void BroadcastPacketParsed(INetworkClient client, IPacket packet) => PacketParsed(client, packet);
        #endregion
    }
}
