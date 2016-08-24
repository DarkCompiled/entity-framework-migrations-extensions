using SoftGage.EntityFramework.Migrations.Annotations;
using SoftGage.EntityFramework.Migrations.Configurations;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;

namespace SoftGage.EntityFramework.Migrations.Conventions
{
    internal sealed class DefaultConstraintAnnotationConvention : AttributeToColumnAnnotationConvention<DefaultConstraintAttribute, string>
    {
        #region Constants
        public const string AnnotationName = "DefaultConstraint";
        #endregion

        #region Constructor
        public DefaultConstraintAnnotationConvention()
            : base(AnnotationName, AnnotationFactory) { }
        #endregion

        #region Private Methods
        private static string AnnotationFactory(PropertyInfo propertyInfo, IList<DefaultConstraintAttribute> defaultValueAttributes)
        {
            var attribute = defaultValueAttributes.First();

            var config = new DefaultConstraintConfiguration
            {
                Name = attribute.Name,
                Value = attribute.Value
            };

            return config.Serialize();
        }
        #endregion
    }
}
