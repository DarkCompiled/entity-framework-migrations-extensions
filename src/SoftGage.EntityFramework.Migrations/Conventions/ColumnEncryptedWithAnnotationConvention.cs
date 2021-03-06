﻿using SoftGage.EntityFramework.Migrations.Annotations;
using SoftGage.EntityFramework.Migrations.Configurations;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;

namespace SoftGage.EntityFramework.Migrations.Conventions
{
    internal sealed class ColumnEncryptedWithAnnotationConvention : AttributeToColumnAnnotationConvention<EncryptedWithAttribute, string>
    {
        #region Constants
        public const string AnnotationName = "EncryptedWith";
        #endregion

        #region Constructor
        public ColumnEncryptedWithAnnotationConvention()
            : base(AnnotationName, AnnotationFactory) { }
        #endregion

        #region Private Methods
        private static string AnnotationFactory(PropertyInfo propertyInfo, IList<EncryptedWithAttribute> encryptedAttributes)
        {
            var attribute = encryptedAttributes.First();

            var config = new EncryptedWithConfiguration
            {
                KeyName = attribute.KeyName,
                Type = attribute.Type
            };

            return config.Serialize();
        }
        #endregion
    }
}
