using System;
using System.Collections.Generic;
using System.Linq;

namespace MigSharp.Core
{
    internal class MigrationStepMetadata : IMigrationStepMetadata
    {
        private readonly string _moduleName;
        private readonly MigrationDirection _direction;
        private readonly bool _useModuleNameAsDefaultSchema;
        private readonly IEnumerable<IMigrationMetadata> _migrationMetadatas;

        public string ModuleName { get { return _moduleName; } }
        public MigrationDirection Direction { get { return _direction; } }
        public bool UseModuleNameAsDefaultSchema { get { return _useModuleNameAsDefaultSchema; } }
        public IEnumerable<IMigrationMetadata> Migrations { get { return _migrationMetadatas; } }

        public MigrationStepMetadata(MigrationDirection direction, bool useModuleNameAsDefaultSchema, IEnumerable<IMigrationMetadata> migrationMetadatas)
        {
            if (!migrationMetadatas.Any()) throw new ArgumentException("No migrations provided.", "migrationMetadatas");

            _moduleName = migrationMetadatas.Select(m => m.ModuleName).Distinct().Single();
            _direction = direction;
            _useModuleNameAsDefaultSchema = useModuleNameAsDefaultSchema;
            _migrationMetadatas = migrationMetadatas;
        }
    }
}