namespace MigSharp.Providers
{
    public class AddedColumn : Column
    {
        private readonly bool _isIdentity;

        public bool IsIdentity { get { return _isIdentity; } }

        public AddedColumn(string name, DataType dataType, bool isNullable, bool isIdentity, object defaultValue)
            : base(name, dataType, isNullable, defaultValue)
        {
            _isIdentity = isIdentity;
        }
    }
}