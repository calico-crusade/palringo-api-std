using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PalApi.Utilities.Storage.DatabaseTypes
{
    public class MySqlDatabase : IDatabaseType
    {
        private string ConnectionString;
        public MySqlDatabase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IDbConnection Connection => new MySqlConnection(ConnectionString);

        public string EncapsulationCharFront => "`";

        public string EncapsulationCharBack => EncapsulationCharFront;

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

                if (!string.IsNullOrEmpty(prop.DefaultExpression))
                    p += " NOT NULL DEFAULT " + prop.DefaultExpression;
                else if (prop.AutoIncrement)
                    p += " NOT NULL AUTO_INCREMENT";
                else if (!prop.CanBeNull)
                    p += " NOT NULL";
                else
                    p += " DEFAULT NULL";

                query += p + ",";
            }

            foreach (var prop in table.Properties)
            {
                if (prop.Unique)
                    query += "\r\n\tUNIQUE (" + Encapsulate(prop.Name) + "),";

                if (prop.PrimaryKey)
                    query += "\r\n\tPRIMARY KEY (" + Encapsulate(prop.Name) + "),";
            }

            return query.Trim(',') + "\r\n) ENGINE=InnoDB DEFAULT CHARSET=utf8";
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
            return query + ";\r\nSELECT LAST_INSERT_ID();";
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
            if (type == typeof(sbyte))
                return "TINYINT";

            if (type == typeof(byte))
                return "TINYINT UNSIGNED";

            if (type == typeof(short))
                return "SMALLINT";

            if (type == typeof(ushort))
                return "SMALLINT UNSIGNED";

            if (type == typeof(int) || type.IsEnum)
                return "INT";

            if (type == typeof(uint))
                return "INT UNSIGNED";

            if (type == typeof(long))
                return "BIGINT";

            if (type == typeof(ulong))
                return "BIGINT UNSIGNED";

            if (type == typeof(DateTime))
                return "DATETIME";

            if (type == typeof(float))
                return "FLOAT";

            if (type == typeof(double))
                return "DOUBLE";

            if (type == typeof(decimal))
                return "DECIMAL";

            if (type == typeof(byte[]) || type == typeof(ICollection<byte>))
                return "LONGBLOB";

            if (type == typeof(bool))
                return "BIT";

            return "LONGTEXT";
        }
    }
}
