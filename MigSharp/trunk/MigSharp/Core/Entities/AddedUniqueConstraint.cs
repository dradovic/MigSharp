using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class AddedUniqueConstraint : IAddedUniqueConstraint
    {
        private readonly AddUniqueConstraintCommand _command;

        public AddedUniqueConstraint(AddUniqueConstraintCommand command)
        {
            _command = command;
        }

        public IAddedUniqueConstraint OnColumn(string columnName)
        {
            _command.AddColumn(columnName);
            return this;
        }
    }
}