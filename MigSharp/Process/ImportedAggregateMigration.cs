using MigSharp.Core;

namespace MigSharp.Process
{
    internal class ImportedAggregateMigration
    {
        private readonly IMigration _implementation;
        private readonly IAggregateMigrationMetadata _metadata;

        public IMigration Implementation { get { return _implementation; } }
        public IAggregateMigrationMetadata Metadata { get { return _metadata; } }

        public ImportedAggregateMigration(IMigration implementation, IAggregateMigrationMetadata metadata)
        {
            _implementation = implementation;
            _metadata = metadata;
        }
    }
}