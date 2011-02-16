using System;
using System.Collections.Generic;
using System.Linq;

using MigSharp.Core.Entities;
using MigSharp.Providers;

namespace MigSharp.Core
{
    /// <summary>
    /// Visits all commands executed against a <see cref="IDatabase"/> and scripts them out in SQL
    /// using a specific <see cref="IProvider"/>.
    /// </summary>
    internal class CommandScripter
    {
        private readonly IProvider _provider;

        public CommandScripter(IProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Translates the recorded commands on the <paramref name = "database" /> to a SQL script.
        /// </summary>
        public IEnumerable<string> GetCommandTexts(Database database, IRuntimeContext context)
        {
            return Visit<IScriptableCommand, IEnumerable<string>>(database.Root, c => c.Script(_provider, context)).SelectMany(c => c);
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