using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class AddedIndex : IAddedIndex
    {
        private readonly AddIndexCommand _command;

        public AddedIndex(AddIndexCommand command)
        {
            _command = command;
        }

        public IAddedIndex OnColumn(string columnName)
        {
            _command.AddColumn(columnName);
            return this;
        }
    }
}