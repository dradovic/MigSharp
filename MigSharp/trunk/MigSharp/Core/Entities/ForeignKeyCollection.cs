using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ForeignKeyCollection : AdHocCollection<IForeignKey>, IForeignKeyCollection
    {
        private readonly AlterTableCommand _command;

        public ForeignKeyCollection(AlterTableCommand command)
        {
            _command = command;
        }

        protected override IForeignKey CreateItem(string name)
        {
            var command = new AlterForeignKeyCommand(_command, name);
            _command.Add(command);
            return new ExistingForeignKey(command);
        }
    }
}