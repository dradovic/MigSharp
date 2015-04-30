using System.Data;

namespace MigSharp.Core.Commands
{
    internal class CreateColumnCommand : Command
    {
        private readonly string _columnName;
        private readonly DbType _type;
        private readonly bool _isNullable;
        private readonly bool _isPrimaryKey;
        private readonly bool _isRowVersion;

        public string ColumnName { get { return _columnName; } }
        public DbType Type { get { return _type; } }
        public bool IsNullable { get { return _isNullable; } }
        public bool IsPrimaryKey { get { return _isPrimaryKey; } }
        public bool IsRowVersion { get { return _isRowVersion; } }

        public int? Size { get; set; }
        public int? Scale { get; set; }
        public bool IsUnique { get; set; }
        public string UniqueConstraint { get; set; }
        public bool IsIdentity { get; set; }

        internal new CreateTableCommand Parent { get { return (CreateTableCommand)base.Parent; } }
        public object DefaultValue { get; set; }

        public CreateColumnCommand(CreateTableCommand parent, string columnName, DbType type, bool isNullable, bool isPrimaryKey, bool isRowVersion)
            : base(parent)
        {
            _columnName = columnName;
            _type = type;
            _isNullable = isNullable;
            _isPrimaryKey = isPrimaryKey;
            _isRowVersion = isRowVersion;
        }
    }
}