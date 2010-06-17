using System;

namespace MigSharp.Process
{
    internal static class MetadataExtensions
    {
        public static DateTime Timestamp(this IMigrationMetadata metadata)
        {
            return new DateTime(metadata.Year, metadata.Month, metadata.Day, metadata.Hour, metadata.Minute, metadata.Second);
        }
    }
}