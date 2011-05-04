using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Process
{
    internal interface IRecordedMigration
    {
        /// <summary>
        /// Gets a list of <see cref="DataType"/>s that were used to create new objects (excluding primary key columns).
        /// </summary>
        IEnumerable<DataType> DataTypes { get; }

        /// <summary>
        /// Gets a list of <see cref="DataType"/>s that were used to create primary key columns.
        /// </summary>
        IEnumerable<DataType> PrimaryKeyDataTypes { get; }

        /// <summary>
        /// Gets a list of names of any created objects.
        /// </summary>
        IEnumerable<string> NewObjectNames { get; }

        /// <summary>
        /// Gets a list of method names that were used for the migration.
        /// </summary>
        IEnumerable<string> Methods { get; }
    }
}