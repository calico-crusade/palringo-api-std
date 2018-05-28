namespace PalApi.Utilities.Storage
{
    public abstract class IdTable : Obsoletable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
