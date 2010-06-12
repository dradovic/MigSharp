using System.Data;

namespace MigSharp.Providers
{
    public class CreatedColumn
    {
        private readonly string _name;
        private readonly DbType _dbType;
        private readonly bool _isNullable;
        private readonly bool _isPrimaryKey;

        public string Name { get { return _name; } }
        public DbType DbType { get { return _dbType; } }
        public bool IsNullable { get { return _isNullable; } }
        public bool IsPrimaryKey { get { return _isPrimaryKey; } }

        public CreatedColumn(string name, DbType dbType, bool isNullable, bool isPrimaryKey)
        {
            _name = name;
            _isNullable = isNullable;
            _isPrimaryKey = isPrimaryKey;
            _dbType = dbType;
        }
    }
}