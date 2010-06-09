using System;
using System.ComponentModel.Composition;

namespace MigSharp
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MigrationExportAttribute : ExportAttribute
    {
        private readonly DateTime _timestamp;
        public DateTime Timestamp { get { return _timestamp; } }

        public string Name { get; set; }

        public string Module { get; set; }

        public MigrationExportAttribute(DateTime timestamp) : base(typeof(IMigration))
        {
            _timestamp = timestamp;
        }
    }
}