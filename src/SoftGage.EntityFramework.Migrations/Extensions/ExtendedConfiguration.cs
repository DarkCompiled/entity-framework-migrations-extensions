using System.Data.Entity.Migrations;

namespace SoftGage.EntityFramework.Migrations.Extensions
{
    /// <summary>
    /// Base migrations configuration for <see cref="ExtendedDbContext"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtendedConfiguration<T> : DbMigrationsConfiguration<T>
        where T : ExtendedDbContext
    {
        /// <summary>
        /// Creates an instance of <see cref="ExtendedConfiguration{T}"/>.
        /// </summary>
        public ExtendedConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            //AutomaticMigrationDataLossAllowed = false;
            SetSqlGenerator("System.Data.SqlClient", new ExtendedSqlGenerator());
        }
    }
}
