using System.Diagnostics;

namespace MigSharp.Core
{
    internal static class Log
    {
        public static void Info(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }
    }
}