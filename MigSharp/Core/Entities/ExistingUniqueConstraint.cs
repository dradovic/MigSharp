using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingUniqueConstraint : IUniqueConstraint
    {
        private readonly AlterUniqueConstraintCommand _command;

        public ExistingUniqueConstraint(AlterUniqueConstraintCommand command)
        {
            _command = command;
        }

        public void Drop()
        {
            _command.Add(new DropCommand(_command));
        }
    }
}