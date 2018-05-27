namespace PalApi.PacketTypes
{
    using Networking.Mapping;
    using Types;
    using Utilities;

    public class Response : IPacketMap
    {
        public string Command => "RESPONSE";

        [Header("WHAT")]
        public What What { get; set; }

        [Header("TYPE")]
        public Type Type { get; set; }

        [Header("MESG-ID")]
        public long MessageId { get; set; }

        [Payload]
        public string Message { get; set; }

        private Code? _code = null;

        public Code Code
        {
            get { return _code ?? (Type == Type.Code ? (Code)DataInputStream.GetLong(Message) : Types.Code.INTERNAL_CODE); }
            set
            {
                Type = Type.Code;
                _code = value;
            }
        }

        public override string ToString()
        {
            if (Type == Type.Code)
            {
                if (What == What.SUBSCRIBE_TO_GROUP)
                {
                    if (Code == Code.GROUP_ALREADY_EXISTS)
                        return "Alread in that group.";
                    if (Code == Code.INSUFFICIENT_PRIVILEGES)
                        return "My privileges aren't sufficient to join that group";
                    if (Code == Code.OK)
                        return "I joined the group.";
                    if (Code == Code.NO_SUCH_WHATEVER || Code == Code.GROUP_NOT_FOUND)
                        return "That group does not exists.";
                }
            }

            return $"{What.ToString().FromConstCase()} - {(Type == Type.Code ? Code.ToString().FromConstCase() : Message)}";
        }
    }
}
