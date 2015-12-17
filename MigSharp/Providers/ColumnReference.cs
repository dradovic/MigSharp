namespace MigSharp.Providers
{
    /// <summary>
    /// Represents a pair of column names which is used as an element of a foreign key relationship.
    /// </summary>
    internal class ColumnReference
    {
        private readonly string _columnName;
        private readonly string _referencedColumnName;

        /// <summary>
        /// Gets the name of the referencing column.
        /// </summary>
        public string ColumnName { get { return _columnName; } }

        /// <summary>
        /// Gets the name of the referenced column.
        /// </summary>
        public string ReferencedColumnName { get { return _referencedColumnName; } }

        internal ColumnReference(string columnName, string referencedColumnName)
        {
            _columnName = columnName;
            _referencedColumnName = referencedColumnName;
        }
    }
}