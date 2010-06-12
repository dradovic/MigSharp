using System;
using System.Globalization;

namespace MigSharp.Providers
{
    internal class ProviderFactory : IProviderFactory
    {
        public IProvider GetProvider(string providerInvariantName)
        {
            // TODO: use MEF (or another DI container) to get rid of this switch
            switch (providerInvariantName)
            {
                case "System.Data.SqlClient":
                    return new SqlServerProvider();
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Cannot find a Provider for the DbProvider '{0}'", providerInvariantName)); // TODO: reformulate message text (Provider -> CommandTextProvider?)
            }
        }
    }
}