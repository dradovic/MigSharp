using System.Data;

using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class NewTable : INewTable // TODO: merge NewTable and Table
    {
        private readonly CreateTableCommand _createTableCommand;

        internal NewTable(CreateTableCommand createTableCommand)
        {
            _createTableCommand = createTableCommand;
        }

        public INewTable WithPrimaryKeyColumn(string columnName, DbType type)
        {
            _createTableCommand.Add(new CreateColumnCommand(columnName, type, false, true));
            return this;
        }

        public INewTable WithNullableColumn(string columnName, DbType type)
        {
            _createTableCommand.Add(new CreateColumnCommand(columnName, type, true, false));
            return this;
        }
    }
}