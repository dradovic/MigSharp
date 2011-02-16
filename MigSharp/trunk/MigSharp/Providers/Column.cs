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
            string defaultText = defaultValue as string;
            if (defaultText != null)
            {
                _defaultValue = "'" + defaultText.Replace("'", "''") + "'"; // CLEAN: dr, find a better and more generic way to correctly escape the default value: overload quota yes/no
            }
            else
            {
                _defaultValue = defaultValue;
            }
        }
    }
}