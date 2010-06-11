using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace MigSharp.NUnit
{
    public static class ScriptComparer
    {
        public static void AssertAreEqual(IEnumerable<string> expected, IEnumerable<string> actual)
        {
            // ignore newline differences and call ToList in order to get a nice NUnit output 
            // that shows the exact position of a difference
            CollectionAssert.AreEqual(
                expected.Select(s => s.Replace("\r\n", "\n")).ToList(), 
                actual.Select(s => s.Replace("\r\n", "\n")).ToList());
        }
    }
}