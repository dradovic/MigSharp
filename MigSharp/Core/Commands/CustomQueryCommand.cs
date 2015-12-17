using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class CustomQueryCommand : Command, ITranslatableCommand
    {
        private readonly string _query;

        public CustomQueryCommand(MigrateCommand parent, string query) : base(parent)
        {
            _query = query;
        }

        public IEnumerable<string> ToSql(IProvider provider, IMigrationContext context)
        {
            yield return _query;
        }
    }
}