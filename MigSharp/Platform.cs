namespace MigSharp
{
    /// <summary>
    /// The database platform.
    /// </summary>
    public enum Platform
    {
#pragma warning disable 1591
        SqlServer, // default platform
        MySql,
        Oracle,
        SQLite,
        Teradata
#pragma warning restore 1591
    }
}