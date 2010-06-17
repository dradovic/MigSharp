using System.Collections.Generic;
using System.Linq;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class CreateTableCommand : Command, IScriptableCommand
    {
        private readonly string _tableName;

        public bool IfNotExists { get; set; }

        public CreateTableCommand(ICommand parent, string tableName)
            : base(parent)
        {
            _tableName = tableName;
        }

        public IEnumerable<string> Script(IProvider provider)
        {
            IEnumerable<CreateColumnCommand> createColumnCommands = Children.OfType<CreateColumnCommand>();
            if (createColumnCommands.Count() > 0)
            {
                foreach (string commandText in provider.CreateTable(_tableName,
                    createColumnCommands.Select(c => new CreatedColumn(c.ColumnName, c.Type, c.IsNullable, c.IsPrimaryKey, c.Length)), IfNotExists))
                {
                    yield return commandText;
                }
            }
        }
    }
}