using System;
using System.Collections.Generic;

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
        private readonly IProviderMetaData _metaData;

        public CommandScripter(IProvider provider, IProviderMetaData metaData)
        {
            _provider = provider;
            _metaData = metaData;
        }

        /// <summary>
        /// Translates the recorded commands on the <paramref name = "database" /> to a SQL script.
        /// </summary>
        public IEnumerable<string> GetCommandTexts(Database database)
        {
            return Visit(database.Root);
        }

        private IEnumerable<string> Visit(ICommand command)
        {
            IScriptableCommand scriptableCommand = command as IScriptableCommand;
            if (scriptableCommand != null)
            {
                foreach (string commandText in scriptableCommand.Script(_provider, _metaData))
                {
                    yield return commandText;
                }
            }

            foreach (ICommand child in command.Children)
            {
                foreach (string commandText in Visit(child))
                {
                    yield return commandText;
                }
            }
        }
    }
}