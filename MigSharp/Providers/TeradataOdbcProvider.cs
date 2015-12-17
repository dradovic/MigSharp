using System.Globalization;
using MigSharp.Core;

namespace MigSharp.Providers
{
    [ProviderExport(Platform.Teradata, 12, Driver.Odbc, "System.Data.Odbc", SupportsTransactions = false, ParameterExpression = "?", PrefixUnicodeLiterals = PrefixUnicodeLiterals)]
    internal class TeradataOdbcProvider : TeradataProvider
    {
        public override string ExistsTable(string databaseName, TableName tableName)
        {
            // For the TeradataOdbc provider, we need to call the built-in DATABASE function in order to find out the current database as
            // the passed in parameter for the databaseName is empty.
            // Unfortunately, the DATABASE function does not work when connected we the native driver.
            return string.Format(CultureInfo.InvariantCulture, @"SELECT COUNT(*) FROM DBC.TABLES WHERE DATABASENAME=DATABASE AND TABLENAME='{0}'", tableName.Name);
        }
    }
}