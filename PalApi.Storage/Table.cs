using System;
using System.Reflection;
using System.Linq;

namespace PalApi.Utilities.Storage
{
    public class Table
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public Property[] Properties { get; private set; }
        public bool Auditable { get; private set; }
        public bool Obsoletable { get; private set; }

        public Table(Type type, IDatabaseType db)
        {
            Type = type;
            Name = type.GetCustomAttribute<NameAttribute>()?.Name ?? type.Name;
            Properties = type.GetRuntimeProperties()
                .Select(t => new Property(t, db))
                .Where(t => !t.Ignore)
                .ToArray();

            Auditable = type.IsSubclassOf(typeof(Auditable));
            Obsoletable = type.IsSubclassOf(typeof(Obsoletable));
        }
    }
}
