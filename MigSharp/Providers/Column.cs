namespace MigSharp.Providers
{
    internal class Column
    {
        private readonly string _name;
        private readonly DataType _dataType;
        private readonly bool _isNullable;
        private readonly object _defaultValue;
        private readonly bool _isRowVersion;

        public string Name { get { return _name; } }
        public DataType DataType { get { return _dataType; } }

        public bool IsNullable { get { return _isNullable; } }
        public object DefaultValue { get { return _defaultValue; } }
        public bool IsRowVersion { get { return _isRowVersion; } }

        public Column(string name, DataType dataType, bool isNullable, object defaultValue, bool isRowVersion)
        {
            _name = name;
            _dataType = dataType;
            _isNullable = isNullable;
            _defaultValue = defaultValue;
            _isRowVersion = isRowVersion;
        }
    }
}