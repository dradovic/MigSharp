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

            Match match = Regex.Match(migration.Name, @"^M_([\d_]+)_\D.+");
            if (!match.Success)
                match = Regex.Match(migration.Name, @"(\d+)$");
            if (!match.Success)
            {
                
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Could not derive timestamp from type name ({0}). Types implementing migrations must either be postfixed with a number or be on the format M_<timestamp_with_underscores_<Description>, for example M_1_AddUsers or M_2011_10_08_1340_AddUsers", migration.Name));
            }
            return long.Parse(match.Groups[1].Value.Replace("_",""), CultureInfo.InvariantCulture);
        }
    }
}