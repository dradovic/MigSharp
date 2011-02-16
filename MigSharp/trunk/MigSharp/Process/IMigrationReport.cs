using System.Collections.Generic;

using MigSharp.Core.Commands;
using MigSharp.Providers;

namespace MigSharp.Process
{
    /// <summary>
    /// Contains information about used data types, longest name, etc. for a specific migration.
    /// </summary>
    internal interface IMigrationReport
    {
        /// <summary>
        /// Gets the full type name of the migration for this report.
        /// </summary>
        string MigrationName { get; }

        /// <summary>
        /// Gets the error message for the migration if there was an <see cref="InvalidCommandException"/>.
        /// </summary>
        string Error { get; }

        /// <summary>
        /// Gets a list of <see cref="DataType"/>s that were used to create new objects (including primary key columns).
        /// </summary>
        IEnumerable<DataType> DataTypes { get; }

        /// <summary>
        /// Gets a list of <see cref="DataType"/>s that were used to create primary key columns.
        /// </summary>
        IEnumerable<DataType> PrimaryKeyDataTypes { get; }

        /// <summary>
        /// Gets the longest name of any created objects.
        /// </summary>
        string LongestName { get; }

        /// <summary>
        /// Gets a list of used provider method names.
        /// </summary>
        IEnumerable<string> Methods { get; }
    }
}