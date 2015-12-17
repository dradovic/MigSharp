using System.Collections.Generic;
using System.Linq;
using MigSharp.Core.Entities;
using MigSharp.Providers;

namespace MigSharp.Core
{
    /// <summary>
    /// Visits all commands executed against a <see cref="IDatabase"/> and translates them into SQL
    /// using a specific <see cref="IProvider"/>.
    /// </summary>
    internal class CommandsToSqlTranslator
    {
        private readonly IProvider _provider;

        public CommandsToSqlTranslator(IProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Translates the recorded commands on the <paramref name="database" /> to SQL commands.
        /// </summary>
        public IEnumerable<string> TranslateToSql(Database database, IMigrationContext context)
        {
            return database.Root.TranslatableCommands.SelectMany(c => c.ToSql(_provider, context));
        }
    }
}