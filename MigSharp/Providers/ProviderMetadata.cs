namespace MigSharp.Providers
{
    internal class ProviderMetadata : IProviderMetadata
    {
        public Platform Platform { get; set; }

        public int MajorVersion { get; set; }

        public Driver Driver { get; set; }

        public string InvariantName { get; set; }

        public bool SupportsTransactions { get; set; }

        public string ParameterExpression { get; set; }

        public int MaximumDbObjectNameLength { get; set; }

        public string EnableAnsiQuotesCommand { get; set; }

        public bool PrefixUnicodeLiterals { get; set; }
    }
}