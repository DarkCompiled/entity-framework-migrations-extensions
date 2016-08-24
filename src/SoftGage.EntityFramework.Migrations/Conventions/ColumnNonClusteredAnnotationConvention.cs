using SoftGage.EntityFramework.Migrations.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace SoftGage.EntityFramework.Migrations.Conventions
{
    internal sealed class ColumnNonClusteredAnnotationConvention : AttributeToColumnAnnotationConvention<NonClusteredAttribute, string>
    {
        #region Constants
        public const string AnnotationName = "NonClustered";
        #endregion

        #region Constructor
        public ColumnNonClusteredAnnotationConvention()
            : base(AnnotationName, (info, list) => string.Empty) { }
        #endregion
    }
}
