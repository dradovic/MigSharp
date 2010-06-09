using System;

namespace MigSharp.Versioning
{
    internal interface IMigrationMetaData
    {
        DateTime Timestamp { get; }
    }
}