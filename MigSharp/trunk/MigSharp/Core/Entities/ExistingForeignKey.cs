using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingForeignKey : IForeignKey
    {
        private readonly AlterForeignKeyCommand _command;

        public ExistingForeignKey(AlterForeignKeyCommand command)
        {
            _command = command;
        }

        public void Drop()
        {
            _command.Add(new DropCommand(_command));
        }
    }
}