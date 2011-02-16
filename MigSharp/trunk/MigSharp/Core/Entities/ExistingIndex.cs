using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingIndex : IIndex
    {
        private readonly AlterIndexCommand _command;

        public ExistingIndex(AlterIndexCommand command)
        {
            _command = command;
        }

        public void Drop()
        {
            _command.Add(new DropCommand(_command));
        }
    }
}