using System;
using System.ComponentModel.Composition;
using System.Globalization;

namespace MigSharp
{
    /// <summary>
    /// Use this attribute to mark classes as migrations.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MigrationExportAttribute : ExportAttribute
    {
        /// <summary>
        /// The maximum string length for a module name.
        /// </summary>
        public const int MaximumModuleNameLength = 250;

        internal const string DefaultModuleName = "Default"; // do not use empty string as Oracle treats empty string as null

        private string _moduleName = DefaultModuleName;

        /// <summary>
        /// Gets the name of the module to which this migration belongs to (see also <seealso cref="MaximumModuleNameLength"/>).
        /// </summary>
        public string ModuleName
        {
            get { return _moduleName; }
            set
            {
                if (value != null && value.Length > MaximumModuleNameLength) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The provided module name exceeds the maximum length of {0}.", MaximumModuleNameLength));
                _moduleName = string.IsNullOrEmpty(value) ? DefaultModuleName : value;
            }
        }

        /// <summary>
        /// Gets the tag associated with this migration.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Indicates if the module name should be used as the default schema if no other schema name was specified for a command. SQL Server only.
        /// <p>
        /// ATTENTION: never change this setting for migration that has already been executed.
        /// </p>
        /// </summary>
        public bool UseModuleNameAsDefaultSchema { get; set; }

        /// <summary>
        /// Initializes a new instance of the attribute.
        /// </summary>
        public MigrationExportAttribute()
            : base(typeof(IMigration))
        {
        }
    }
}