using System;

namespace SoftGage.EntityFramework.Migrations.Annotations
{
    /// <summary>
    /// Preventes a Primary Key to create a Clustered Index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NonClusteredAttribute : Attribute { }
}
