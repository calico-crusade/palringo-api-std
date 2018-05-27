using System;

namespace PalApi.Plugins
{
    using Types;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class Command : Attribute
    {
        public string Cmd { get; private set; }
        public MessageType MessageType { get; set; } = MessageType.Group | MessageType.Private;
        public string Roles { get; set; }

        public Command(string cmd)
        {
            this.Cmd = cmd;
        }
    }
}
