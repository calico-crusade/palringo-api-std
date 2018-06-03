namespace PalApi.PacketTypes
{
    using Networking.Mapping;
    using Types;

    public class AdminAction : IPacketMap
    {
        public string Command => "GROUP ADMIN";

        [Header("SOURCE-ID")]
        public int SourceId { get; set; }

        [Header("TARGET-ID")]
        public int TargetId { get; set; }

        [Header("GROUP-ID")]
        public int GroupId { get; set; }

        [Header("ACTION")]
        public AdminActions Action { get; set; }

        public override string ToString()
        {
            return $"{Action.ToString()} - {SourceId} => {TargetId} ({GroupId})";
        }
    }
}
