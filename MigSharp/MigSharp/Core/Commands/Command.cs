using System.Collections.Generic;

namespace MigSharp.Core.Commands
{
    internal abstract class Command : ICommand
    {
        private readonly ICommand _parent;
        private readonly List<ICommand> _children = new List<ICommand>();

        protected internal ICommand Parent { get { return _parent; } }

        protected Command(ICommand parent)
        {
            _parent = parent;
        }

        public void Add(ICommand child)
        {
            _children.Add(child);
        }

        public IEnumerable<ICommand> Children { get { return _children; } }
    }
}