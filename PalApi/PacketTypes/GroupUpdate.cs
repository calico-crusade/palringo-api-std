namespace PalApi.PacketTypes
{
    using Networking.Mapping;
    using SubProfile.Parsing;
    using Types;

    public class GroupUpdate : IPacketMap
    {
        public string Command => "GROUP UPDATE";

        [Payload]
        public DataMap Payload { get; set; }

        public int UserId => Payload.GetValueInt("contact-id");
        public int GroupId => Payload.GetValueInt("group-id");
        public GroupUpdateType Type => (GroupUpdateType)Payload.GetValueInt("type");
    }
}
