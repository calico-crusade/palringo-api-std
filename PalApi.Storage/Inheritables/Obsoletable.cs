using System;

namespace PalApi.Utilities.Storage
{
    public abstract class Obsoletable : Auditable
    {
        [CanBeNull]
        public string ObsoletedBy { get; set; } = null;
        [CanBeNull]
        public DateTime? ObsoletedOn { get; set; } = null;
    }
}
