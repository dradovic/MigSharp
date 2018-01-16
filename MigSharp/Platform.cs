// ReSharper disable InconsistentNaming
namespace MigSharp
{
    /// <summary>
    /// The database platform.
    /// </summary>
    public enum Platform
    {
#pragma warning disable 1591
        SqlServer = 0, // default platform
        MySql = 1,
        Oracle = 2,
        SQLite = 3,
        //Teradata = 4
#pragma warning restore 1591
    }
}