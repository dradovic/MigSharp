using System;

namespace MigSharp.Process
{
    internal static class MetaDataExtensions
    {
        public static DateTime Timestamp(this IMigrationMetaData metaData)
        {
            return new DateTime(metaData.Year, metaData.Month, metaData.Day, metaData.Hour, metaData.Minute, metaData.Second);
        }
    }
}