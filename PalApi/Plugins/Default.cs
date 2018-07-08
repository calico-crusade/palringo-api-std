using PalApi.Types;
using System;

namespace PalApi.Plugins
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class Default : Attribute
    {
        public string Roles { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Group | MessageType.Private;
    }
}
