using System.Collections.Generic;

namespace MigSharp.Core.Commands
{
    internal class CreateTableCommand : ICommand
    {
        private readonly List<CreateColumnCommand> _createColumnCommands = new List<CreateColumnCommand>();

        public void Add(CreateColumnCommand createColumnCommand)
        {
            _createColumnCommands.Add(createColumnCommand);
        }

        public IEnumerable<ICommand> Children { get { yield break; } }
    }
}