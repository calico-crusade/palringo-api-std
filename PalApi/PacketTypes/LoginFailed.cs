namespace PalApi.PacketTypes
{
    using Networking.Mapping;

    public class LoginFailed : IPacketMap
    {
        public string Command => "LOGON FAILED";

        [Header("REASON")]
        public string Reason { get; set; }

        [Payload]
        public string Payload { get; set; }
    }
}
