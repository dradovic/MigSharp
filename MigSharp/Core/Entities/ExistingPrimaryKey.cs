using System;

using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingPrimaryKey : IExistingPrimaryKey
    {
        private readonly AlterPrimaryKeyCommand _command;

        public ExistingPrimaryKey(AlterPrimaryKeyCommand command)
        {
            _command = command;
        }

        public void Drop()
        {
            _command.Add(new DropCommand(_command));
        }

        public void Rename(string newName)
        {
            _command.Add(new RenameCommand(_command, newName));
        }
    }
}