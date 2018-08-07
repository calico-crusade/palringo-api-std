using System;
using PalApi.Types;

namespace PalApi.Plugins.Comparitors
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class BaseCommand : Attribute, ICommand
    {
        public virtual string Comparitor { get; }
        public virtual MessageType MessageType { get; set; }
        public virtual string Roles { get; set; }
        public virtual string Grouping { get; set; }

        public BaseCommand(string comp)
        {
            Comparitor = comp;
        }
    }
}
