using System;
using System.Data;
using System.Linq;

using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class CreatedTable : ICreatedTable, ICreatedTableWithAddedColumn
    {
        private readonly CreateTableCommand _command;

        public CreatedTable(CreateTableCommand command)
        {
            _command = command;
        }

        ICreatedTableWithAddedColumn ICreatedTableBase.WithPrimaryKeyColumn(string columnName, DbType type)
        {
            if (type == DbType.String) throw new ArgumentException(string.Format("Not all providers support '{0}' for primary key columns.", type)); // TODO: extract this to a generic validation where providers are asked what they support and what not

            _command.Add(new CreateColumnCommand(_command, columnName, type, false, true));
            return this;
        }

        ICreatedTableWithAddedColumn ICreatedTableBase.WithNullableColumn(string columnName, DbType type)
        {
            _command.Add(new CreateColumnCommand(_command, columnName, type, true, false));
            return this;
        }

        ICreatedTableWithAddedColumn ICreatedTableWithAddedColumn.OfLength(int length)
        {
            var command = (CreateColumnCommand)_command.Children.Last();
            command.Length = length;
            return this;
        }

        ICreatedTableBase ICreatedTable.IfNotExists()
        {
            _command.IfNotExists = true;
            return this;
        }
    }
}