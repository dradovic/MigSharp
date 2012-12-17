using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

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
        public override string Dbo { get { return "[dbo]."; } }

        public override string ExistsTable(string databaseName, string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].{0}') AND type in (N'U')) SELECT 0 ELSE SELECT 1",
                Escape(tableName));
        }

        protected override IEnumerable<string> DropDefaultConstraint(string tableName, string columnName, bool checkIfExists)
        {
            string constraintName = GetDefaultConstraintName(tableName, columnName);
            string commandText = DropConstraint(tableName, constraintName);
            if (checkIfExists)
            {
                commandText = PrefixIfObjectExists(constraintName, commandText);
            }
            yield return commandText;
        }

        private static string PrefixIfObjectExists(string objectName, string commandTextToBePrefixed)
        {
            return string.Format(CultureInfo.InvariantCulture, "IF OBJECT_ID('{0}') IS NOT NULL ", objectName) + commandTextToBePrefixed;
        }

        public override IEnumerable<string> RenameTable(string oldName, string newName)
        {
            yield return string.Format("EXEC dbo.sp_rename @objname = N'[dbo].{0}', @newname = N'{1}', @objtype = N'OBJECT'", Escape(oldName), newName);
        }

        public override IEnumerable<string> RenameColumn(string tableName, string oldName, string newName)
        {
            string defaultConstraintName = GetDefaultConstraintName(tableName, oldName);
            string renameDefaultConstraintName = string.Format("EXEC dbo.sp_rename @objname = N'{0}', @newname = N'{1}', @objtype = N'OBJECT'", Escape(defaultConstraintName), GetDefaultConstraintName(tableName, newName));
            yield return PrefixIfObjectExists(defaultConstraintName, renameDefaultConstraintName);
            yield return string.Format("EXEC dbo.sp_rename @objname=N'[dbo].{0}.{1}', @newname=N'{2}', @objtype=N'COLUMN'", Escape(tableName), Escape(oldName), newName);
        }

        public override IEnumerable<string> DropColumn(string tableName, string columnName)
        {
            return DropDefaultConstraint(tableName, columnName, true)
                .Concat(base.DropColumn(tableName, columnName));
        }

        public override IEnumerable<string> RenamePrimaryKey(string tableName, string oldName, string newName)
        {
            yield return string.Format("EXEC sp_rename N'[dbo].{0}.{1}', N'{2}', N'INDEX'", Escape(tableName), Escape(oldName), newName);
        }

        public override IEnumerable<string> DropIndex(string tableName, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP INDEX {0} ON [dbo].{1} WITH ( ONLINE = OFF )", Escape(indexName), Escape(tableName));
        }

        protected override string GetTypeSpecifier(DataType type)
        {
            switch (type.DbType)
            {
                case DbType.AnsiString:
                    if (type.Size > 0)
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
                    return "[smallint]";
                case DbType.Boolean:
                    return "[bit]";
                    //case DbType.Currency:
                    //    break;
                case DbType.Date:
                    return "[date]";
                case DbType.DateTime:
                    return "[datetime]";
                case DbType.Decimal:
                    return string.Format(CultureInfo.InvariantCulture, "[decimal]({0}, {1})", type.Size, type.Scale);
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
                    if (type.Size > 0)
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
                    return string.Format(CultureInfo.InvariantCulture, "[numeric]({0}, {1})", type.Size, type.Scale);
                case DbType.AnsiStringFixedLength:
                    return string.Format(CultureInfo.InvariantCulture, "[char]({0})", type.Size);
                case DbType.StringFixedLength:
                    return string.Format(CultureInfo.InvariantCulture, "[nchar]({0})", type.Size);
                    //case DbType.Xml:
                    //    break;
                case DbType.DateTime2:
                    return "[datetime2]";
                case DbType.DateTimeOffset:
                    return "[datetimeoffset]";
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}