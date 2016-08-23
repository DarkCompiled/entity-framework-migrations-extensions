namespace SoftGage.EntityFramework.Migrations.Annotations
{
    /// <summary>
    /// Use deterministic encryption for columns that will be search parameters or grouping parameters, for example a government ID number.
    /// Use randomized encryption, for data such as a credit card number, which is not grouped with other records, or used to join tables, and which is not
    /// searched for because you use other columns (such as a transaction number) to find the row which contains the encrypted column of interest.
    /// Columns must be of a qualifying data type.
    /// </summary>
    public enum EncryptionType
    {
        /// <summary>
        /// Deterministic encryption uses a method which always generates the same encrypted value for any given plain text value.
        /// Using deterministic encryption allows searching using equality comparison, grouping, and joining tables using equality joins
        /// based on encrypted values, but can also allow unauthorized users to guess information about encrypted values by examining
        /// patterns in the encrypted column.
        /// Joining two tables on columns encrypted deterministically is only possible if both columns are encrypted using the same column encryption key.
        /// Deterministic encryption must use a column collation with a binary2 sort order for character columns.
        /// </summary>
        Deterministic,
        /// <summary>
        /// Randomized encryption uses a method that encrypts data in a less predictable manner.
        /// Randomized encryption is more secure, but prevents equality searches, grouping, and joining on encrypted columns.
        /// Columns using randomized encryption cannot be indexed.
        /// </summary>
        Randomized
    }
}
