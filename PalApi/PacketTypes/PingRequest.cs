namespace PalApi.PacketTypes
{
    using Networking.Mapping;

    public class PingRequest : IPacketMap
    {
        public string Command => "P";
    }
}
