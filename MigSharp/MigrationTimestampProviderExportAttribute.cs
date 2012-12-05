using System;
using System.ComponentModel.Composition;
using System.Globalization;

namespace MigSharp
{
    /// <summary>
    /// Use this attribute to mark a class as a timestamp provider.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MigrationTimestampProviderExportAttribute : ExportAttribute
    {
        private string _moduleName = MigrationExportAttribute.DefaultModuleName;

        /// <summary>
        /// Gets the name of the module which is handled by this timestamp provider (see also <seealso cref="MigrationExportAttribute.MaximumModuleNameLength"/>).
        /// </summary>
        public string ModuleName
        {
            get { return _moduleName; }
            set
            {
                if (value != null && value.Length > MigrationExportAttribute.MaximumModuleNameLength) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The provided module name exceeds the maximum length of {0}.", MigrationExportAttribute.MaximumModuleNameLength));
                _moduleName = string.IsNullOrEmpty(value) ? MigrationExportAttribute.DefaultModuleName : value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the attribute
        /// </summary>
        public MigrationTimestampProviderExportAttribute() : base(typeof(IMigrationTimestampProvider))
        {
        }
    }
}
