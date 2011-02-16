using System.Globalization;

namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.TeradataOdbc, "System.Data.Odbc", SupportsTransactions = false, ParameterExpression = "?")]
    internal class TeradataOdbcProvider : TeradataProvider
    {
        public override string ExistsTable(string databaseName, string tableName)
        {
            // For the TeradataOdbc provider, we need to call the built-in DATABASE function in order to find out the current database as
            // the passed in parameter for the databaseName is empty.
            // Unfortunately, the DATABASE function does not work when connected we the native driver.
            return string.Format(CultureInfo.InvariantCulture, @"SELECT COUNT(*) FROM DBC.TABLES WHERE DATABASENAME=DATABASE AND TABLENAME='{0}'", tableName);
        }
    }
}