using System;

namespace MigSharp
{
    /// <summary>
    /// The interface that needs to be implemented in order to define a migration.
    /// Additionally, the <see cref="MigrationExportAttribute"/> must be applied
    /// to a class implementing this interface in order to be recognized as a migration.
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// Applies the required changes to the provided <paramref name="db"/> for this migration.
        /// </summary>
        void Up(IDatabase db);
    }

    internal static class MigrationExtensions
    {
        public static string GetName(this IMigration migration)
        {
            if (migration == null) throw new ArgumentNullException("migration");

            return migration.GetType().FullName;
        }
    }
}