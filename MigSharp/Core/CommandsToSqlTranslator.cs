using System;
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
        /// Translates the recorded commands on the <paramref name = "database" /> to SQL commands.
        /// </summary>
        public IEnumerable<string> TranslateToSql(Database database, IRuntimeContext context)
        {
            return Visit<ITranslatableCommand, IEnumerable<string>>(database.Root, c => c.ToSql(_provider, context)).SelectMany(c => c);
        }

        private static IEnumerable<TResult> Visit<TCommand, TResult>(ICommand command, Func<TCommand, TResult> eval) where TCommand : class, ICommand
        {
            TCommand c = command as TCommand;
            if (c != null)
            {
                yield return eval(c);
            }

            foreach (ICommand child in command.Children)
            {
                foreach (TResult value in Visit(child, eval))
                {
                    yield return value;
                }
            }
        }
    }
}