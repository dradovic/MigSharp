using System;
using System.ComponentModel.Composition;
using System.Globalization;

namespace MigSharp
{
    /// <summary>
    /// Use this attribute to mark classes as aggregate migrations.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AggregateMigrationExportAttribute : ExportAttribute
    {
        internal new const string ContractName = "AggregateMigration";

        private string _moduleName = MigrationExportAttribute.DefaultModuleName;

        /// <summary>
        /// Gets the name of the module to which this aggregate migration belongs to (see also <seealso cref="MigrationExportAttribute.MaximumModuleNameLength"/>).
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
        /// Initializes a new instance of the attribute.
        /// </summary>
        public AggregateMigrationExportAttribute()
            : base(ContractName, typeof(IMigration))
        {
        }
    }
}