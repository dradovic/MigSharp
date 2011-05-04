using System.Data;
using System.Linq;

using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class CreatedTable : ICreatedTable, ICreatedTableWithAddedColumn
    {
        private readonly CreateTableCommand _command;

        public CreatedTable(CreateTableCommand command)
        {
            _command = command;
        }

        string ICreatedTableBase.TableName { get { return _command.TableName; } }

        ICreatedTableWithAddedColumn ICreatedTableBase.WithPrimaryKeyColumn(string columnName, DbType type)
        {
            _command.Add(new CreateColumnCommand(_command, columnName, type, false, true));
            return this;
        }

        ICreatedTableWithAddedColumn ICreatedTableBase.WithNotNullableColumn(string columnName, DbType type)
        {
            _command.Add(new CreateColumnCommand(_command, columnName, type, false, false));
            return this;
        }

        ICreatedTableWithAddedColumn ICreatedTableBase.WithNullableColumn(string columnName, DbType type)
        {
            _command.Add(new CreateColumnCommand(_command, columnName, type, true, false));
            return this;
        }

        string ICreatedTableWithAddedColumn.ColumnName
        {
            get
            {
                var command = (CreateColumnCommand)_command.Children.Last();
                return command.ColumnName;
            }
        }

        ICreatedTableWithAddedColumn ICreatedTableWithAddedColumn.OfSize(int size, int scale)
        {
            var command = (CreateColumnCommand)_command.Children.Last();
            command.Size = size;
            command.Scale = scale;
            return this;
        }

        ICreatedTableWithAddedColumn ICreatedTableWithAddedColumn.Unique(string constraintName)
        {
            var command = (CreateColumnCommand)_command.Children.Last();
            command.IsUnique = true;
            command.UniqueConstraint = constraintName;
            return this;
        }

        ICreatedTableWithAddedColumn ICreatedTableWithAddedColumn.AsIdentity()
        {
            var command = (CreateColumnCommand)_command.Children.Last();
            command.IsIdentity = true;
            return this;
        }

        public ICreatedTableWithAddedColumn HavingDefault<T>(T value) where T : struct
        {
            return HavingDefault((object)value);
        }

        public ICreatedTableWithAddedColumn HavingDefault(string value)
        {
            return HavingDefault((object)value);
        }

        private ICreatedTableWithAddedColumn HavingDefault(object value)
        {
            var command = (CreateColumnCommand)_command.Children.Last();
            command.DefaultValue = value;
            return this;
        }
    }
}