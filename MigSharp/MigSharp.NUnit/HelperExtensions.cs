using System;

namespace MigSharp.NUnit
{
    internal static class HelperExtensions
    {
        public static DateTime GetTimestamp(this Type migration)
        {
            MigrationExportAttribute[] attributes = (MigrationExportAttribute[])migration.GetCustomAttributes(typeof(MigrationExportAttribute), false);
            return new DateTime(attributes[0].Year, attributes[0].Month, attributes[0].Day, attributes[0].Hour, attributes[0].Minute, attributes[0].Second);
        }
    }
}