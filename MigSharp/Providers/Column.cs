using System.Data;

using MigSharp.Core;

namespace MigSharp.Providers
{
    public class Column
    {
        private readonly string _name;
        private readonly DataType _dataType;
        private readonly bool _isNullable;
        private readonly object _defaultValue;

        public string Name { get { return _name; } }
        public DataType DataType { get { return _dataType; } }

        public bool IsNullable { get { return _isNullable; } }
        public object DefaultValue { get { return _defaultValue; } }

        public Column(string name, DataType dataType, bool isNullable, object defaultValue)
        {
            _name = name;
            _dataType = dataType;
            _isNullable = isNullable;
            _defaultValue = defaultValue;
        }
    }
}