using System.Data;

namespace MigSharp.Providers
{
    public class AddedColumn
    {
        private readonly string _name;
        private readonly DbType _dbType;
        private readonly bool _isNullable;
        private readonly object _defaultValue;
        private readonly bool _dropThereafter;

        public string Name { get { return _name; } }
        public DbType DbType { get { return _dbType; } }
        public bool IsNullable { get { return _isNullable; } }
        public object DefaultValue { get { return _defaultValue; } }
        public bool DropThereafter { get { return _dropThereafter; } }

        public AddedColumn(string name, DbType dbType, bool isNullable, object defaultValue, bool dropThereafter)
        {
            _name = name;
            _dbType = dbType;
            _isNullable = isNullable;
            _defaultValue = defaultValue;
            _dropThereafter = dropThereafter;
        }
    }
}