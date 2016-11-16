using System.Data;
using MigSharp.Process;
using MigSharp.Providers;

namespace MigSharp
{
    internal interface IRuntimeConfiguration
    {
        ProviderInfo ProviderInfo { get; }
        ConnectionInfo ConnectionInfo { get; }
        IDbConnectionFactory ConnectionFactory { get; }
        IValidator Validator { get; }
        ISqlDispatcher SqlDispatcher { get; }
    }

    internal static class RuntimeConfigurationExtensions
    {
        public static IDbConnection OpenConnection(this IRuntimeConfiguration configuration)
        {
            return configuration.ConnectionFactory.OpenConnection(configuration.ConnectionInfo);
        }
    }
}