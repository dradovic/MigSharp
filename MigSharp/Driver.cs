namespace MigSharp
{
    /// <summary>
    /// The type of driver used to access the database.
    /// </summary>
    public enum Driver
    {
#pragma warning disable 1591
        AdoNet = 0,
        Odbc = 1,
#pragma warning restore 1591
    }
}