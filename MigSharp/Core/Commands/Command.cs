using System.Collections.Generic;

namespace MigSharp.Core.Commands
{
    internal abstract class Command : ICommand
    {
        private readonly Command _parent;
        private readonly List<ICommand> _children = new List<ICommand>();
        private readonly List<ITranslatableCommand> _translatableCommands = new List<ITranslatableCommand>(); 

        protected internal ICommand Parent { get { return _parent; } }

        protected Command(Command parent)
        {
            _parent = parent;
        }

        public void Add(ICommand child)
        {
            _children.Add(child);
            var translatableCommand = child as ITranslatableCommand;
            if (translatableCommand != null)
            {
                AddTranslatableCommand(translatableCommand);
            }
        }

        protected void AddTranslatableCommand(ITranslatableCommand command)
        {
            if (_parent != null)
            {
                _parent.AddTranslatableCommand(command);
            }
            else
            {
                _translatableCommands.Add(command);
            }
        }

        public IEnumerable<ICommand> Children { get { return _children; } }

        public IEnumerable<ITranslatableCommand> TranslatableCommands { get { return _translatableCommands; } }
    }
}