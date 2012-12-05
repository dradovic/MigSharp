using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MigSharp
{
    /// <summary>
    /// Default timestamp migration provider, expects the class name of the migration to be post-fixed with a number.
    /// </summary>
    public class DefaultMigrationTimestampProvider : IMigrationTimestampProvider
    {
        /// <summary>
        /// Retrieves a timestamp from a migration with a class name that has a post-fixed timestamp.
        /// </summary>
        /// <param name="migration">The migration to retrieve the timestamp for.</param>
        public long GetTimestamp(Type migration)
        {
            if (migration == null) throw new ArgumentNullException("migration");

            var match = Regex.Match(migration.Name, @"(\d+)$");
            if (!match.Success)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Could not derive timestamp from type name ({0}). Types implementing migrations must be post-fixed with a number.", migration.Name));
            }
            return long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }
    }
}
