using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.SqlServer.Management.Smo;

namespace MigSharp.Generate
{
    internal abstract class SqlMigrationGeneratorBase : IGenerator
    {
        private readonly Server _server;
        private readonly Database _database;
        private readonly GeneratorOptions _options;
        private readonly StringBuilder _migration = new StringBuilder();
        private readonly List<string> _errors = new List<string>();

        public ReadOnlyCollection<string> Errors { get { return new ReadOnlyCollection<string>(_errors); } }

        protected Database Database { get { return _database; } }

        protected GeneratorOptions Options { get { return _options; } }

        protected abstract string ClassName { get; }
        protected abstract string ExportAttribute { get; }

        protected SqlMigrationGeneratorBase(Server server, Database database, GeneratorOptions options)
        {
            _server = server;
            _database = database;
            _options = options;
        }

        public string Generate()
        {
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            AppendLine("using System.CodeDom.Compiler;", 0);
            AppendLine("using System.Data;", 0);
            AppendLine("using MigSharp;", 0);
            AppendLine("// ReSharper disable RedundantVerbatimStringPrefix", 0);
            AppendLine();
            AppendLine("namespace " + _options.Namespace, 0);
            AppendLine("{{", 0);
            AppendLine("[GeneratedCode(\"{0}\", \"{1}\")]", 1, assemblyName.Name, assemblyName.Version);
            AppendLine("[{0}{1}]", 1, ExportAttribute, GetExportAttributeArgs());
            AppendLine("public class {0} : IMigration", 1, ClassName);
            AppendLine("{{", 1);
            AppendLine("[GeneratedCode(\"{0}\", \"{1}\")]", 2, assemblyName.Name, assemblyName.Version);
            AppendLine("public void Up(IDatabase db)", 2);
            AppendLine("{{", 2);
            GenerateUpBody();
            AppendLine("}}", 2);
            AppendLine("}}", 1);
            AppendLine("}}", 0);

            return _migration.ToString();
        }

        private void GenerateUpBody()
        {
            bool isFirstSchema = true;
            foreach (dynamic schema in _database.Schemas)
            {
                if (!_options.IsSchemaIncluded(schema.Name) ||
                    schema.IsSystemObject)
                {
                    Console.WriteLine("Skipping schema [{0}]", schema.Name);
                    continue;
                }
                if (isFirstSchema)
                {
                    AppendLine("//", 3);
                    AppendLine("// Schemas", 3);
                    AppendLine("//", 3);
                    isFirstSchema = false;
                }
                HandleSchema(schema);
            }

            if (!isFirstSchema) // there were schemas
            {
                AppendLine();
            }
            AppendLine("//", 3);
            AppendLine("// Tables", 3);
            AppendLine("//", 3);
            var tablesWithForeignKeys = new List<Table>();
            foreach (Table table in _database.Tables.Cast<Table>().OrderBy(t => t.Schema).ThenBy(t => t.Name))
            {
                if (!_options.IsSchemaIncluded(table.Schema) ||
                    !_options.IsTableIncluded(table.Name) ||
                    table.Name.StartsWith("__", StringComparison.Ordinal) || // hide special tables such as the EF migration history table
                    (table.Name == _options.VersioningTableName && table.Schema == _options.VersioningTableSchema))
                {
                    Console.WriteLine("Skipping table [{0}].[{1}]", table.Schema, table.Name);
                    continue;
                }

                HandleTable(table);
                if (table.ForeignKeys.Count > 0)
                {
                    tablesWithForeignKeys.Add(table);
                }
            }

            if (tablesWithForeignKeys.Any())
            {
                AppendLine();
                AppendLine("//", 3);
                AppendLine("// Foreign Keys", 3);
                AppendLine("//", 3);
            }
            foreach (var table in tablesWithForeignKeys)
            {
                foreach (ForeignKey foreignKey in table.ForeignKeys)
                {
                    HandleForeignKey(table, foreignKey);
                }
            }

            if (_database.UserDefinedTableTypes.Count > 0)
            {
                AppendLine();
                AppendLine("//", 3);
                AppendLine("// User Defined Table Types", 3);
                AppendLine("//", 3);
            }
            foreach (UserDefinedTableType tableType in _database.UserDefinedTableTypes)
            {
                HandleUserDefinedTableType(tableType);
            }
        }

        private void HandleSchema(Schema schema)
        {
            AppendLine("db.CreateSchema(\"{0}\");", 3, schema.Name);
        }

        private void HandleTable(Table table)
        {
            var primaryKeyName = GetPrimaryKeyName(table);
            AppendLine(string.Format(CultureInfo.InvariantCulture, "db.Schemata[\"{0}\"].CreateTable(\"{1}\"{2})",
                table.Schema,
                table.Name,
                string.IsNullOrEmpty(primaryKeyName) ? string.Empty : ", \"" + primaryKeyName + "\""), 3);
            Column lastColumn = table.Columns.OfType<Column>().Last();
            foreach (Column column in table.Columns)
            {
                HandleColumn(table, column, column == lastColumn);
            }
            foreach (Index index in table.Indexes)
            {
                HandleIndex(table, index);
            }
            if (_options.IncludeData)
            {
                HandleTableData(table);
            }
            AppendLine();
        }

        private void HandleUserDefinedTableType(UserDefinedTableType userDefinedTableType)
        {
            var scripter = new Scripter(_server) { Options = { ScriptData = true, IncludeHeaders = false } };
            string[] script = scripter.EnumScript(new SqlSmoObject[] { userDefinedTableType }).ToArray();
            if (script.Length != 1)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not script user defined table type. Expected one line but was: {0}", string.Join(Environment.NewLine, script)));
            }
            AppendLine("db.Execute(@\"{0}\");", 3, script[0]);
        }

        private void HandleTableData(Table table)
        {
            var scripter = new Scripter(_server) { Options = { ScriptData = true, IncludeHeaders = false } };
            IEnumerable<string> script = scripter.EnumScript(new SqlSmoObject[] { table });
            foreach (var line in script
                .Where(l => l.ToUpperInvariant().StartsWith("INSERT", StringComparison.Ordinal) ||
                            l.ToUpperInvariant().StartsWith("SET IDENTITY_INSERT", StringComparison.Ordinal)))
            {
                AppendLine("db.Execute(@\"{0}\");", 3, line.Replace("\"", "\"\""));
            }
        }

        private static string GetPrimaryKeyName(Table table)
        {
            Index primaryKey = table.Indexes.Cast<Index>().SingleOrDefault(i => i.IndexKeyType == IndexKeyType.DriPrimaryKey);
            return primaryKey != null ? primaryKey.Name : string.Empty;
        }

        private void HandleForeignKey(Table table, ForeignKey foreignKey)
        {
            AppendLine("db.Schemata[\"{0}\"].Tables[\"{1}\"].AddForeignKeyTo(\"{2}\", \"{3}\")", 3,
                table.Schema,
                table.Name,
                foreignKey.ReferencedTable,
                foreignKey.Name);
            if (foreignKey.ReferencedTableSchema != table.Schema)
            {
                AppendLine(".InSchema(\"{0}\")", 4, foreignKey.ReferencedTableSchema); // CLEAN: use nameof
            }
            ForeignKeyColumn lastColumn = foreignKey.Columns.Cast<ForeignKeyColumn>().Last();
            bool cascadeDeletion = foreignKey.DeleteAction == ForeignKeyAction.Cascade;
            foreach (ForeignKeyColumn column in foreignKey.Columns)
            {
                AppendLine(".Through(\"{0}\", \"{1}\"){2}", 4,
                    column.Name,
                    column.ReferencedColumn,
                    !cascadeDeletion && column == lastColumn ? ";" : string.Empty);
            }
            if (cascadeDeletion)
            {
                AppendLine(".CascadeOnDelete();", 4);
            }
        }

        private void HandleIndex(Table table, Index index)
        {
            if (index.IndexKeyType == IndexKeyType.DriPrimaryKey || index.IsUnique) return; // handled in HandleColumn

            string line = string.Format(CultureInfo.InvariantCulture, "db.Schemata[\"{0}\"].Tables[\"{1}\"].AddIndex()",
                table.Schema,
                table.Name);
            foreach (IndexedColumn column in index.IndexedColumns)
            {
                line += string.Format(CultureInfo.InvariantCulture, ".OnColumn(\"{0}\")", column.Name);
            }
            line += ";";
            AppendLine(line, 3);
        }

        private void HandleColumn(Table table, Column column, bool isLastColumn)
        {
            try
            {
                bool isRowVersionColumn = column.DataType.SqlDataType == SqlDataType.Timestamp;
                string columnKind = column.InPrimaryKey ? "PrimaryKey" :
                    (isRowVersionColumn ? "RowVersion" : string.Format(CultureInfo.InvariantCulture, "{0}Nullable", column.Nullable ? string.Empty : "Not"));
                string uniqueExpression = GetUniqueExpression(table, column);
                AppendLine(".With{0}Column(\"{1}\"{2}){3}{4}{5}{6}{7}", 4,
                    columnKind,
                    column.Name,
                    isRowVersionColumn ? string.Empty : ", " + GetDbTypeExpression(column),
                    isRowVersionColumn ? string.Empty : GetOfSize(column),
                    column.Identity ? ".AsIdentity()" : string.Empty,
                    uniqueExpression,
                    GetHavingDefault(column),
                    isLastColumn ? ";" : string.Empty);
            }
            catch (NotSupportedException x)
            {
                _errors.Add(string.Format(CultureInfo.CurrentCulture, "In table {0}.{1} for column {2}: {3}", table.Schema, table.Name, column.Name, x.Message));
            }
        }

        private static string GetHavingDefault(Column column)
        {
            if (column.DefaultConstraint == null) return string.Empty;

            string defaultContraint = column.DefaultConstraint.Text.Substring(1, column.DefaultConstraint.Text.Length - 2); // note: for some reason, the Text always is enclosed in parenthesis (...)
            if (defaultContraint.ToUpperInvariant() == "GETDATE()") return ".HavingCurrentDateTimeAsDefault()";
            if (defaultContraint.ToUpperInvariant() == "GETUTCDATE()") return ".HavingCurrentUtcDateTimeAsDefault()";
            if (defaultContraint.ToUpperInvariant() == "SYSDATETIMEOFFSET()") return ".HavingCurrentDateTimeOffsetAsDefault()";
            if (defaultContraint.ToUpperInvariant() == "NEWID()") return ".HavingNewGuidAsDefault()";
            if (defaultContraint.ToUpperInvariant() == "NEWSEQUENTIALID()") return ".HavingNewSequentialGuidAsDefault()";
            return ".HavingDefault(" + GetDefaultConstraintValue(defaultContraint, column.DataType) + ")";
        }

        private static string GetDefaultConstraintValue(string defaultConstraint, DataType dataType)
        {
            if (dataType.SqlDataType == SqlDataType.BigInt ||
                dataType.SqlDataType == SqlDataType.Int ||
                dataType.SqlDataType == SqlDataType.SmallInt ||
                dataType.SqlDataType == SqlDataType.TinyInt ||
                dataType.SqlDataType == SqlDataType.Bit)
            {
                return defaultConstraint;
            }
            if (dataType.SqlDataType == SqlDataType.NVarChar ||
                dataType.SqlDataType == SqlDataType.NVarCharMax)
            {
                Match match = Regex.Match(defaultConstraint, @"N'(.*?)'");
                if (!match.Success)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not parse '{0}' as NVARCHAR literal.", defaultConstraint));
                }
                return "\"" + match.Groups[1].Value + "\"";
            }
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Default constraint for columns of type: {0} are not supported yet.", dataType.SqlDataType));
        }

        private static string GetUniqueExpression(Table table, Column column)
        {
            if (column.InPrimaryKey) return string.Empty;

            Index uniqueIndex = FindUniqueIndex(table, column);
            if (uniqueIndex == null) return string.Empty;

            if (uniqueIndex.IndexedColumns.Count == 1)
            {
                return ".Unique()"; // no unique constraint name required
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, ".Unique(\"{0}\")", uniqueIndex.Name);
            }
        }

        private static Index FindUniqueIndex(Table table, Column column)
        {
            return table.Indexes.Cast<Index>().SingleOrDefault(i => i.IsUnique && i.IndexedColumns.Cast<IndexedColumn>().Any(c => c.Name == column.Name));
        }

        private static string GetOfSize(Column column)
        {
            if (column.DataType.MaximumLength <= 0)
            {
                return string.Empty;
            }
            DbType dbType = Convert(column.DataType.SqlDataType);
            if (dbType == DbType.String || dbType == DbType.AnsiString || dbType == DbType.StringFixedLength || dbType == DbType.AnsiStringFixedLength)
            {
                return string.Format(CultureInfo.InvariantCulture, ".OfSize({0})", column.DataType.MaximumLength);
            }
            if (dbType == DbType.Decimal)
            {
                if (column.DataType.NumericScale > 0)
                {
                    return string.Format(CultureInfo.InvariantCulture, ".OfSize({0}, {1})", column.DataType.NumericPrecision, column.DataType.NumericScale);
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, ".OfSize({0})", column.DataType.NumericPrecision);
                }
            }
            if (dbType == DbType.DateTime2 && column.DataType.NumericScale != 7)
            {
                return string.Format(CultureInfo.InvariantCulture, ".OfSize({0})", column.DataType.NumericScale);
            }
            return string.Empty;
        }

        private static string GetDbTypeExpression(Column column)
        {
            DbType dbType = Convert(column.DataType.SqlDataType);
            return typeof(DbType).Name + "." + Enum.GetName(typeof(DbType), dbType);
        }

        private static DbType Convert(SqlDataType type)
        {
            Debug.Assert(type != SqlDataType.Timestamp, "This case is handled elsewhere as RowVersion column.");

            // see: https://msdn.microsoft.com/en-us/library/cc716729(v=vs.110).aspx
            switch (type)
            {
                case SqlDataType.BigInt:
                    return DbType.Int64;
                case SqlDataType.Binary:
                    return DbType.Binary;
                case SqlDataType.Bit:
                    return DbType.Boolean;
                case SqlDataType.Char:
                    return DbType.AnsiStringFixedLength;
                case SqlDataType.DateTime:
                    return DbType.DateTime;
                case SqlDataType.Decimal:
                    return DbType.Decimal;
                case SqlDataType.Float:
                    return DbType.Double;
                //case SqlDataType.Image:
                //    break;
                case SqlDataType.Int:
                    return DbType.Int32;
                //case SqlDataType.Money:
                //    break;
                case SqlDataType.NChar:
                    return DbType.StringFixedLength;
                //case SqlDataType.NText:
                //    break;
                case SqlDataType.NVarChar:
                    return DbType.String;
                case SqlDataType.NVarCharMax:
                    return DbType.String;
                case SqlDataType.Real:
                    return DbType.Single;
                //case SqlDataType.SmallDateTime:
                //    break;
                case SqlDataType.SmallInt:
                    return DbType.Int16;
                //case SqlDataType.SmallMoney:
                //    break;
                //case SqlDataType.Text:
                //    break;
                case SqlDataType.TinyInt:
                    return DbType.Byte;
                case SqlDataType.UniqueIdentifier:
                    return DbType.Guid;
                //case SqlDataType.UserDefinedDataType:
                //    break;
                //case SqlDataType.UserDefinedType:
                //    break;
                //case SqlDataType.VarBinary:
                //    break;
                case SqlDataType.VarBinaryMax:
                    return DbType.Binary;
                case SqlDataType.VarChar:
                    return DbType.AnsiString;
                case SqlDataType.VarCharMax:
                    return DbType.AnsiString;
                //case SqlDataType.Variant:
                //    break;
                case SqlDataType.Xml:
                    return DbType.Xml;
                //case SqlDataType.SysName:
                //    break;
                //case SqlDataType.Numeric:
                //    break;
                case SqlDataType.Date:
                    return DbType.Date;
                case SqlDataType.Time:
                    return DbType.Time;
                case SqlDataType.DateTimeOffset:
                    return DbType.DateTimeOffset;
                case SqlDataType.DateTime2:
                    return DbType.DateTime2;
                //case SqlDataType.UserDefinedTableType:
                //    break;
                //case SqlDataType.HierarchyId:
                //    break;
                //case SqlDataType.Geometry:
                //    break;
                //case SqlDataType.Geography:
                //    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "The column type {0} is not supported yet.", type));
            }
        }

        [StringFormatMethod("format")]
        private void AppendLine(string format, int indent, params object[] args)
        {
            string lineToAppend = Indent(indent) + format;
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, lineToAppend, args));
            _migration.AppendFormat(CultureInfo.InvariantCulture, lineToAppend + Environment.NewLine, args);
        }

        private void AppendLine()
        {
            _migration.AppendLine();
        }

        private static string Indent(int count)
        {
            return new string('\t', count);
        }

        private string GetExportAttributeArgs()
        {
            return string.IsNullOrEmpty(_options.ModuleName) ? string.Empty : string.Format(CultureInfo.InvariantCulture, "(ModuleName = \"{0}\")", _options.ModuleName);
        }
    }
}