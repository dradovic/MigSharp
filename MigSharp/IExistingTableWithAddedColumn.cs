using MigSharp.Providers;

namespace MigSharp
{
    /// <summary>
    /// Represents an added column on an existing table.
    /// </summary>
    public interface IExistingTableWithAddedColumn : IExistingTableBase
    {
        /// <summary>
        /// Specifies the size and the scale of the data type of the column.
        /// </summary>
        /// <param name="size">The length for character data types or the maximum total number of decimal digits for numeric data types.</param>
        /// <param name="scale">The maximum number of decimal digits that can be stored to the right of the decimal point. Scale must be a value from 0 through <paramref name="size"/>.</param>
        IExistingTableWithAddedColumn OfSize(int size, int? scale);

        /// <summary>
        /// Adds a default value to the column.
        /// </summary>
        IExistingTableBase HavingDefault<T>(T value) where T : struct;

        /// <summary>
        /// Adds a default value to the column.
        /// </summary>
        IExistingTableBase HavingDefault(string value);

        /// <summary>
        /// Adds a default value to the column which is dropped after adding the column to the table.
        /// Use this method to fill a non-nullable column with default values.
        /// </summary>
        IExistingTableBase HavingTemporaryDefault<T>(T value) where T : struct;

        /// <summary>
        /// Adds a default value to the column which is dropped after adding the column to the table.
        /// Use this method to fill a non-nullable column with default values.
        /// </summary>
        IExistingTableBase HavingTemporaryDefault(string value);
    }

    /// <summary>
    /// Contains the extensions methods for the <see cref="IExistingTableWithAddedColumn"/> interface.
    /// </summary>
    public static class ExistingTableWithAddedColumnExtensions
    {
        /// <summary>
        /// Specifies the size of the data type of the column.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="size">The length for character data types or the maximum total number of decimal digits for numeric data types.</param>
        public static IExistingTableWithAddedColumn OfSize(this IExistingTableWithAddedColumn column, int size)
        {
            return column.OfSize(size, null);
        }

        /// <summary>
        /// Sets the default of the column to be the current system time of the database server.
        /// </summary>
        public static IExistingTableBase HavingCurrentDateTimeAsDefault(this IExistingTableWithAddedColumn column)
        {
            return column.HavingDefault(SpecialDefaultValue.CurrentDateTime);
        }

        /// <summary>
        /// Sets the default of the column to be the current UTC system time of the database server.
        /// </summary>
        public static IExistingTableBase HavingCurrentUtcDateTimeAsDefault(this IExistingTableWithAddedColumn column)
        {
            return column.HavingDefault(SpecialDefaultValue.CurrentUtcDateTime);
        }

        /// <summary>
        /// Sets the default of the column to be the current system time including the timezone offset of the database server.
        /// </summary>
        public static IExistingTableBase HavingCurrentDateTimeOffsetAsDefault(this IExistingTableWithAddedColumn column)
        {
            return column.HavingDefault(SpecialDefaultValue.CurrentDateTimeOffset);
        }
    }
}