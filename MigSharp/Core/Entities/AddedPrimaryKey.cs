using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class AddedPrimaryKey : IAddedPrimaryKey
    {
        private readonly AddPrimaryKeyCommand _command;

        public AddedPrimaryKey(AddPrimaryKeyCommand command)
        {
            _command = command;
        }

        public IAddedPrimaryKey OnColumn(string columnName)
        {
            _command.AddColumn(columnName);
            return this;
        }
    }
}