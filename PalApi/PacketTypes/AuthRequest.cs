namespace PalApi.PacketTypes
{
    using Networking.Mapping;

    public class AuthRequest : IPacketMap
    {
        public string Command => "AUTH";

        [Payload]
        public byte[] Key { get; set; }
    }
}
