using System.Data.Common;

namespace MigSharp
{
    public interface ICustomQuery
    {
        /// <summary>
        /// Only executes the custom query if the used provider equals to <paramref name="providerInvariantName"/>.
        /// </summary>
        /// <param name="providerInvariantName">See <see cref="DbProviderFactories.GetFactory(string)"/>.</param>
        void IfUsing(string providerInvariantName);
    }
}