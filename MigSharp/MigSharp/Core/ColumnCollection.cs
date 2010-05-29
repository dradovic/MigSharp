using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class ColumnCollection : AdHocCollection<IExistingColumn>, IExistingColumnCollection
    {
        private readonly ICommand _command;

        public ColumnCollection(ICommand command)
        {
            _command = command;
        }

        protected override IExistingColumn CreateItem(string name)
        {
            AlterColumnCommand alterColumnCommand = new AlterColumnCommand(name);
            _command.Add(alterColumnCommand);
            return new Column(alterColumnCommand);
        }
    }
}