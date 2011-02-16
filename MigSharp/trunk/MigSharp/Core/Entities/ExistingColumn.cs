using System.Data;
using System.Linq;

using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingColumn : IExistingColumn, IAlteredColumn
    {
        private readonly AlterColumnCommand _command;

        internal ExistingColumn(AlterColumnCommand command)
        {
            _command = command;
        }

        public string ColumnName { get { return _command.ColumnName; } }

        public string TableName { get { return _command.Parent.TableName; } }

        void IExistingColumn.Rename(string newName)
        {
            _command.Add(new RenameCommand(_command, newName));
        }

        void IExistingColumn.Drop()
        {
            _command.Add(new DropCommand(_command));
        }

        IAlteredColumn IExistingColumn.AlterToNullable(DbType dbType)
        {
            _command.Add(new AlterColumnDefinitionCommand(_command, dbType, true));
            return this;
        }

        IAlteredColumn IExistingColumn.AlterToNotNullable(DbType dbType)
        {
            _command.Add(new AlterColumnDefinitionCommand(_command, dbType, false));
            return this;
        }

        IAlteredColumn IAlteredColumn.OfSize(int size, int scale)
        {
            var command = (AlterColumnDefinitionCommand)_command.Children.Last();
            command.Size = size;
            command.Scale = scale;
            return this;
        }

        IAlteredColumn IAlteredColumn.HavingDefault<T>(T value)
        {
            return HavingDefault(value);
        }

        IAlteredColumn IAlteredColumn.HavingDefault(string value)
        {
            return HavingDefault(value);
        }

        private IAlteredColumn HavingDefault(object value)
        {
            var command = (AlterColumnDefinitionCommand)_command.Children.Last();
            command.DefaultValue = value;
            return this;
        }
    }
}