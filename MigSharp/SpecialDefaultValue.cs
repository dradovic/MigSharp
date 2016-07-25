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
        CurrentDateTime,

        /// <summary>
        /// Represents the current UTC date time of the database server.
        /// </summary>
        CurrentUtcDateTime,

        /// <summary>
        /// Represents the current date time of the database server including its timezone offset.
        /// </summary>
        CurrentDateTimeOffset
    }
}