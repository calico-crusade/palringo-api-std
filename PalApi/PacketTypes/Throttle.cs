namespace PalApi.PacketTypes
{
    using Networking.Mapping;

    public class Throttle : IPacketMap
    {
        public string Command => "THROTTLE";

        [Header("DURATION")]
        public int Duration { get; set; }

        [Header("REASON")]
        public string Reason { get; set; }
    }
}
