using System;
using System.Data;
using System.Data.Common;

using MigSharp.Providers;

namespace MigSharp
{
    /// <summary>
    /// Reperesents metadata about an <see cref="IProvider"/>.
    /// </summary>
    public interface IProviderMetadata
    {
        /// <summary>
        /// Gets the unique name of this provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the invariant name of the provider needed for <see cref="DbProviderFactories.GetFactory(string)"/>.
        /// </summary>
        string InvariantName { get; }

        /// <summary>
        /// Gets an indication if the underlying provider supports transactions.
        /// </summary>
        bool SupportsTransactions { get; }

        /// <summary>
        /// Gets an expression that specifies how <see cref="IDataParameter"/>s are addressed in command texts. The literal 'p' is replaced by the parameter name.
        /// </summary>
        string ParameterExpression { get; }

        /// <summary>
        /// Gets the maximum length of object names within the database. 0 meaning that there is non restriction which is the default.
        /// </summary>
        int MaximumDbObjectNameLength { get; }

        /// <summary>
        /// Gets a command-text to be executed on opening the connection to the database to enable ANSI quoting.
        /// </summary>
        string EnableAnsiQuotesCommand { get; }
    }

    internal static class ProviderMetadataExtensions
    {
        public static string GetParameterSpecifier(this IProviderMetadata metadata, IDataParameter parameter)
        {
            if (parameter == null) throw new ArgumentNullException("parameter");
            if (!parameter.ParameterName.StartsWith("@", StringComparison.Ordinal)) throw new ArgumentException("Parameter names must start with an '@'.");

            string name = parameter.ParameterName.Substring(1); // the name itself starts after the @
            return metadata.ParameterExpression.Replace("p", name);
        }

        public static bool UsesPositionalParameters(this IProviderMetadata metadata)
        {
            return !metadata.ParameterExpression.Contains("p");
        }
    }
}