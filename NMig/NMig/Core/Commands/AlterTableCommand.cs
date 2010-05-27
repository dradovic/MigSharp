using System.Collections.Generic;

namespace NMig.Core.Commands
{
    internal class AlterTableCommand : ICommand
    {
        private readonly string _tableName;
        private readonly List<AddColumnCommand> _addColumnCommands = new List<AddColumnCommand>();
        private readonly List<ICommand> _children = new List<ICommand>();

        public string TableName { get { return _tableName; } }
        public List<AddColumnCommand> AddColumnCommands { get { return _addColumnCommands; } }

        public AlterTableCommand(string tableName)
        {
            _tableName = tableName;
        }

        public void Add(AddColumnCommand command)
        {
            _addColumnCommands.Add(command);
        }

        public void Add(AlterColumnCommand command)
        {
            _children.Add(command);
        }

        public void Add(RenameCommand command)
        {
            _children.Add(command);
        }

        public IEnumerable<ICommand> Children { get { return _children; } }
    }
}