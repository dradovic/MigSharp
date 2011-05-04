using System.Collections.Generic;
using System.Linq;

namespace MigSharp.Core
{
    internal static class StringHelper
    {
        /// <summary>
        /// Returns the longer string.
        /// </summary>
        public static string Longer(string s1, string s2)
        {
            if (s2 == null) return s1;
            if (s1 == null) return s2;
            return s1.Length >= s2.Length ? s1 : s2;
        }

        /// <summary>
        /// Returns the longest string.
        /// </summary>
        public static string Longest(this IEnumerable<string> strings)
        {
            if (strings == null)
            {
                return string.Empty;
            }
            else
            {
                return strings.Aggregate(string.Empty, Longer);
            }
        }
    }
}