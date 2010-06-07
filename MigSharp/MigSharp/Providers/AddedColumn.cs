using System.Data;

namespace MigSharp.Providers
{
    public class AddedColumn
    {
        private readonly string _name;
        private readonly DbType _type;
        private readonly bool _isNullable;
        private readonly object _defaultValue;
        private readonly bool _dropThereafter;

        public string Name { get { return _name; } }
        public DbType Type { get { return _type; } }
        public bool IsNullable { get { return _isNullable; } }
        public object DefaultValue { get { return _defaultValue; } }
        public bool DropThereafter { get { return _dropThereafter; } }

        public AddedColumn(string name, DbType type, bool isNullable, object defaultValue, bool dropThereafter)
        {
            _name = name;
            _type = type;
            _isNullable = isNullable;
            _defaultValue = defaultValue;
            _dropThereafter = dropThereafter;
        }
    }
}