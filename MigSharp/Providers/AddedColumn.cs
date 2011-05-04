namespace MigSharp.Providers
{
    public class AddedColumn : Column
    {
        private readonly bool _isIdentity;
        private readonly bool _dropThereafter;

        public bool DropThereafter { get { return _dropThereafter; } }
        public bool IsIdentity { get { return _isIdentity; } }

        public AddedColumn(string name, DataType dataType, bool isNullable, bool isIdentity, object defaultValue, bool dropThereafter)
            : base(name, dataType, isNullable, defaultValue)
        {
            _isIdentity = isIdentity;
            _dropThereafter = dropThereafter;
        }
    }
}