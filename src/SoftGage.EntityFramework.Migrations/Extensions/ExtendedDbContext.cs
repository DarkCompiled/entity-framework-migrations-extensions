using SoftGage.EntityFramework.Migrations.Conventions;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace SoftGage.EntityFramework.Migrations.Extensions
{
    /// <summary>
    /// Database context with extended settings.
    /// This context disables the following configurations:
    /// <list type="bullet">
    ///   <item><description>LazyLoadingEnabled</description></item>
    /// </list>
    /// Removes the following conventions:
    /// <list type="bullet">
    ///   <item><description><see cref="PluralizingTableNameConvention"/>.</description></item>
    /// </list>
    /// Extends with the following conventions:
    /// <list type="bullet">
    ///   <item><description><see cref="TableDescriptionAnnotationConvention"/>,</description></item>
    ///   <item><description><see cref="ColumnDescriptionAnnotationConvention"/>,</description></item>
    ///   <item><description><see cref="ColumnNonClusteredAnnotationConvention"/>,</description></item>
    /// <item><description><see cref="ColumnEncryptedWithAnnotationConvention"/>,</description></item>
    ///   <item><description><see cref="DefaultConstraintAnnotationConvention"/>.</description></item>
    ///   <item><description><see cref="DefaultValueAnnotationConvention"/>.</description></item>
    /// </list>
    /// </summary>
    public class ExtendedDbContext : DbContext
    {
        #region Constructors
        /// <summary>
        /// Initializes an instance of <see cref="ExtendedDbContext"/>.
        /// <remarks>
        /// Uses the default connection string name "DefaultConnection".
        /// </remarks>
        /// </summary>
        public ExtendedDbContext()
            : base("DefaultConnection")
        {
            Configuration.LazyLoadingEnabled = false;
        }
        /// <summary>
        /// Initializes an instance of <see cref="ExtendedDbContext"/>.
        /// </summary>
        /// <param name="nameOrConnectionString">Name or connection string.</param>
        public ExtendedDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString ?? "DefaultConnection")
        {
            Configuration.LazyLoadingEnabled = false;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        /// before the model has been locked down and used to initialize the context.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created. </param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Add<TableDescriptionAnnotationConvention>();
            modelBuilder.Conventions.Add<ColumnDescriptionAnnotationConvention>();
            modelBuilder.Conventions.Add<ColumnNonClusteredAnnotationConvention>();
            modelBuilder.Conventions.Add<ColumnEncryptedWithAnnotationConvention>();
            modelBuilder.Conventions.Add<DefaultConstraintAnnotationConvention>();
            modelBuilder.Conventions.Add<DefaultValueAnnotationConvention>();
        }
        #endregion
    }
}
