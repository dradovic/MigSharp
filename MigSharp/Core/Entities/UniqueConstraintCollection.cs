using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class UniqueConstraintCollection : AdHocCollection<IUniqueConstraint>, IUniqueConstraintCollection
    {
        private readonly AlterTableCommand _command;

        public UniqueConstraintCollection(AlterTableCommand command)
        {
            _command = command;
        }

        protected override IUniqueConstraint CreateItem(string name)
        {
            var command = new AlterUniqueConstraintCommand(_command, name);
            _command.Add(command);
            return new ExistingUniqueConstraint(command);
        }
    }
}