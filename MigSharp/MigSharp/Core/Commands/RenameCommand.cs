using System.Collections.Generic;

namespace MigSharp.Core.Commands
{
    internal class RenameCommand : ICommand
    {
        private readonly string _newName;

        public string NewName { get { return _newName; } }

        public RenameCommand(string newName)
        {
            _newName = newName;
        }

        public IEnumerable<ICommand> Children { get { yield break; } }
    }
}