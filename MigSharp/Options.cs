using System.Diagnostics;
using MigSharp.Core;

namespace MigSharp
{
    /// <summary>
    /// Access to the static options of Mig#.
    /// </summary>
    public static class Options
    {
        ///<summary>
        /// Sets the level of general information being traced.
        ///</summary>
        public static void SetGeneralTraceLevel(SourceLevels sourceLevels)
        {
            Log.SetTraceLevel(LogCategory.General, sourceLevels);
        }

        ///<summary>
        /// Sets the level of SQL information being traced.
        ///</summary>
        public static void SetSqlTraceLevel(SourceLevels sourceLevels) // signature used in a Wiki example
        {
            Log.SetTraceLevel(LogCategory.Sql, sourceLevels);
        }

        ///<summary>
        /// Sets the level of performance information being traced.
        ///</summary>
        public static void SetPerformanceTraceLevel(SourceLevels sourceLevels)
        {
            Log.SetTraceLevel(LogCategory.Performance, sourceLevels);
        }
    }
}