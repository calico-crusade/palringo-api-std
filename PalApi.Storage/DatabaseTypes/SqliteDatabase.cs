using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PalApi.Utilities.Storage.DatabaseTypes
{
    public class SqliteDatabase : IDatabaseType
    {
        private string ConnectionString;
        public SqliteDatabase(string source = "PalApi.db")
        {
            ConnectionString = "Data Source=" + source;
        }

        public IDbConnection Connection => new SqliteConnection(ConnectionString);

        public string EncapsulationCharFront => "[";

        public string EncapsulationCharBack => "]";

        public string ParameterizationCharacter => "@";

        private string Encapsulate(string thing)
        {
            if (!thing.StartsWith(EncapsulationCharFront))
                thing = EncapsulationCharFront + thing;

            if (!thing.EndsWith(EncapsulationCharBack))
                thing += EncapsulationCharBack;

            return thing;
        }

        private string PkWhere(Table table)
        {
            var pk = table.Properties.FirstOrDefault(t => t.PrimaryKey);
            if (pk == null)
                throw new PrimaryKeyNotFoundException(table);

            return $"WHERE {Encapsulate(pk.Name)} = {pk.ParameterizedName}";
        }

        public string Create(Table table)
        {
            var query = $"CREATE TABLE IF NOT EXISTS {Encapsulate(table.Name)} (";
            foreach (var prop in table.Properties)
            {
                var p = $"\r\n\t{Encapsulate(prop.Name)} {prop.DataType}";

                if (prop.DataLength != null && prop.DataLength > 0)
                    p += "(" + prop.DataLength.Value + ")";

                if (prop.PrimaryKey || prop.AutoIncrement)
                    p += " PRIMARY KEY";
                else if (!prop.CanBeNull)
                    p += " NOT NULL";
                else if (prop.Unique)
                    p += " UNIQUE";

                query += p + ",";
            }

            return query.Trim(',') + "\r\n)";
        }

        public string Delete(Table table)
        {
            if (!table.Obsoletable)
                return $"DELETE FROM {Encapsulate(table.Name)} {PkWhere(table)}";

            return $@"UPDATE {Encapsulate(table.Name)} 
SET 
    {Encapsulate("ObsoletedBy")} = {(ParameterizationCharacter + "ObsoletedBy")},
    {Encapsulate("ObsoletedOn")} = {(ParameterizationCharacter + "ObsoletedOn")}
{PkWhere(table)}";
        }

        public string Get(Table table)
        {
            if (!table.Obsoletable)
                return $"SELECT * FROM {Encapsulate(table.Name)} {PkWhere(table)}";

            return $@"SELECT * FROM {Encapsulate(table.Name)} 
{PkWhere(table)} AND {Encapsulate("ObsoletedOn")} IS NULL";
        }

        public string GetAll(Table table)
        {
            if (!table.Obsoletable)
                return $"SELECT * FROM {Encapsulate(table.Name)}";

            return $@"SELECT * FROM {Encapsulate(table.Name)}
WHERE {Encapsulate("ObsoletedOn")} IS NULL";
        }

        public string Insert(Table table)
        {
            var query = $"INSERT INTO {Encapsulate(table.Name)} (";
            var values = $"\r\n) VALUES (";

            foreach (var prop in table.Properties)
            {
                if (prop.AutoIncrement || prop.PrimaryKey)
                    continue;

                query += $"\r\n\t{Encapsulate(prop.Name)},";
                values += $"\r\n\t{prop.ParameterizedName},";
            }

            return query.TrimEnd(',') + values.TrimEnd(',') + "\r\n)";
        }

        public string InsertGetId(Table table)
        {
            var query = Insert(table);
            return query + ";\r\nSELECT last_insert_rowid();";
        }

        public string Update(Table table)
        {
            var query = $"UPDATE {Encapsulate(table.Name)}\r\nSET";

            foreach (var prop in table.Properties)
            {
                if (prop.PrimaryKey || prop.AutoIncrement)
                    continue;

                if (table.Auditable && (prop.Name == "CreatedBy" || prop.Name == "CreatedOn"))
                    continue;

                query += $"\r\n\t{Encapsulate(prop.Name)} = {prop.ParameterizedName},";
            }

            return query.TrimEnd(',') + $"\r\n{PkWhere(table)}";
        }

        public string ResolvePropertyType(Type type)
        {
            if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
                return "REAL";

            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(bool) || type.IsEnum)
                return "INTEGER";

            if (type == typeof(byte[]) || type == typeof(ICollection<byte>))
                return "BLOB";

            return "TEXT";
        }
    }
}
