using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class TableCollection : AdHocCollection<IExistingTable>, IExistingTableCollection
    {
        private readonly ICommand _command;

        internal TableCollection(MigrateCommand command)
        {
            _command = command;
        }

        internal TableCollection(AlterSchemaCommand command)
        {
            _command = command;
        }

        protected override IExistingTable CreateItem(string name)
        {
            AlterTableCommand alterTableCommand;
            MigrateCommand migrateCommand;
            if ((migrateCommand = _command as MigrateCommand) != null)
            {
                alterTableCommand = new AlterTableCommand(migrateCommand, name);
            }
            else
            {
                alterTableCommand = new AlterTableCommand((AlterSchemaCommand)_command, name);
            }
            _command.Add(alterTableCommand);
            return new ExistingTable(alterTableCommand);
        }
    }
}