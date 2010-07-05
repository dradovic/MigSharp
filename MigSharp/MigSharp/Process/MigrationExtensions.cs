using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MigSharp.Process
{
    internal static class MigrationExtensions
    {
        public static long GetTimestamp(this Type migration)
        {
            if (migration == null) throw new ArgumentNullException("migration");

            Match match = Regex.Match(migration.Name, @"(\d+)$");
            if (!match.Success)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Could not derive timestamp from type name ({0}). Types implementing migrations must be post-fixed with a number.", migration.Name));
            }
            return long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }
    }
}