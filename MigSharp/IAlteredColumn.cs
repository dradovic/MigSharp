using MigSharp.Providers;

namespace MigSharp
{
    /// <summary>
    /// Represents an existing column whose data type is being altered.
    /// </summary>
    public interface IAlteredColumn
    {
        /// <summary>
        /// Specifies the size and the scale of the new data type of the column.
        /// </summary>
        /// <param name="size">The length for character data types or the maximum total number of decimal digits for numeric data types.</param>
        /// <param name="scale">The maximum number of decimal digits that can be stored to the right of the decimal point. Scale must be a value from 0 through <paramref name="size"/>.</param>
        IAlteredColumn OfSize(int size, int? scale);

        /// <summary>
        /// Adds a default value to the column.
        /// </summary>
        IAlteredColumn HavingDefault<T>(T value) where T : struct;

        /// <summary>
        /// Adds a default value to the column.
        /// </summary>
        IAlteredColumn HavingDefault(string value);
    }

    /// <summary>
    /// Contains the extensions methods for the <see cref="IAlteredColumn"/> interface.
    /// </summary>
    public static class AlteredColumnExtensions
    {
        /// <summary>
        /// Specifies the size of the new data type of the column.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="size">The length for character data types or the maximum total number of decimal digits for numeric data types.</param>
        public static IAlteredColumn OfSize(this IAlteredColumn column, int size)
        {
            return column.OfSize(size, null);
        }

        /// <summary>
        /// Sets the default of the column to be the current system time of the database server.
        /// </summary>
        public static IAlteredColumn HavingCurrentDateTimeAsDefault(this IAlteredColumn column)
        {
            return column.HavingDefault(SpecialDefaultValue.CurrentDateTime);
        }
    }
}