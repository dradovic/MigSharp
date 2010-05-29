using System.Collections.Generic;

namespace MigSharp.Core.Commands
{
    internal abstract class Command : ICommand
    {
        private readonly List<ICommand> _children = new List<ICommand>();

        public void Add(ICommand child)
        {
            _children.Add(child);
        }

        public IEnumerable<ICommand> Children { get { return _children; } }
    }
}