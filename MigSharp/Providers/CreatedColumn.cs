namespace MigSharp.Providers
{
    public class CreatedColumn : Column
    {
        private readonly bool _isPrimaryKey;
        private readonly bool _isIdentity;
        private readonly bool _isRowVersion;
        private readonly string _uniqueConstraint;

        public bool IsPrimaryKey { get { return _isPrimaryKey; } }
        public bool IsIdentity { get { return _isIdentity; } }
        public string UniqueConstraint { get { return _uniqueConstraint; } }
        public bool IsRowVersion { get { return _isRowVersion; } }

        public CreatedColumn(string name, DataType dataType, bool isNullable, bool isPrimaryKey, string uniqueConstraint, bool isIdentity, object defaultValue, bool isRowVersion)
            : base(name, dataType, isNullable, defaultValue)
        {
            _isPrimaryKey = isPrimaryKey;
            _isIdentity = isIdentity;
            _isRowVersion = isRowVersion;
            _uniqueConstraint = uniqueConstraint;
        }
    }
}