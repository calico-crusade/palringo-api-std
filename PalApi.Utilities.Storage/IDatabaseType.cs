using System;
using System.Data;

namespace PalApi.Utilities.Storage
{
    public interface IDatabaseType
    {
        IDbConnection Connection { get; }
        string EncapsulationCharFront { get; }
        string EncapsulationCharBack { get; }
        string ParameterizationCharacter { get; }
        string ResolvePropertyType(Type type);

        string Create(Table table);
        string Insert(Table table);
        string InsertGetId(Table table);
        string Delete(Table table);
        string Update(Table table);
        string Get(Table table);
        string GetAll(Table table);
    }
}
