using System.Diagnostics;
using System.Globalization;

namespace MigSharp.Core
{
    internal enum LogCategory
    {
        General,
        Sql,
        Performance,
    }

    internal static class Log
    {
        public static void Info(string format, params object[] args)
        {
            Info(LogCategory.General, format, args);
        }

        public static void Info(LogCategory category, string format, params object[] args)
        {
            string prefix = GetCategoryPrefix(category);
            Trace.TraceInformation(string.Format(CultureInfo.CurrentCulture, prefix + format, args));
        }

        private static string GetCategoryPrefix(LogCategory category)
        {
            switch (category)
            {
                case LogCategory.General:
                    return string.Empty;
                default:
                    return string.Format(CultureInfo.CurrentCulture, "{0}: ", category);
            }
        }
    }
}