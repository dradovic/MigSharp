using System;
using JetBrains.Annotations;

namespace MigSharp.Core
{
    /// <summary>
    /// Represents a fully qualified table name.
    /// </summary>
    internal class TableName
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        [NotNull]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the schema name of the table if specified, or null otherwise.
        /// </summary>
        public string Schema { get; private set; }

        public TableName(string name, string schema)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("The table name cannot be null or empty.", "name");
            
            Name = name;
            Schema = schema;
        }

        /// <summary>
        /// Used for debugging puproses only. The outcome of this method should not be used to build SQL queries.
        /// </summary>
        public override string ToString()
        {
            return (Schema != null ? Schema + "." : string.Empty) + Name;
        }
    }
}