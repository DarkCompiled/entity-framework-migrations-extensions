using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace SoftGage.EntityFramework.Migrations.Conventions
{
    internal sealed class TableDescriptionAnnotationConvention : AttributeToTableAnnotationConvention<DescriptionAttribute, string>
    {
        #region Constants
        public const string AnnotationName = "Description";
        #endregion

        #region Constructor
        public TableDescriptionAnnotationConvention()
            : base(AnnotationName, (info, list) => list.First().Description) { }
        #endregion
    }
}