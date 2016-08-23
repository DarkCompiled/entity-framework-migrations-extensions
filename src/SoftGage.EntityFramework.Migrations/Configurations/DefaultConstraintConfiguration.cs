using System;

namespace SoftGage.EntityFramework.Migrations.Configurations
{
    [Serializable]
    internal sealed class DefaultConstraintConfiguration
    {
        #region Properties
        public string Name { get; set; }
        public string Value { get; set; }
        #endregion

        #region Public methods
        public string Serialize()
        {
            return SimpleSerializer.Serialize(Name, Value);
        }
        public static DefaultConstraintConfiguration Deserialize(string serialized)
        {
            string first, second;
            SimpleSerializer.Deserialize(serialized, out first, out second);
            return new DefaultConstraintConfiguration
            {
                Name = first,
                Value = second
            };
        }
        #endregion
    }
}
