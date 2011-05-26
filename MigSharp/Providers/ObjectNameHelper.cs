using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using MigSharp.Core;

namespace MigSharp.Providers
{
    internal static class ObjectNameHelper
    {
        private const string Delimiter = "_";
        private const char AbbreviationSymbol = '-';

        /// <summary>
        /// Creates a name with the following structure: '<paramref name="tableName"/>_<paramref name="additionalNames"/>[0]_<paramref name="additionalNames"/>[1]_..._<paramref name="postfix"/>'.
        /// The contained names are shortened such that the complete generated name fits <paramref name="maximumNameLength"/> characters.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="postfix">The postfix to be appended to the name. The postfix will *not* be shortened in any way.</param>
        /// <param name="maximumNameLength">The maximum length of the generated name.</param>
        /// <param name="additionalNames">Additional names that should be included in the result which follow the table name.</param>
        public static string GetObjectName(string tableName, string postfix, int maximumNameLength, params string[] additionalNames)
        {
            var names = new List<string> { tableName };
            names.AddRange(additionalNames);
            string result;
            while ((result = GetObjectName(names, postfix)).Length > maximumNameLength)
            {
                string longestName = names.Longest();
                string shortenedName = Shorten(longestName);
                names[names.IndexOf(longestName)] = shortenedName;
            }
            Debug.Assert(result.Length <= maximumNameLength);
            return result;
        }

        private static string Shorten(string name)
        {
            if (name.Length <= 1) throw new ArgumentException("Cannot shorten anymore.");

            if (name.Length == 2) return name.Substring(0, 1);

            string result;
            int midIndex = name.Length / 2;
            char midChar = name[midIndex];
            if (midChar == AbbreviationSymbol && name.Length % 2 == 1)
            {
                // take away subsequent char from the right side of the abbreviation symbol
                result =  name.Substring(0, midIndex + 1) + name.Substring(midIndex + 2);
            }
            else if (midChar == AbbreviationSymbol && name.Length % 2 == 0)
            {
                // take away preceeding char from the left side of the abbreviation symbol
                result =  name.Substring(0, midIndex - 1) + name.Substring(midIndex);
            }
            else
            {
                // the name does not contain the abbreviation symbol yet (at least not in the middle)
                result = name.Substring(0, midIndex) + AbbreviationSymbol + name.Substring(midIndex + 1); // this does not shorten the string, but the next iteration will
            }

            if (result.Length == 2)
            {
                Debug.Assert(result[1] == AbbreviationSymbol);
                return result.Substring(0, 1); // drop trailing abbreviation symbol
            }
            return result;
        }

        private static string GetObjectName(IEnumerable<string> names, string postfix)
        {
            return string.Join(Delimiter, names.Concat(new[] { postfix }).ToArray()); // .ToArray() is needed to target .NET 3.5
        }
    }
}