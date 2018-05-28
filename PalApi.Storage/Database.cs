using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PalApi.Utilities.Storage
{
    using DatabaseTypes;

    public class Database
    {
        public static IDatabaseType DefaultDatabase { get; set; } = new SqliteDatabase();

        private IDatabaseType _dbType = null;

        public IDbConnection Connection => DbType.Connection;
        public IDatabaseType DbType => _dbType ?? DefaultDatabase;
        public Table Table { get; private set; }
        public Queries Queries { get; private set; }

        public Database(Type type) : this(type, DefaultDatabase) { }

        public Database(Type type, IDatabaseType database)
        {
            _dbType = database;
            Table = new Table(type, DbType);
            Queries = new Queries
            {
                Get = database.Get(Table),
                GetAll = database.GetAll(Table),
                Create = database.Create(Table),
                Delete = database.Delete(Table),
                Update = database.Update(Table),
                Insert = database.Insert(Table),
                InsertGetId = database.InsertGetId(Table)
            };
            Setup();
        }

        private void Setup()
        {
            ExecuteScalar<int>(Queries.Create);
        }

        public IEnumerable<T2> Query<T2>(string sql, object parameters = null)
        {
            using (var con = Connection)
            {
                con.Open();
                return con.Query<T2>(sql, parameters);
            }
        }

        public int Execute(string sql, object parameters = null)
        {
            using (var con = Connection)
            {
                con.Open();
                return con.Execute(sql, parameters);
            }
        }

        public T2 ExecuteScalar<T2>(string sql, object parameters = null)
        {
            using (var con = Connection)
            {
                con.Open();
                return con.ExecuteScalar<T2>(sql, parameters);
            }
        }
    }

    public class Database<T> : Database
    {
        public Database() : base(typeof(T)) { }

        public Database(IDatabaseType db) : base(typeof(T), db) { }
        
        public IEnumerable<T> Query(string sql, object parameters = null)
        {
            return Query<T>(sql, parameters);
        }
        
        public bool Insert(T item)
        {
            return Execute(Queries.Insert, item) > 0;
        }

        public bool Update(T item)
        {
            return Execute(Queries.Update, item) > 0;
        }

        public bool Delete(T item)
        {
            return Execute(Queries.Delete, item) > 0;
        }

        public IEnumerable<T> Get()
        {
            return Query<T>(Queries.GetAll);
        }
    }

    public class Database<TTable, TPrimaryKey> : Database<TTable>
    {
        public Database() : base() { }
        public Database(IDatabaseType db) : base(db) { }

        public TTable Get(TPrimaryKey key)
        {
            var pk = Table.Properties.FirstOrDefault(t => t.PrimaryKey);
            if (pk == null)
                throw new PrimaryKeyNotFoundException(Table);

            var p = new DynamicParameters();
            p.Add(pk.ParameterizedName, key);

            return Query<TTable>(Queries.Get, p).FirstOrDefault();
        }

        public TPrimaryKey InsertGetId(TTable item)
        {
            return ExecuteScalar<TPrimaryKey>(Queries.InsertGetId, item);
        }
    }
}
