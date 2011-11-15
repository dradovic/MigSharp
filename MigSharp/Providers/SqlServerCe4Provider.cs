using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MigSharp.Providers
{
    /// <summary>
    /// MigSharp provider for Microsoft SQL Compact Edition 4.0.
    /// </summary>
    [ProviderExport(ProviderNames.SqlServerCe4, InvariantName, MaximumDbObjectNameLength = 128)]
    internal class SqlServerCe4Provider : SqlServerCeProviderBase
    {
        private const string InvariantName = "System.Data.SqlServerCe.4.0";
    }
}