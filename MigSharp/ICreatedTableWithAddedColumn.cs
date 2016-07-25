namespace MigSharp
{
    /// <summary>
    /// Represents a column on a newly created table.
    /// </summary>
    public interface ICreatedTableWithAddedColumn : ICreatedTableBase
    {
        /// <summary>
        /// Gets the column name.
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Specifies the size and the scale of the data type of the column.
        /// </summary>
        /// <param name="size">The length for character data types or the maximum total number of decimal digits for numeric data types.</param>
        /// <param name="scale">The maximum number of decimal digits that can be stored to the right of the decimal point. Scale must be a value from 0 through <paramref name="size"/>.</param>
        ICreatedTableWithAddedColumn OfSize(int size, int? scale);

        /// <summary>
        /// Puts the column under an unique constraint.
        /// </summary>
        /// <param name="constraintName">Optionally, specify the name of the unique constraint. If null or empty, a default constraint name will be generated.</param>
        ICreatedTableWithAddedColumn Unique(string constraintName);

        /// <summary>
        /// Makes the column auto-increment.
        /// </summary>
        ICreatedTableWithAddedColumn AsIdentity();

        /// <summary>
        /// Adds a default value to the column.
        /// </summary>
        ICreatedTableWithAddedColumn HavingDefault<T>(T value) where T : struct;

        /// <summary>
        /// Adds a default value to the column.
        /// </summary>
        ICreatedTableWithAddedColumn HavingDefault(string value);
    }

    /// <summary>
    /// Contains the extensions methods for the <see cref="ICreatedTableWithAddedColumn"/> interface.
    /// </summary>
    public static class CreatedTableWithAddedColumnExtensions
    {
        /// <summary>
        /// Specifies the size of the data type of the column.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="size">The length for character data types or the maximum total number of decimal digits for numeric data types.</param>
        public static ICreatedTableWithAddedColumn OfSize(this ICreatedTableWithAddedColumn column, int size)
        {
            return column.OfSize(size, null);
        }

        /// <summary>
        /// Puts the column under an unique constraint with a default constraint name.
        /// </summary>
        public static ICreatedTableWithAddedColumn Unique(this ICreatedTableWithAddedColumn column)
        {
            return column.Unique(null);
        }

        /// <summary>
        /// Sets the default of the column to be the current system time of the database server.
        /// </summary>
        public static ICreatedTableWithAddedColumn HavingCurrentDateTimeAsDefault(this ICreatedTableWithAddedColumn column)
        {
            return column.HavingDefault(SpecialDefaultValue.CurrentDateTime);
        }

        /// <summary>
        /// Sets the default of the column to be the current UTC system time of the database server.
        /// </summary>
        public static ICreatedTableWithAddedColumn HavingCurrentUtcDateTimeAsDefault(this ICreatedTableWithAddedColumn column)
        {
            return column.HavingDefault(SpecialDefaultValue.CurrentUtcDateTime);
        }

        /// <summary>
        /// Sets the default of the column to be the current system time including the timezone offset of the database server.
        /// </summary>
        public static ICreatedTableWithAddedColumn HavingCurrentDateTimeOffsetAsDefault(this ICreatedTableWithAddedColumn column)
        {
            return column.HavingDefault(SpecialDefaultValue.CurrentDateTimeOffset);
        }
    }
}