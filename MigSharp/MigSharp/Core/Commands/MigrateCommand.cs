using System.Collections.Generic;

namespace MigSharp.Core.Commands
{
    internal class MigrateCommand : ICommand
    {
        private readonly List<ICommand> _children = new List<ICommand>();

        public void Add(AlterTableCommand child)
        {
            _children.Add(child);
        }

        public IEnumerable<ICommand> Children { get { return _children; } }

        public void Add(CreateTableCommand child)
        {
            _children.Add(child);
        }
    }
}