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
            AlterColumnCommand alterColumnCommand = new AlterColumnCommand(_command, name);
            _command.Add(alterColumnCommand);
            return new Column(alterColumnCommand);
        }
    }
}