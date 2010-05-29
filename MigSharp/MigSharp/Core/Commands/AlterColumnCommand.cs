using System.Collections.Generic;

namespace MigSharp.Core.Commands
{
    internal class AlterColumnCommand : ICommand
    {
        private readonly string _columnName;
        private readonly List<ICommand> _children = new List<ICommand>();

        public string ColumnName { get { return _columnName; } }
        public IEnumerable<ICommand> Children { get { return _children; } }

        public AlterColumnCommand(string columnName)
        {
            _columnName = columnName;
        }

        public void Add(RenameCommand renameCommand)
        {
            _children.Add(renameCommand);
        }
    }
}