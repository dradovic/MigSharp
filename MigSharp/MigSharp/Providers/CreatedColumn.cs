using System.Data;

namespace MigSharp.Providers
{
    public class CreatedColumn
    {
        private readonly string _name;
        private readonly DbType _type;
        private readonly bool _isNullable;
        private readonly bool _isPrimaryKey;

        public string Name { get { return _name; } }
        public DbType Type { get { return _type; } }
        public bool IsNullable { get { return _isNullable; } }
        public bool IsPrimaryKey { get { return _isPrimaryKey; } }

        public CreatedColumn(string name, DbType type, bool isNullable, bool isPrimaryKey)
        {
            _name = name;
            _isNullable = isNullable;
            _isPrimaryKey = isPrimaryKey;
            _type = type;
        }
    }
}