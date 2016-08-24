# SoftGage Entity Framework Migrations Extensions
![current build status](https://travis-ci.org/DarkCompiled/entity-framework-migrations-extensions.svg?branch=develop)
Extensions for Entity Framework 6 migrations.  
This brings SQL Always Encrypted columns, SQL descriptions and SQL default values to Entity Framework 6.  

## How to Use?
It's very easy to use, you just need to use some classes as part of the configuration and you're done.

### Database Context
Extend you database context from `ExtendedDbContext` instead of `DbContext`:
```C#
public class SampleContext : ExtendedDbContext
{
    public virtual IDbSet<User> Users { get; set; }
}
```

### Migrations Configuration
If you would like to use the `Seed` mechanism to pre-populate database or to change the context key, you can extend your configuration class from `ExtendedConfiguration` instead of `DbMigrationsConfiguration`.  
This extended configuration enables Automatic Migrations by default (you may change that if you want), and registers the extended SQL generator.  
```C#
public class DbConfiguration : ExtendedConfiguration<SampleContext>
{
    public DbConfiguration()
    {
        // Set context key as you wish.
        ContextKey = "MyCompany.Data";
    }

    protected override void Seed(T context)
    {
	    // Pre-populate the database.
        context.Set<User>().AddOrUpdate(
                //user => user.Email, new User(...)
            );
    }
}
```

### Database Initialization
Then, use the configuration to initialize the database, just like you would normally do:
```C#
    Database.SetInitializer(new MigrateDatabaseToLatestVersion<SampleContext, DbConfiguration());
```

## What's new?
The following conventions become available:
- [`TableDescriptionAnnotationConvention`](#ColumnDescriptionAnnotationConvention) Allows configuring SQL descriptions on tables.
- [`ColumnDescriptionAnnotationConvention`](#ColumnDescriptionAnnotationConvention) Allows configuring SQL descriptions on columns.
- [`ColumnNonClusteredAnnotationConvention`](#ColumnNonClusteredAnnotationConvention) Allows configuring SQL Key column as Non-Clustered.
- [`ColumnEncryptedAnnotationConvention`](#ColumnEncryptedAnnotationConvention) Allows configuring SQL Always Encrypted columns.
- [`DefaultConstraintAnnotationConvention`](#DefaultConstraintAnnotationConvention) Allows configuring SQL column default value.

Context is created, by default, without the following conventions:
- `PluralizingTableNameConvention` I personally prefer singular names, but you may restore it if you like.

Context is created, by default, with the following settings disabled:
- `LazyLoadingEnabled` I may occasionally enable it, but I prefer not to have it enabled by default.

## TableDescriptionAnnotationConvention
Sets a SQL description on a table. This is especially useful to help database administrators to figure out what's the purpose of that table.  
To use it, just decorate an entity class with the annotation `Description`:
```C#
[Description("This table has users.")]
public class User
{
    //(...)
}
```

## ColumnDescriptionAnnotationConvention
Sets a SQL description on a column. This is especially useful to help database administrators to figure out what's the purpose of that column.  
To use it, just decorate an entity property with the annotation `Description`:
```C#
public class User
{
    [Description("UTC date when this user expires and, therefore, is prevented from logging in.")]
	public string ExpiresOn {get; set;}
}
```

## ColumnNonClusteredAnnotationConvention
Sometimes you don't want your primary key to be the Clustered Index. This enables you to set a primary key as Non-Clustered.  
To use it, just decorate an entity primary key with the annotation `NonClustered` and another entity property with a `Clustered` `Index`:
```C#
public class User
{
    [Key, NonClustered]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
	
    [Index(IsClustered = true, IsUnique = true)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClusteredId { get; set; }
}
```

## ColumnEncryptedAnnotationConvention
Sets a SQL column as [Always Encrypted]. This new feature instructs the driver to automatically encrypt and decrypt sensitive data based on an encryption key, never revealed to the database engine.  
Please notice that this feature requires Microsoft SQL Server 2016 or later.  
To use it, just decorate an entity property with the annotation `EncryptedWith`, and specify the key name to use.
You may optionally specify the desired [type of encryption] (Deterministic or Randomized) using the parameter `Type`.
```C#
public class User
{
    [EncryptedWith("MyKey", Type=EncryptionType.Randomized)]
	public string CreditCardNumber {get; set;}
}
```

## DefaultConstraintAnnotationConvention
Sets a SQL default values on a column. This is extremely useful when you don't want to care about some column values.  
To use it, just decorate an entity property with the annotation `DefaultConstraint` and `DatabaseGenerated` with `Computed` value:
```C#
public class User
{
    [DefaultConstraint("GETUTCDATE()")]
	[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedOn { get; set; }
}
```

## License
Apache License, Version 2.0

[Always Encrypted]: https://msdn.microsoft.com/en-us/library/mt163865.aspx
[type of encryption]: https://msdn.microsoft.com/library/mt459280.aspx#Anchor_2