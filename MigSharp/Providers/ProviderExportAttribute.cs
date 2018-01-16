using System;
using System.Composition;
using System.Data;
using System.Data.Common;

namespace MigSharp.Providers
{
    /// <summary>
    /// Marks the class as an provider implementing <see cref="IProvider"/>.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class ProviderExportAttribute : ExportAttribute
    {
        private readonly Platform _platform;
        private readonly int _majorVersion;
        private readonly Driver _driver;
        private readonly string _invariantName;

        public Platform Platform { get { return _platform; } }

        public int MajorVersion { get { return _majorVersion; } }

        public Driver Driver { get { return _driver; } }

        /// <summary>
        /// Gets the invariant name of the ADO.NET provider needed for <see cref="DbProviderFactories.GetFactory(string)"/>.
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
        /// Gets an indication if Unicode literals should be prefixed such as N'...'.
        /// </summary>
        public bool PrefixUnicodeLiterals { get; set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProviderExportAttribute(Platform platform, int majorVersion, Driver driver, string invariantName)
            : base(typeof(IProvider))
        {
            _platform = platform;
            _majorVersion = majorVersion;
            _driver = driver;
            _invariantName = invariantName;
            SupportsTransactions = true;
            ParameterExpression = "@p";
        }

        public ProviderExportAttribute(Platform platform, int majorVersion, string invariantName)
            : this(platform, majorVersion, Driver.AdoNet, invariantName)
        {
        }
    }
}