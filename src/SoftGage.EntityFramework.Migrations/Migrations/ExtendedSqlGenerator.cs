using SoftGage.EntityFramework.Migrations.Configurations;
using SoftGage.EntityFramework.Migrations.Conventions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;

namespace SoftGage.EntityFramework.Migrations
{
    /// <summary>
    /// Extended SQL Server Migration - SQL Generator.
    /// Extended features:
    /// <list type="bullet">
    ///   <item><description>Descriptions on table objects.</description></item>
    ///   <item><description>Descriptions on column objects.</description></item>
    ///   <item><description>Default constraints.</description></item>
    ///   <item><description>Non-clustered indexes.</description></item>
    ///   <item><description>Always-Encrypted columns.</description></item>
    /// </list>
    /// </summary>
    internal sealed class ExtendedSqlGenerator : SqlServerMigrationSqlGenerator
    {
        #region Constants
        private const string SqlAddNamedConstraint = @"ALTER TABLE {0} ADD CONSTRAINT {2} DEFAULT {3} FOR [{1}];";
        private const string SqlAddUnnamedConstraint = @"ALTER TABLE {0} ADD DEFAULT {2} FOR [{1}];";
        private const string SqlDropNamedConstraint = @"ALTER TABLE {0} DROP CONSTRAINT {1};";
        private const string SqlDropUnnamedConstraint = @"DECLARE @Command NVARCHAR(1000);SELECT @Command = 'ALTER TABLE ' + '{0}' + ' DROP CONSTRAINT ' + d.name FROM sys.tables t JOIN sys.default_constraints d ON d.parent_object_id = t.object_id JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id WHERE t.name = '{1}' AND c.name = '{2}';IF(@Command IS NOT NULL) EXECUTE(@Command);";
        private static readonly char[] Separators = { '.' };
        #endregion

        #region Overrides
        protected override void Generate(AlterColumnOperation alterColumnOperation)
        {
            base.Generate(alterColumnOperation);
            var annotations = alterColumnOperation.Column.Annotations;

            // Retrieve names.
            var tableName = alterColumnOperation.Table;
            var columnName = alterColumnOperation.Column.Name;

            // Do SQL generation for column using annotation value as appropriate.
            GenerateColumnDefaultConstraint(annotations, tableName, columnName);

            // Column encryption process.
            UnsupportedColumnEncryped(annotations, tableName, columnName);

            // Do SQL generation for column using annotation value as appropriate.
            GenerateColumnDescription(annotations, tableName, columnName);
        }
        protected override void Generate(AddColumnOperation addColumnOperation)
        {
            PrepareColumnDefaultConstraint(addColumnOperation);

            base.Generate(addColumnOperation);
            var annotations = addColumnOperation.Column.Annotations;

            // Retrieve names.
            var tableName = addColumnOperation.Table;
            var columnName = addColumnOperation.Column.Name;

            // Do SQL generation for column using annotation value as appropriate.
            GenerateColumnDescription(annotations, tableName, columnName);
        }
        protected override void Generate(CreateTableOperation createTableOperation)
        {
            // Retrieve columns.
            var columns = createTableOperation.Columns;

            // Check for non-clustered primary keys.
            var keyColumns = createTableOperation.PrimaryKey.Columns;
            if (createTableOperation.PrimaryKey.IsClustered)
            {
                for (int i = 0, len = keyColumns.Count; i < len; i++)
                {
                    // Retrieve column annotations.
                    var keyColumn = keyColumns[i];
                    var columnAnnotations = columns.First(c => c.Name.Equals(keyColumn)).Annotations;

                    // Obtain annotation info for description.
                    AnnotationValues columnAnnotationValues;
                    columnAnnotations.TryGetValue(ColumnNonClusteredAnnotationConvention.AnnotationName, out columnAnnotationValues);

                    // Do SQL generation for column using annotation value as appropriate.
                    if (columnAnnotationValues != null)
                    {
                        createTableOperation.PrimaryKey.IsClustered = false;
                        break;
                    }
                }
            }

            // Generate base SQL.
            base.Generate(createTableOperation);

            // Retrieve table name.
            var tableName = createTableOperation.Name;

            // Retrieve table annotations.
            var tableAnnotations = createTableOperation.Annotations;

            // Create table description.
            GenerateTableDescription(tableAnnotations, tableName);

            // Iterate on every column.
            for (int i = 0, len = columns.Count; i < len; i++)
            {
                // Retrieve column annotations.
                var column = columns[i];
                var columnAnnotations = column.Annotations;

                // Retrieve names.
                var columnName = column.Name;

                // Do SQL generation for description annotation.
                GenerateColumnDescription(columnAnnotations, tableName, columnName);
            }
        }
        protected override void Generate(AlterTableOperation alterTableOperation)
        {
            base.Generate(alterTableOperation);

            // Retrieve table name.
            var tableName = alterTableOperation.Name;

            // Retrieve table annotations.
            var tableAnnotations = alterTableOperation.Annotations;

            // Generate table description.
            GenerateTableDescription(tableAnnotations, tableName);
        }
        protected override void Generate(ColumnModel column, IndentedTextWriter writer)
        {
            string baseCommand;
            using (var baseWriter = Writer())
            {
                base.Generate(column, baseWriter);
                baseCommand = baseWriter.InnerWriter.ToString();
            }

            // Generate column default value.
            baseCommand = GenerateColumnDefaultConstraint(column, baseCommand);

            // Generate column encrypted.
            baseCommand = GenerateColumnEncrypted(column, baseCommand);

            // Write result.
            writer.Write(baseCommand);
        }
        #endregion

        #region Private Generate Methods
        private static string GenerateColumnEncrypted(ColumnModel column, string command)
        {
            // Obtain annotation info
            AnnotationValues values;
            column.Annotations.TryGetValue(ColumnEncryptedWithAnnotationConvention.AnnotationName, out values);
            if (values == null)
            {
                return command;
            }

            // Do SQL generation for column using annotation value as appropriate.
            var value = values.NewValue as string;

            // Exit if no value is defined.
            if (value == null)
            {
                return command;
            }

            // Remove any other default collation if this is a string.
            // String fields need to have BIN2 collation.
            var collate = string.Empty;
            if (column.ClrType == typeof(string))
            {
                collate = " COLLATE Latin1_General_BIN2";
                var indexOfCollation = command.IndexOf(" COLLATE ", StringComparison.InvariantCulture);
                if (indexOfCollation > 0)
                {
                    command = command.Remove(indexOfCollation);
                }
            }

            // Bind the constraint.
            var config = EncryptedWithConfiguration.Deserialize(value);
            var encryptionType = config.Type.ToString().ToUpperInvariant();
            return command + $"{collate} ENCRYPTED WITH (ENCRYPTION_TYPE = {encryptionType}, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256', COLUMN_ENCRYPTION_KEY = {config.KeyName}) ";
        }
        private static void PrepareColumnDefaultConstraint(AddColumnOperation addColumnOperation)
        {
            var column = addColumnOperation.Column;

            // Obtain default contraint annotation info.
            AnnotationValues values;
            column.Annotations.TryGetValue(DefaultConstraintAnnotationConvention.AnnotationName, out values);
            var value = values?.NewValue as string;

            // Constraint is not defined.
            if (value == null)
            {
                // Try reading default value annotation if constraint is not available.
                column.Annotations.TryGetValue(DefaultValueAnnotationConvention.AnnotationName, out values);
                value = values?.NewValue as string;

                // Exit if also no value is defined.
                if (value == null)
                {
                    return;
                }
            }

            // Set column default value.
            addColumnOperation.Column.DefaultValueSql = value;
        }
        private static string GenerateColumnDefaultConstraint(ColumnModel column, string command)
        {
            // Obtain annotation info
            var deserialize = true;
            AnnotationValues values;
            column.Annotations.TryGetValue(DefaultConstraintAnnotationConvention.AnnotationName, out values);
            if (values == null)
            {
                column.Annotations.TryGetValue(DefaultValueAnnotationConvention.AnnotationName, out values);
                if (values == null)
                {
                    return command;
                }
                deserialize = false;
            }

            // Do SQL generation for column using annotation value as appropriate.
            var serialized = values.NewValue as string;

            // Exit if no value is defined.
            if (serialized == null)
            {
                return command;
            }

            // Remove any other default constraint.
            var indexOfDefault = command.IndexOf(" DEFAULT ", StringComparison.InvariantCulture);
            if (indexOfDefault > 0)
            {
                command = command.Remove(indexOfDefault);
            }

            // Bind the constraint.
            string name;
            string value;
            if (deserialize)
            {
                var config = DefaultConstraintConfiguration.Deserialize(serialized);
                name = config.Name;
                value = config.Value;
            }
            else
            {
                name = null;
                value = serialized;
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                command += " CONSTRAINT [" + name + "]";
            }
            return command + " DEFAULT " + value;
        }
        private void GenerateTableDescription(IDictionary<string, AnnotationValues> annotations, string tableName)
        {
            // Obtain annotation info for description.
            AnnotationValues descriptionAnnotation;
            annotations.TryGetValue(TableDescriptionAnnotationConvention.AnnotationName, out descriptionAnnotation);
            if (descriptionAnnotation == null) return;

            GenerateTableDescription(descriptionAnnotation.OldValue as string, descriptionAnnotation.NewValue as string, tableName);
        }
        private void GenerateTableDescription(IDictionary<string, object> annotations, string tableName)
        {
            // Obtain annotation info for description.
            object newDescription;
            annotations.TryGetValue(TableDescriptionAnnotationConvention.AnnotationName, out newDescription);
            GenerateTableDescription(null, newDescription as string, tableName);
        }
        private void GenerateTableDescription(string oldDescription, string newDescription, string tableName)
        {
            // Drop description.
            if (oldDescription != null)
            {
                using (var writer = Writer())
                {
                    DropTableDescription(writer, tableName);
                    Statement(writer);
                }
            }

            // Create description.
            if (newDescription != null)
            {
                using (var writer = Writer())
                {
                    CreateTableDescription(writer, tableName, newDescription);
                    Statement(writer);
                }
            }
        }
        private void GenerateColumnDefaultConstraint(IDictionary<string, AnnotationValues> annotations, string tableName, string columnName)
        {
            // Obtain annotation info for default constraint.
            AnnotationValues defaultAnnotation;
            annotations.TryGetValue(DefaultConstraintAnnotationConvention.AnnotationName, out defaultAnnotation);
            if (GenerateColumnDefaultConstraint(defaultAnnotation, tableName, columnName))
            {
                return;
            }

            // Obtain annotation info for default value.
            annotations.TryGetValue(DefaultValueAnnotationConvention.AnnotationName, out defaultAnnotation);
            GenerateColumnDefaultValue(defaultAnnotation, tableName, columnName);
        }
        private bool GenerateColumnDefaultConstraint(AnnotationValues annotationValues, string tableName, string columnName)
        {
            if (annotationValues == null) return false;
            var oldValue = annotationValues.OldValue as string;
            var newValue = annotationValues.NewValue as string;
            var modified = false;

            // Drop default value.
            if (oldValue != null)
            {
                var config = DefaultConstraintConfiguration.Deserialize(oldValue);
                using (var writer = Writer())
                {
                    DropColumnDefaultConstraint(writer, tableName, columnName, config.Name);
                    Statement(writer);
                }
                modified = true;
            }

            // Create default value.
            if (newValue != null)
            {
                var config = DefaultConstraintConfiguration.Deserialize(newValue);
                using (var writer = Writer())
                {
                    CreateColumnDefaultConstraint(writer, tableName, columnName, config.Name, config.Value);
                    Statement(writer);
                }
                modified = true;
            }

            return modified;
        }
        private void GenerateColumnDefaultValue(AnnotationValues annotationValues, string tableName, string columnName)
        {
            if (annotationValues == null) return;
            var oldValue = annotationValues.OldValue as string;
            var newValue = annotationValues.NewValue as string;

            // Drop default value.
            if (oldValue != null)
            {
                using (var writer = Writer())
                {
                    DropColumnDefaultConstraint(writer, tableName, columnName, null);
                    Statement(writer);
                }
            }

            // Create default value.
            if (newValue != null)
            {
                using (var writer = Writer())
                {
                    CreateColumnDefaultConstraint(writer, tableName, columnName, null, newValue);
                    Statement(writer);
                }
            }
        }
        private void GenerateColumnDescription(IDictionary<string, AnnotationValues> annotations, string tableName, string columnName)
        {
            // Obtain annotation info for description.
            AnnotationValues descriptionAnnotation;
            annotations.TryGetValue(ColumnDescriptionAnnotationConvention.AnnotationName, out descriptionAnnotation);
            GenerateColumnDescription(descriptionAnnotation, tableName, columnName);
        }
        private void GenerateColumnDescription(AnnotationValues annotationValues, string tableName, string columnName)
        {
            if (annotationValues == null) return;
            var oldDescription = annotationValues.OldValue as string;
            var newDescription = annotationValues.NewValue as string;

            // Drop description.
            if (oldDescription != null)
            {
                using (var writer = Writer())
                {
                    DropColumnDescription(writer, tableName, columnName);
                    Statement(writer);
                }
            }

            // Create description.
            if (newDescription != null)
            {
                using (var writer = Writer())
                {
                    CreateColumnDescription(writer, tableName, columnName, newDescription);
                    Statement(writer);
                }
            }
        }
        #endregion

        #region Private SQL Utilities
        public static void CreateTableDescription(TextWriter writer, string tableName, string description)
        {
            string[] parts = tableName.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            var tableSchema = parts.Length == 2 ? parts[0] : "dbo";
            var tableNoSchema = parts.Length == 2 ? parts[1] : parts[0];
            description = description.Replace("'", "''");
            writer.Write("IF NOT EXISTS (SELECT NULL FROM SYS.EXTENDED_PROPERTIES WHERE [major_id] = OBJECT_ID('{0}') AND [name] = N'MS_Description' AND [minor_id] = 0) EXECUTE sp_addextendedproperty N'MS_Description', '{1}', N'SCHEMA', N'{2}', N'TABLE', N'{0}', NULL, NULL ELSE EXECUTE sp_updateextendedproperty N'MS_Description', '{1}', N'SCHEMA', N'{2}', N'TABLE', N'{0}', NULL, NULL", tableNoSchema, description, tableSchema);
        }
        public static void DropTableDescription(TextWriter writer, string tableName)
        {
            string[] parts = tableName.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            var tableSchema = parts.Length == 2 ? parts[0] : "dbo";
            var tableNoSchema = parts.Length == 2 ? parts[1] : parts[0];
            writer.Write("EXECUTE sp_dropextendedproperty N'MS_Description', N'SCHEMA', N'{0}', N'TABLE', N'{1}'", tableSchema, tableNoSchema);
        }
        public static void CreateColumnDescription(TextWriter writer, string tableName, string columnName, string description)
        {
            string[] parts = tableName.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            var tableSchema = parts.Length == 2 ? parts[0] : "dbo";
            var tableNoSchema = parts.Length == 2 ? parts[1] : parts[0];
            description = description.Replace("'", "''");
            writer.Write("IF NOT EXISTS (SELECT NULL FROM SYS.EXTENDED_PROPERTIES WHERE [major_id] = OBJECT_ID('{0}') AND [name] = N'MS_Description' AND [minor_id] = (SELECT [column_id] FROM SYS.COLUMNS WHERE [name] = '{1}' AND [object_id] = OBJECT_ID('{0}'))) EXECUTE sp_addextendedproperty N'MS_Description', '{2}', N'SCHEMA', N'{3}', N'TABLE', N'{0}', N'COLUMN', N'{1}' ELSE EXECUTE sp_updateextendedproperty N'MS_Description', '{2}', N'SCHEMA', N'{3}', N'TABLE', N'{0}', N'COLUMN', N'{1}'", tableNoSchema, columnName, description, tableSchema);
        }
        public static void DropColumnDescription(TextWriter writer, string tableName, string columnName)
        {
            string[] parts = tableName.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            var tableSchema = parts.Length == 2 ? parts[0] : "dbo";
            var tableNoSchema = parts.Length == 2 ? parts[1] : parts[0];
            writer.Write("EXECUTE sp_dropextendedproperty N'MS_Description', N'SCHEMA', N'{0}', N'TABLE', N'{1}', N'COLUMN', N'{2}'", tableSchema, tableNoSchema, columnName);
        }
        private void CreateColumnDefaultConstraint(TextWriter writer, string tableName, string columnName, string constraintName, string value)
        {
            if (string.IsNullOrEmpty(constraintName))
            {
                writer.WriteLine(SqlAddUnnamedConstraint, Name(tableName), columnName, value);
            }
            else
            {
                writer.WriteLine(SqlAddNamedConstraint, Name(tableName), columnName, constraintName, value);
            }
        }
        private void DropColumnDefaultConstraint(TextWriter writer, string tableName, string columnName, string constraintName)
        {
            if (string.IsNullOrEmpty(constraintName))
            {
                var tableNoSchema = tableName.Split(Separators, StringSplitOptions.RemoveEmptyEntries).Last();
                writer.WriteLine(SqlDropUnnamedConstraint, Name(tableName), tableNoSchema, columnName);
            }
            else
            {
                writer.WriteLine(SqlDropNamedConstraint, Name(tableName), constraintName);
            }
        }
        #endregion

        #region Unsupported migrations
        private static void UnsupportedColumnEncryped(IDictionary<string, AnnotationValues> annotations, string tableName, string columnName)
        {
            // Obtain annotation.
            AnnotationValues encryptedAnnotation;
            annotations.TryGetValue(ColumnEncryptedWithAnnotationConvention.AnnotationName, out encryptedAnnotation);
            if (encryptedAnnotation == null) return;

            // Unsupported operation.
            throw new MigrationsException($"Unable to modify column '{columnName}' of table '{tableName}'. Column Always Encrypted must be added when the column is created, this column is already created.");
        }
        #endregion
    }
}
