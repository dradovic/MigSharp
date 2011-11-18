using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MigSharp.NUnit.Integration
{
    internal abstract class CustomPatternMigrationBase : IMigration, ICustomTimestampPattern
    {
        public abstract void Up(IDatabase db);

        public Func<string, long> GetTimestampParser()
        {
            return (typeName) =>
            {
                Match match = Regex.Match(typeName, @"^M_([\d_]+)_\D.+");
                if (match.Success)
                    return long.Parse(match.Groups[1].Value.Replace("_", ""), CultureInfo.InvariantCulture);
                else
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Could not derive timestamp from type name ({0}). Types implementing migrations must either be postfixed with a number or be on the format M_<timestamp_with_underscores_<Description>, for example M_1_AddUsers or M_2011_10_08_1340_AddUsers", typeName));
            };
        }
    }
}
