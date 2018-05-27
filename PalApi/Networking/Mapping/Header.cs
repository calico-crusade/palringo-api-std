using System;

namespace PalApi.Networking.Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Header : Attribute
    {
        public string Name { get; private set; }

        public Header(string name)
        {
            this.Name = name;
        }
    }
}
