using System;

namespace PalApi.Plugins
{
    using Types;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class Command : Attribute
    {
        public string Cmd { get; private set; } = null;
        public MessageType MessageType { get; set; } = MessageType.Group | MessageType.Private;
        public string Roles { get; set; } = null;
        public string Grouping { get; set; } = null;

        public Command(string cmd)
        {
            this.Cmd = cmd;
        }
    }
}
