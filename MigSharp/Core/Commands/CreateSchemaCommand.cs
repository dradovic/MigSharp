using System.Collections.Generic;
using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class CreateSchemaCommand : Command, ITranslatableCommand
    {
        private readonly string _schemaName;

        public CreateSchemaCommand(MigrateCommand parent, string schemaName)
            : base(parent)
        {
            _schemaName = schemaName;
        }

        public IEnumerable<string> ToSql(IProvider provider, IMigrationContext context)
        {
            return provider.CreateSchema(_schemaName);
        }
    }
}