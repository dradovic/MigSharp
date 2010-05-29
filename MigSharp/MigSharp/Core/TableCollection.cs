using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class TableCollection : AdHocCollection<Table>, ITableCollection
    {
        private readonly MigrateCommand _migrateCommand;

        internal TableCollection(MigrateCommand migrateCommand)
        {
            _migrateCommand = migrateCommand;
        }

        protected override Table CreateItem(string name)
        {
            AlterTableCommand alterTableCommand = new AlterTableCommand(name);
            _migrateCommand.Add(alterTableCommand);
            return new Table(alterTableCommand);
        }
    }
}