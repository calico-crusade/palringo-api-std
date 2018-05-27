namespace PalApi.PacketTypes
{
    using Networking.Mapping;

    public class SubProfilePacket : IPacketMap
    {
        public string Command => "SUB PROFILE";

        [Header("IV")]
        public int? IV { get; set; }

        [Header("RK")]
        public int? RK { get; set; }

        [Payload]
        public byte[] Data { get; set; }
    }
}
