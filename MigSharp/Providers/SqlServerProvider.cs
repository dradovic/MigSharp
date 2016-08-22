using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using MigSharp.Core;

namespace MigSharp.Providers
{
    [Supports(DbType.AnsiString, MaximumSize = 8000, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.AnsiString)] // translates to VARCHAR(MAX) without specifying the size
    [Supports(DbType.Binary)]
    [Supports(DbType.Byte, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Boolean, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.DateTime, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Decimal, MaximumSize = 28, MaximumScale = 28, CanBeUsedAsPrimaryKey = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Decimal, MaximumSize = 28, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Double)]
    [Supports(DbType.Guid, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Int16, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int32, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int64, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Single, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.String, MaximumSize = 4000, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.String)] // translates to NVARCHAR(MAX) without specifying the Size
    // FIXME: supports DbType.Time is missing?
    // FIXME: supports DbType.VarNumeric is missing?
    [Supports(DbType.AnsiStringFixedLength, MaximumSize = 8000, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.StringFixedLength, MaximumSize = 4000, CanBeUsedAsPrimaryKey = true)]
    internal abstract class SqlServerProvider : SqlServerProviderBase
    {
        public override bool SpecifyWith { get { return true; } }

        [NotNull]
        private static string GetSchemaName(TableName tableName)
        {
            return tableName.Schema ?? "dbo";
        }

        public override string GetSchemaPrefix(TableName tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}].", GetSchemaName(tableName));
        }

        public override string ExistsTable(string databaseName, TableName tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"SELECT COUNT(TABLE_NAME) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}' AND TABLE_SCHEMA = '{1}'", tableName.Name, tableName.Schema ?? "dbo");
        }

        protected override IEnumerable<string> DropDefaultConstraint(TableName tableName, string columnName, bool checkIfExists)
        {
            string constraintName = GetDefaultConstraintName(tableName.Name, columnName);
            string commandText = DropConstraint(tableName, constraintName);
            if (checkIfExists)
            {
                commandText = PrefixIfObjectExists(constraintName, GetSchemaName(tableName), commandText);
            }
            yield return commandText;
        }

        private static string PrefixIfObjectExists(string objectName, string schemaName, string commandTextToBePrefixed)
        {
            return string.Format(CultureInfo.InvariantCulture, "IF OBJECT_ID('{0}.{1}') IS NOT NULL ", schemaName, objectName) + commandTextToBePrefixed;
        }

        public override IEnumerable<string> RenameTable(TableName oldName, string newName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "EXEC dbo.sp_rename @objname = N'{0}', @newname = N'{1}', @objtype = N'OBJECT'", GetTableQualifier(oldName), newName);
        }

        public override IEnumerable<string> RenameColumn(TableName tableName, string oldName, string newName)
        {
            string schemaName = GetSchemaName(tableName);
            string defaultConstraintName = GetDefaultConstraintName(tableName.Name, oldName);
            string renameDefaultConstraintName = string.Format(CultureInfo.InvariantCulture, "EXEC dbo.sp_rename @objname = N'{0}.{1}', @newname = N'{2}', @objtype = N'OBJECT'", schemaName, Escape(defaultConstraintName), GetDefaultConstraintName(tableName.Name, newName));
            yield return PrefixIfObjectExists(defaultConstraintName, schemaName, renameDefaultConstraintName);
            yield return string.Format(CultureInfo.InvariantCulture, "EXEC dbo.sp_rename @objname=N'{0}.{1}', @newname=N'{2}', @objtype=N'COLUMN'", GetTableQualifier(tableName), Escape(oldName), newName);
        }

        public override IEnumerable<string> DropColumn(TableName tableName, string columnName)
        {
            return DropDefaultConstraint(tableName, columnName, true)
                .Concat(base.DropColumn(tableName, columnName));
        }

        public override IEnumerable<string> RenamePrimaryKey(TableName tableName, string oldName, string newName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "EXEC sp_rename N'{0}.{1}', N'{2}', N'INDEX'", GetTableQualifier(tableName), Escape(oldName), newName);
        }

        public override IEnumerable<string> DropIndex(TableName tableName, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP INDEX {0} ON {1} WITH ( ONLINE = OFF )", Escape(indexName), GetTableQualifier(tableName));
        }

        public override IEnumerable<string> CreateSchema(string schemaName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "CREATE SCHEMA {0}", Escape(schemaName));
        }

        public override IEnumerable<string> DropSchema(string schemaName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP SCHEMA {0}", Escape(schemaName));
        }

        // see: http://msdn.microsoft.com/en-us/library/cc716729.aspx
        protected override string GetTypeSpecifier(DataType type, bool isRowVersion)
        {
            if (isRowVersion)
            {
                return "[rowversion]";
            }
            switch (type.DbType)
            {
                case DbType.AnsiString:
                    if (type.Size.HasValue)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "[varchar]({0})", type.Size);
                    }
                    else
                    {
                        return "[varchar](max)";
                    }
                case DbType.Binary:
                    return "[varbinary](max)";
                case DbType.Byte:
                    return "[tinyint]";
                case DbType.Boolean:
                    return "[bit]";
                    //case DbType.Currency:
                    //    break;
                case DbType.Date:
                    return "[date]";
                case DbType.DateTime:
                    return "[datetime]";
                case DbType.Decimal:
                    return string.Format(CultureInfo.InvariantCulture, "[decimal]({0}, {1})", type.Size, type.Scale.HasValue ? type.Scale : 0);
                case DbType.Double:
                    return "[float]";
                case DbType.Guid:
                    return "[uniqueidentifier]";
                case DbType.Int16:
                    return "[smallint]";
                case DbType.Int32:
                    return "[int]";
                case DbType.Int64:
                    return "[bigint]";
                    //case DbType.Object:
                    //    break;
                case DbType.SByte:
                    return "[tinyint]";
                case DbType.Single:
                    return "[real]";
                case DbType.String:
                    if (type.Size.HasValue)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "[nvarchar]({0})", type.Size);
                    }
                    else
                    {
                        return "[nvarchar](max)";
                    }
                case DbType.Time:
                    return "[time]";
                    //case DbType.UInt16:
                    //    break;
                    //case DbType.UInt32:
                    //    break;
                    //case DbType.UInt64:
                    //    break;
                case DbType.VarNumeric:
                    if (type.Scale.HasValue)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "[numeric]({0}, {1})", type.Size, type.Scale);
                    }
                    else
                    {
                        return string.Format(CultureInfo.InvariantCulture, "[numeric]({0})", type.Size);                        
                    }
                case DbType.AnsiStringFixedLength:
                    return string.Format(CultureInfo.InvariantCulture, "[char]({0})", type.Size);
                case DbType.StringFixedLength:
                    return string.Format(CultureInfo.InvariantCulture, "[nchar]({0})", type.Size);
                    //case DbType.Xml:
                    //    break;
                case DbType.DateTime2:
                    if (type.Size.HasValue)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "[datetime2]({0})", type.Size);
                    }
                    else
                    {
                        return "[datetime2]";
                    }
                case DbType.DateTimeOffset:
                    return "[datetimeoffset]";
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}