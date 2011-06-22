using System.Collections.Generic;

namespace MigSharp.Process
{
    internal interface IRecordedMigration
    {
        /// <summary>
        /// Gets a list of <see cref="UsedDataType"/>s that were used to create new objects.
        /// </summary>
        IEnumerable<UsedDataType> DataTypes { get; }

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