using SoftGage.EntityFramework.Migrations.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace SoftGage.EntityFramework.Migrations.Conventions
{
    internal sealed class ColumnNonClusteredAnnotationConvention : AttributeToColumnAnnotationConvention<NonClusteredAttribute, string>
    {
        #region Constructor
        public ColumnNonClusteredAnnotationConvention()
            : base("NonClustered", (info, list) => string.Empty) { }
        #endregion
    }
}
