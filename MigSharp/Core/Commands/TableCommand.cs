namespace MigSharp.Core.Commands
{
    internal abstract class TableCommand : Command
    {
        private readonly string _tableName;
        public string TableName { get { return _tableName; } }

        public string Schema
        {
            get
            {
                var alterSchemaCommand = Parent as AlterSchemaCommand;
                return alterSchemaCommand != null ? alterSchemaCommand.Schema : null;
            }
        }

        protected TableCommand(MigrateCommand parent, string tableName)
            : this((Command)parent, tableName)
        {
        }

        protected TableCommand(AlterSchemaCommand parent, string tableName)
            : this((Command)parent, tableName)
        {
        }

        private TableCommand(Command parent, string tableName)
            : base(parent)
        {
            _tableName = tableName;
        }
    }
}