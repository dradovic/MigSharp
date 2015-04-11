using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;

namespace MigSharp.Generate
{
    internal class SqlMigrationGenerator
    {
        private readonly string _connectionString;
        private readonly List<string> _errors = new List<string>();

        public ReadOnlyCollection<string> Errors { get { return new ReadOnlyCollection<string>(_errors); } }

        public SqlMigrationGenerator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string Generate()
        {
            _errors.Clear();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connectionString);
            var server = new Server(builder.DataSource);
            Database database = server.Databases[builder.InitialCatalog];
            database.Refresh(); // load the meta-data
            string migration = string.Empty;
            foreach (Table table in database.Tables)
            {
                if (table.Name.StartsWith("__", StringComparison.Ordinal))
                {
                    // hide special tables such as the migration history table
                    continue;
                }

                AppendLine(string.Format("{0}db.CreateTable(\"{1}\")", Indent(0), table.Name), ref migration);
                Column lastColumn = table.Columns.OfType<Column>().Last();
                foreach (Column column in table.Columns)
                {
                    try
                    {
                        string dbTypeExpression = GetDbTypeExpression(column);
                        string columnKind = column.InPrimaryKey ? "PrimaryKey" : string.Format(CultureInfo.InvariantCulture, "{0}Nullable", column.Nullable ? string.Empty : "Not");
                        AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}.With{1}Column(\"{2}\", {3}){4}{5}",
                            Indent(1),
                            columnKind,
                            column.Name,
                            dbTypeExpression,
                            column.Identity ? ".AsIdentity()" : string.Empty,
                            column == lastColumn ? ";" : string.Empty), ref migration);
                    }
                    catch (NotSupportedException x)
                    {
                        _errors.Add(string.Format("In table {0} for column {1}: {2}", table.Name, column.Name, x.Message));
                    }
                }
            }
            return migration;
        }

        private static string Indent(int count)
        {
            return new string('\t', count + 3);
        }

        private static void AppendLine(string line, ref string migration)
        {
            Console.WriteLine(line);
            migration += line + Environment.NewLine;
        }

        private static string GetDbTypeExpression(Column column)
        {
            DbType dbType = Convert(column.DataType.SqlDataType);
            return typeof(DbType).Name + "." + Enum.GetName(typeof(DbType), dbType);
        }

        private static DbType Convert(SqlDataType type)
        {
            switch (type)
            {
                case SqlDataType.BigInt:
                    return DbType.Int64;
                case SqlDataType.Binary:
                    return DbType.Binary;
                case SqlDataType.Bit:
                    return DbType.Boolean;
                    //case SqlDataType.Char:
                    //    break;
                case SqlDataType.DateTime:
                    return DbType.DateTime;
                case SqlDataType.Decimal:
                    return DbType.Decimal;
                case SqlDataType.Float:
                    return DbType.Single;
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
                    //case SqlDataType.Real:
                    //    break;
                    //case SqlDataType.SmallDateTime:
                    //    break;
                case SqlDataType.SmallInt:
                    return DbType.Int16;
                    //case SqlDataType.SmallMoney:
                    //    break;
                    //case SqlDataType.Text:
                    //    break;
                    //case SqlDataType.Timestamp:
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
                    //case SqlDataType.VarChar:
                    //    break;
                    //case SqlDataType.VarCharMax:
                    //    break;
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
    }
}