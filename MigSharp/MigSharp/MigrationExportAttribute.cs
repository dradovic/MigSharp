using System;
using System.ComponentModel.Composition;
using System.Globalization;

namespace MigSharp
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MigrationExportAttribute : ExportAttribute
    {
        public const int MaximumModuleNameLength = 250;

        private string _moduleName = string.Empty;
        public string ModuleName
        {
            get { return _moduleName; }
            set
            {
                if (value != null && value.Length > MaximumModuleNameLength) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The provided module name exceeds the maximum length of {0}.", MaximumModuleNameLength));
                _moduleName = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string Tag { get; set; }

        /// <summary>
        /// Initializes a new instance of the attribute.
        /// </summary>
        public MigrationExportAttribute()
            : base(typeof(IMigration))
        {
        }
    }
}