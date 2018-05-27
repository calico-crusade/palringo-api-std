namespace PalApi.Delegates
{
    using Networking;

    public delegate void NetworkDataCarrier(INetworkClient client, byte[] data);
}
