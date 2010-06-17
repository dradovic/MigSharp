using System;
using System.ComponentModel.Composition;

namespace MigSharp.Providers
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProviderExportAttribute : ExportAttribute
    {
        private readonly string _invariantName;

        public string InvariantName { get { return _invariantName; } }

        public ProviderExportAttribute(string invariantName) : base(typeof(IProvider))
        {
            _invariantName = invariantName;
        }
    }
}