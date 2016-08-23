# SoftGage Entity Framework Migrations Extensions
Extensions for Entity Framework 6 migrations.  
The following extended conventions are available:
- `TableDescriptionAnnotationConvention` Allows configuring SQL descriptions on tables.
- `ColumnDescriptionAnnotationConvention` Allows configuring SQL descriptions on columns.
- `ColumnNonClusteredAnnotationConvention` Allows configuring a Key column as Non-Clustered.
- `ColumnEncryptedAnnotationConvention` Allows configuring a SQL Always Encrypted column.
- `DefaultConstraintAnnotationConvention` Allows configuring SQL column default value.

Context is created, by default, without the following conventions:
- `PluralizingTableNameConvention` I personally prefer singular names, but you may restore it if you like.

Context is created, by default, with the following settings disabled:
- `LazyLoadingEnabled` I may occasionally enable it, but I prefer not to have it enabled by default.
