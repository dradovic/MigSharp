using System.Data;

namespace MigSharp.Core.Commands
{
    internal class AddColumnCommand : Command
    {
        private readonly string _name;
        private readonly DbType _type;
        private readonly bool _isNullable;
        private object _defaultValue;
        private bool _dropThereafter;

        public string Name { get { return _name; } }
        public DbType Type { get { return _type; } }
        public bool IsNullable { get { return _isNullable; } }
        public object DefaultValue { get { return _defaultValue; } set { _defaultValue = value; } }
        public bool DropThereafter { get { return _dropThereafter; } set { _dropThereafter = value; } }

        public AddColumnCommand(ICommand parent, string name, DbType type, bool isNullable)
            : base(parent)
        {
            _name = name;
            _type = type;
            _isNullable = isNullable;
        }
    }
}