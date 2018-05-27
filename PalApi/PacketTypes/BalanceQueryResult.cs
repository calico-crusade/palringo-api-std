namespace PalApi.PacketTypes
{
    using Networking.Mapping;

    public class BalanceQueryResult : IPacketMap
    {
        public string Command => "BALANCE QUERY RESULT";

        [Payload]
        public byte[] Payload { get; set; }
    }
}
