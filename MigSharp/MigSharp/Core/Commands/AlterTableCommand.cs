using System.Collections.Generic;
using System.Linq;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class AlterTableCommand : Command, IScriptableCommand
    {
        private readonly string _tableName;

        public string TableName { get { return _tableName; } }

        public AlterTableCommand(string tableName)
        {
            _tableName = tableName;
        }

        public IEnumerable<string> Script(IProvider provider, ICommand parentCommand)
        {
            IEnumerable<AddColumnCommand> addColumnCommands = Children.OfType<AddColumnCommand>();
            if (addColumnCommands.Count() > 0)
            {
                foreach (string commandText in provider.AddColumns(TableName,
                    addColumnCommands.Select(c => new AddedColumn(c.Name, c.Type, c.IsNullable, c.DefaultValue, c.Options))))
                {
                    yield return commandText;
                }
            }
        }
    }
}