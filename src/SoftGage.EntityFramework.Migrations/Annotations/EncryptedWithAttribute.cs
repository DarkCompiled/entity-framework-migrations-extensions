using System;

namespace SoftGage.EntityFramework.Migrations.Annotations
{
    /// <summary>
    /// Specifies encrypting columns by using the Always Encrypted feature.
    /// <remarks>
    /// Please notice that this feature is only supported by Microsoft SQL Server 2016 onwards.
    /// Please visit https://msdn.microsoft.com/en-us/library/mt163865.aspx for more information.
    /// </remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EncryptedWithAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Specifies the column encryption key to use.
        /// </summary>
        public string KeyName { get; set; }
        /// <summary>
        /// Specifies the encryption type to use.
        /// </summary>
        public EncryptionType Type { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of <see cref="EncryptedWithAttribute"/>.
        /// </summary>
        /// <param name="keyName">Column encryption key to use.</param>
        public EncryptedWithAttribute(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName)) throw new ArgumentException("Please provide a non-empty key name.", nameof(keyName));
            KeyName = keyName;
        }
        #endregion
    }
}
