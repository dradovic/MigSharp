namespace MigSharp.Core.Commands
{
    internal class AlterTableCommand : TableCommand
    {
        public AlterTableCommand(MigrateCommand parent, string tableName)
            : base(parent, tableName)
        {
        }

        public AlterTableCommand(AlterSchemaCommand parent, string tableName)
            : base(parent, tableName)
        {
        }
    }
}