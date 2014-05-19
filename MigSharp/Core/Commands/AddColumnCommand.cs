using System.Collections.Generic;
using System.Data;

using MigSharp.Providers;

using System.Linq;

namespace MigSharp.Core.Commands
{
    internal class AddColumnCommand : Command, ITranslatableCommand
    {
        private readonly string _columnName;
        private readonly DbType _type;
        private readonly bool _isNullable;

        public string ColumnName { get { return _columnName; } }
        public DbType Type { get { return _type; } }
        public bool IsNullable { get { return _isNullable; } }

        public object DefaultValue { get; set; }
        public bool DropThereafter { get; set; }
        public int? Size { get; set; }
        public int? Scale { get; set; }

        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }

        public AddColumnCommand(AlterTableCommand parent, string columnName, DbType type, bool isNullable)
            : base(parent)
        {
            _columnName = columnName;
            _type = type;
            _isNullable = isNullable;
        }

        public IEnumerable<string> ToSql(IProvider provider, IRuntimeContext context)
        {
            if (IsNullable && DefaultValue != null)
            {
                throw new InvalidCommandException("Adding nullable columns with default values is not supported: some database platforms (like SQL Server) leave missing values NULL and some update missing values to the default value. Consider adding the column first as not-nullable, and then altering it to nullable.");
            }
            string tableName = Parent.TableName;
            var column = new Column(
                ColumnName,
                new DataType(Type, Size, Scale),
                IsNullable,
                DefaultValue);
            IEnumerable<string> commands = provider.AddColumn(tableName, column);
            if (DropThereafter)
            {
                commands = commands.Concat(provider.DropDefault(tableName, new Column(
                    column.Name,
                    column.DataType,
                    column.IsNullable,
                    null)));
            }
            return commands;
        }
    }
}