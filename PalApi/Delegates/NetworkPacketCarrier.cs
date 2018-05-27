namespace PalApi.Delegates
{
    using Networking;

    public delegate void NetworkPacketCarrier(INetworkClient client, IPacket packet);
}
