namespace MigSharp
{
    /// <summary>
    /// Lists special default values for columns.
    /// </summary>
    public enum SpecialDefaultValue
    {
        /// <summary>
        /// Represents the current date time of the database server.
        /// </summary>
        CurrentDateTime = 0,

        /// <summary>
        /// Represents the current UTC date time of the database server.
        /// </summary>
        CurrentUtcDateTime = 1,

        /// <summary>
        /// Represents the current date time of the database server including its timezone offset (SQL Server only).
        /// </summary>
        CurrentDateTimeOffset = 2,

        /// <summary>
        /// Generates a new GUID.
        /// </summary>
        NewGuid = 3,

        /// <summary>
        /// Generates a new, tentatively sequential (SQL Server) or otherwise random GUID.
        /// </summary>
        NewSequentialGuid = 4,
    }
}