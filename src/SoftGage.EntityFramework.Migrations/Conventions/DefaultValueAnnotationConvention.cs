using System;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Globalization;
using System.Linq;

namespace SoftGage.EntityFramework.Migrations.Conventions
{
    internal sealed class DefaultValueAnnotationConvention : AttributeToColumnAnnotationConvention<DefaultValueAttribute, string>
    {
        #region Constants
        public const string AnnotationName = "DefaultValue";
        #endregion

        #region Constructor
        public DefaultValueAnnotationConvention()
            : base(AnnotationName, (info, list) => Convert.ToString(list.First().Value, CultureInfo.InvariantCulture)) { }
        #endregion
    }
}
