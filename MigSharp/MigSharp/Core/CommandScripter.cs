using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core
{
    internal class CommandScripter
    {
        private readonly IProvider _provider;

        public CommandScripter(IProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Translates the changes recorded on the <paramref name = "database" /> to a SQL script.
        /// </summary>
        public IEnumerable<string> GetCommandTexts(Database database)
        {
            return Visit(database.Root, null);
        }

        /// <summary>
        /// Recursively visits all command nodes and scripts them out against the _provider.
        /// </summary>
        private IEnumerable<string> Visit(ICommand command, ICommand parentCommand)
        {
            IScriptableCommand scriptableCommand = command as IScriptableCommand;
            if (scriptableCommand != null)
            {
                foreach (string commandText in scriptableCommand.Script(_provider, parentCommand))
                {
                    yield return commandText;
                }
            }

            foreach (ICommand child in command.Children)
            {
                foreach (string commandText in Visit(child, command))
                {
                    yield return commandText;
                }
            }
        }
    }
}