using System;

namespace SoftGage.EntityFramework.Migrations.Annotations
{
    /// <summary>
    /// Allows to set a default value of a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DefaultConstraintAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Constraint name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Default value.
        /// </summary>
        public string Value { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of <see cref="DefaultConstraintAttribute"/>.
        /// </summary>
        /// <param name="value">Default value.</param>
        public DefaultConstraintAttribute(string value)
        {
            Value = value;
        }
        #endregion
    }
}
