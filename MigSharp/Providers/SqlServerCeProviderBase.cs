using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using MigSharp.Core;

namespace MigSharp.Providers
{
    [Supports(DbType.AnsiString, MaximumSize = 4000, CanBeUsedAsPrimaryKey = true, Warning = "Is not natively supported by SQL Server CE. DbParameters with DbType.AnsiString will not work (instead use DbType.String).")]
    [Supports(DbType.AnsiString, Warning = "Is not natively supported by SQL Server CE. DbParameters with DbType.AnsiString throws an exception (instead cast the DbParameter to SqlCeParameter and set SqlDbType to SqlDbType.NText).")]
    [Supports(DbType.Binary)]
    [Supports(DbType.Boolean)]
    [Supports(DbType.Byte)]
    //[Supports(DbType.Currency)]
    [Supports(DbType.DateTime)]
    [Supports(DbType.Decimal, MaximumSize = 28, MaximumScale = 28)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Decimal, MaximumSize = 28)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Double)]
    [Supports(DbType.Guid, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Int16)]
    [Supports(DbType.Int32, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int64, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    //[Supports(DbType.SByte)]
    [Supports(DbType.Single)]
    [Supports(DbType.String, MaximumSize = 4000, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.String, Warning = "Using a DbParameter with DbType set to DbType.String throws an exception when the string is longer than 4000 characters. Cast the DbParameter to SqlCeParameter and set SqlDbType to SqlDbType.NText.")]
    [Supports(DbType.StringFixedLength, MaximumSize = 4000, CanBeUsedAsPrimaryKey = true)]
    //[Supports(DbType.UInt16)]
    //[Supports(DbType.UInt32)]
    //[Supports(DbType.UInt64)]
    internal class SqlServerCeProviderBase : SqlServerProviderBase
    {
        public override bool SpecifyWith { get { return false; } }

        public override string GetSchemaPrefix(TableName tableName)
        {
            return string.Empty;
        }

        public override string ExistsTable(string databaseName, TableName tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"SELECT COUNT(TABLE_NAME) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tableName.Name);
        }

        protected override IEnumerable<string> DropDefaultConstraint(TableName tableName, string columnName, bool checkIfExists)
        {
            // checkIfExists can be ignored
            yield return string.Format(CultureInfo.InvariantCulture, @"ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT",
                Escape(tableName.Name),
                Escape(columnName));
        }

        public override IEnumerable<string> DropTable(TableName tableName, bool checkIfExists)
        {
            if (checkIfExists)
            {
                throw new NotSupportedException("Conditional logic is not supported by the Compact Edition.");
            }
            return base.DropTable(tableName, false);
        }

        public override IEnumerable<string> RenameTable(TableName oldName, string newName)
        {
            yield return string.Format("EXEC sp_rename @objname = N'{0}', @newname = N'{1}'", oldName.Name, newName);
        }

        public override IEnumerable<string> RenameColumn(TableName tableName, string oldName, string newName)
        {
            throw new NotSupportedException("Consider adding a column with the new name, then UPDATEing it with the values from the old one, then dropping the old column. Or if the order of the columns matters, consider creating a new table with the desired schema, then INSERTing the content from the old table to the new one, then renaming the new table to the old name.");
        }

        public override IEnumerable<string> RenamePrimaryKey(TableName tableName, string oldName, string newName)
        {
            throw new NotSupportedException("Drop and add the primary key with the new name instead.");
        }

        public override IEnumerable<string> DropIndex(TableName tableName, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP INDEX {1}.{0}", Escape(indexName), Escape(tableName.Name));
        }

        public override IEnumerable<string> CreateSchema(string schemaName)
        {
            throw new NotSupportedException("Schemata are not supported.");
        }

        public override IEnumerable<string> DropSchema(string schemaName)
        {
            throw new NotSupportedException("Schemata are not supported.");
        }

        protected override string GetTypeSpecifier(DataType type, bool isRowVersion)
        {
            if (isRowVersion)
            {
                return "[rowversion]";
            }
            switch (type.DbType)
            {
                case DbType.Binary:
                    return "image";
                case DbType.Boolean:
                    return "[bit]";
                case DbType.Byte:
                    return "[smallint]";
                    //case DbType.Currency:
                    //    return "[money]";
                case DbType.DateTime:
                    return "[datetime]";
                case DbType.Decimal:
                    return string.Format(CultureInfo.InvariantCulture, "[numeric]({0}, {1})", type.Size, type.Scale ?? 0);
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
                    //case DbType.SByte:
                    //    return "[tinyint]";
                case DbType.Single:
                    return "[real]";
                case DbType.AnsiString:
                case DbType.String:
                    if (type.Size.HasValue)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "[nvarchar]({0})", type.Size);
                    }
                    else
                    {
                        return "ntext";
                    }
                case DbType.StringFixedLength:
                    return string.Format(CultureInfo.InvariantCulture, "[nchar]({0})", type.Size);
                    //case DbType.UInt16:
                    //    break;
                    //case DbType.UInt32:
                    //    break;
                    //case DbType.UInt64:
                    //    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}