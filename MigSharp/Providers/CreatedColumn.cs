namespace MigSharp.Providers
{
    internal class CreatedColumn : Column
    {
        private readonly bool _isPrimaryKey;
        private readonly bool _isIdentity;
        private readonly string _uniqueConstraint;

        public bool IsPrimaryKey { get { return _isPrimaryKey; } }
        public bool IsIdentity { get { return _isIdentity; } }
        public string UniqueConstraint { get { return _uniqueConstraint; } }

        public CreatedColumn(string name, DataType dataType, bool isNullable, bool isPrimaryKey, string uniqueConstraint, bool isIdentity, object defaultValue, bool isRowVersion)
            : base(name, dataType, isNullable, defaultValue, isRowVersion)
        {
            _isPrimaryKey = isPrimaryKey;
            _isIdentity = isIdentity;
            _uniqueConstraint = uniqueConstraint;
        }
    }
}