using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace SoftGage.EntityFramework.Migrations.Conventions
{
    internal sealed class TableDescriptionAnnotationConvention : AttributeToTableAnnotationConvention<DescriptionAttribute, string>
    {
        #region Constructor
        public TableDescriptionAnnotationConvention()
            : base("Description", (info, list) => list.First().Description) { }
        #endregion
    }
}