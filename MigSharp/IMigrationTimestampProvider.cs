using System;

namespace MigSharp
{
    /// <summary>
    /// Responsible for retrieving a timestamp from a migration.
    /// </summary>
    public interface IMigrationTimestampProvider
    {
        /// <summary>
        /// Retrieves a timestamp from a migration.
        /// </summary>
        /// <param name="migration">The migration to retrieve the timestamp for.</param>
        long GetTimestamp(Type migration);
    }
}
