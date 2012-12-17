namespace MigSharp
{
    /// <summary>
    /// Contains the names of the supported providers.
    /// </summary>
    public static class ProviderNames
    {
#pragma warning disable 1591
        public const string MySqlExperimental = "MySql"; // FIXME: remove Experimental
        public const string Oracle = "Oracle";
        public const string OracleOdbc = "OracleOdbc";
        public const string SQLite = "SQLite";
        public const string SqlServer2005 = "SqlServer2005";
        public const string SqlServer2005Odbc = "SqlServer2005Odbc";
        public const string SqlServer2008 = "SqlServer2008";
        public const string SqlServer2012 = "SqlServer2012";
        public const string SqlServerCe35 = "SqlServerCe35";
        public const string SqlServerCe4 = "SqlServerCe4";
        public const string Teradata = "Teradata";
        public const string TeradataOdbc = "TeradataOdbc";
#pragma warning restore 1591
    }
}