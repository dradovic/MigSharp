using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;

namespace MigSharp.Providers
{
    /// <summary>
    /// Marks the class as an provider implementing <see cref="IProvider"/>.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProviderExportAttribute : ExportAttribute
    {
        private readonly string _name;
        private readonly string _invariantName;

        /// <summary>
        /// Gets the unique name of this provider.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets the invariant name of the provider needed for <see cref="DbProviderFactories.GetFactory(string)"/>.
        /// </summary>
        public string InvariantName { get { return _invariantName; } }

        /// <summary>
        /// Gets or sets an indication if the underlying provider supports transactions. True by default.
        /// </summary>
        public bool SupportsTransactions { get; set; }

        /// <summary>
        /// Gets an expression that specifies how <see cref="IDataParameter"/>s are addressed in command texts. The literal 'p' is replaced by the parameter name.
        /// The default is '@p'.
        /// </summary>
        public string ParameterExpression { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of object names within the database. 0 meaning that there is non restriction which is the default.
        /// </summary>
        public int MaximumDbObjectNameLength { get; set; }

        /// <summary>
        /// Gets a command-text to be executed on opening the connection to the database to enable ANSI quoting.
        /// </summary>
        public string EnableAnsiQuotesCommand { get; set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProviderExportAttribute(string name, string invariantName) : base(typeof(IProvider))
        {
            _name = name;
            _invariantName = invariantName;
            SupportsTransactions = true;
            ParameterExpression = "@p";
        }
    }
}