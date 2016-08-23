using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace SoftGage.EntityFramework.Migrations.Conventions
{
    internal sealed class ColumnDescriptionAnnotationConvention : AttributeToColumnAnnotationConvention<DescriptionAttribute, string>
    {
        #region Constructor
        public ColumnDescriptionAnnotationConvention()
            : base("Description", (info, list) => list.First().Description) { }
        #endregion
    }
}
