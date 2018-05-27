using System;

namespace PalApi.Utilities.Storage
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class DataTypeAttribute : Attribute
    {
        public string DataType { get; private set; }
        public int Length { get; set; } = -1;

        public DataTypeAttribute(string datatype)
        {
            DataType = datatype;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class CanBeNullAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrementAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultExpressionAttribute : Attribute
    {
        public string Expression { get; private set; }

        public DefaultExpressionAttribute(string expression)
        {
            Expression = expression;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class NameAttribute : Attribute
    {
        public string Name { get; private set; }

        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}
