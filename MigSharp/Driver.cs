namespace MigSharp
{
    /// <summary>
    /// The type of driver used to access the database.
    /// </summary>
    public enum Driver
    {
#pragma warning disable 1591
        AdoNet,
        Odbc
#pragma warning restore 1591
    }
}