namespace MigSharp
{
    /// <summary>
    /// The database platform.
    /// </summary>
    public enum Platform
    {
#pragma warning disable 1591
        SqlServer, // default platform
        SqlServerCe,
        MySql,
        Oracle,
        SQLite,
        Teradata
#pragma warning restore 1591
    }
}