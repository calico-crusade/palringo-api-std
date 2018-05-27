using System;
using System.Reflection;

namespace PalApi.Utilities.Storage
{
    public class Property
    {
        public bool Ignore { get; private set; }
        public bool PrimaryKey { get; private set; }
        public string DataType { get; private set; }
        public int? DataLength { get; private set; }
        public bool CanBeNull { get; private set; }
        public bool AutoIncrement { get; private set; }
        public bool Unique { get; private set; }
        public string DefaultExpression { get; private set; }
        public Type Type { get; private set; }
        public PropertyInfo Info { get; private set; }
        public string Name { get; private set; }
        public string ParameterizedName { get; private set; }
    
        public Property(PropertyInfo type, IDatabaseType db)
        {
            Info = type;
            Type = Nullable.GetUnderlyingType(type.PropertyType) ?? type.PropertyType;
            Ignore = IsDefined<IgnoreAttribute>();
            PrimaryKey = IsDefined<PrimaryKeyAttribute>();
            CanBeNull = IsDefined<CanBeNullAttribute>();
            AutoIncrement = IsDefined<AutoIncrementAttribute>();
            Unique = IsDefined<UniqueAttribute>();
            DefaultExpression = GetAttribute<DefaultExpressionAttribute>()?.Expression ?? "";
            Name = GetAttribute<NameAttribute>()?.Name ?? type.Name;
            ParameterizedName = db.ParameterizationCharacter + Name;

            var dt = GetAttribute<DataTypeAttribute>();
            if (dt != null)
            {
                DataType = dt.DataType;
                DataLength = dt.Length <= 0 ? null : (int?)dt.Length;
                return;
            }

            DataType = db.ResolvePropertyType(Type);
            DataLength = null;
        }

        private bool IsDefined<T>()
        {
            return Attribute.IsDefined(Info, typeof(T));
        }

        private T GetAttribute<T>() where T : Attribute
        {
            if (!IsDefined<T>())
                return null;

            return Info.GetCustomAttribute<T>();
        }
    }
}
