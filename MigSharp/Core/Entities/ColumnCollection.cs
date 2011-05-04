using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ColumnCollection : AdHocCollection<IExistingColumn>, IExistingColumnCollection
    {
        private readonly AlterTableCommand _command;

        public ColumnCollection(AlterTableCommand command)
        {
            _command = command;
        }

        protected override IExistingColumn CreateItem(string name)
        {
            var command = new AlterColumnCommand(_command, name);
            _command.Add(command);
            return new ExistingColumn(command);
        }
    }
}