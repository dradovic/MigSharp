using System.Data;

namespace MigSharp.Core.Commands
{
    internal class CreateColumnCommand : Command
    {
        private readonly string _columnName;
        private readonly DbType _type;
        private readonly bool _isNullable;
        private readonly bool _isPrimaryKey;

        public string ColumnName { get { return _columnName; } }
        public DbType Type { get { return _type; } }
        public bool IsNullable { get { return _isNullable; } }
        public bool IsPrimaryKey { get { return _isPrimaryKey; } }

        public int Length { get; set; }

        public CreateColumnCommand(ICommand parent, string columnName, DbType type, bool isNullable, bool isPrimaryKey)
            : base(parent)
        {
            _columnName = columnName;
            _type = type;
            _isNullable = isNullable;
            _isPrimaryKey = isPrimaryKey;
        }
    }
}