namespace PalApi.PacketTypes
{
    using Networking.Mapping;
    using SubProfile.Parsing;

    public class SubProfileQueryResult : IPacketMap
    {
        public string Command => "SUB PROFILE QUERY RESULT";

        [Payload]
        public DataMap Data { get; set; }

        public int Id => Data.ContainsKey("sub-id") ? Data.GetValueInt("sub-id") : 0;
    }
}
