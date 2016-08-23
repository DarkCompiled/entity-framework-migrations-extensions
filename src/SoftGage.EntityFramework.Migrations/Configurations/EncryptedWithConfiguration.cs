using SoftGage.EntityFramework.Migrations.Annotations;
using System;

namespace SoftGage.EntityFramework.Migrations.Configurations
{
    [Serializable]
    internal sealed class EncryptedWithConfiguration
    {
        #region Properties
        public string KeyName { get; set; }
        public EncryptionType Type { get; set; }
        #endregion

        #region Public methods
        public string Serialize()
        {
            return SimpleSerializer.Serialize(KeyName, (int)Type);
        }
        public static EncryptedWithConfiguration Deserialize(string serialized)
        {
            string first;
            int second;
            SimpleSerializer.Deserialize(serialized, out first, out second);
            return new EncryptedWithConfiguration
            {
                KeyName = first,
                Type = (EncryptionType)second
            };
        }
        #endregion
    }
}
