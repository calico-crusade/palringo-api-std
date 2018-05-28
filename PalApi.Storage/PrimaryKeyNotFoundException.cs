using System;

namespace PalApi.Utilities.Storage
{
    public class PrimaryKeyNotFoundException : Exception
    {
        public PrimaryKeyNotFoundException(Table table) : base("No Primary Key specified on " + table.Name + ". Please mark/create a primary key using the [PrimaryKey] attribute.") { }
    }
}
