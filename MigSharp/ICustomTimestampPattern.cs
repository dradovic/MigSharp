using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MigSharp
{
    /// <summary>
    /// By implementing this interface in your Migrations you are able to specify 
    /// how MigSharp should parse your type names into timestamps
    /// </summary>
    public interface ICustomTimestampPattern
    {
        /// <summary>
        /// A Func that takes a typename (the name of a Migration class) and returns the timestamp which is encoded in that typename.
        /// </summary>
        /// <returns></returns>
         Func<string, long> GetTimestampParser();
    }
}
