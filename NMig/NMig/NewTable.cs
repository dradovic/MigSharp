using System.Data;

using NMig.Core.Commands;

namespace NMig
{
    public class NewTable
    {
        private readonly string _name;
        private readonly CreateTableCommand _createTableCommand;

        internal NewTable(string name, CreateTableCommand createTableCommand)
        {
            _name = name;
            _createTableCommand = createTableCommand;
        }

        public NewTable WithPrimaryKeyColumn(string columnName, DbType type)
        {
            _createTableCommand.Add(new CreateColumnCommand(columnName, type, true));
            return this;
        }

        public NewTable WithNullableColumn(string columnName, DbType type)
        {
            _createTableCommand.Add(new CreateColumnCommand(columnName, type, false));
            return this;
        }
    }
}