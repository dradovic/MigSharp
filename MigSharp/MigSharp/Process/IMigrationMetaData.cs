using System;

namespace MigSharp.Process
{
    internal interface IMigrationMetaData
    {
        DateTime Timestamp { get; }
    }
}