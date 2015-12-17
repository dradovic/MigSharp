using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingSchema : IExistingSchema
    {
        private readonly AlterSchemaCommand _command;
        private readonly TableCollection _tables;

        public ExistingSchema(AlterSchemaCommand command)
        {
            _command = command;
            _tables = new TableCollection(command);
        }

        public IExistingTableCollection Tables { get { return _tables; } }

        public ICreatedTable CreateTable(string tableName, string primaryKeyConstraintName)
        {
            var createTableCommand = new CreateTableCommand(_command, tableName, primaryKeyConstraintName);
            _command.Add(createTableCommand);
            return new CreatedTable(createTableCommand);
        }

        public void Drop()
        {
            _command.Add(new DropCommand(_command));
        }
    }
}