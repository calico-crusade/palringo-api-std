using System;

namespace PalApi.Utilities.Storage
{
    public abstract class Auditable
    {
        public static string DefaultSystemName = "System";
        public virtual string CreatedBy { get; set; } = DefaultSystemName;
        [DefaultExpression("CURRENT_TIMESTAMP")]
        public virtual DateTime CreatedOn { get; set; } = DateTime.Now;
        public virtual string ModifiedBy { get; set; } = DefaultSystemName;
        [DefaultExpression("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")]
        public virtual DateTime ModifiedOn { get; set; } = DateTime.Now;
    }
}
