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
        private readonly int _length;

        public string Name { get { return _name; } }
        public DbType DbType { get { return _dbType; } }
        public bool IsNullable { get { return _isNullable; } }
        public object DefaultValue { get { return _defaultValue; } }
        public bool DropThereafter { get { return _dropThereafter; } }
        public int Length { get { return _length; } }

        public AddedColumn(string name, DbType dbType, bool isNullable, object defaultValue, bool dropThereafter, int length)
        {
            _name = name;
            _length = length;
            _dbType = dbType;
            _isNullable = isNullable;
            _defaultValue = defaultValue;
            _dropThereafter = dropThereafter;
        }
    }
}