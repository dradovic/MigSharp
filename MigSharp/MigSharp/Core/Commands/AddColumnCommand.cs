using System.Data;

namespace MigSharp.Core.Commands
{
    internal class AddColumnCommand : Command
    {
        private readonly string _name;
        private readonly DbType _type;
        private readonly bool _isNullable;

        public string Name { get { return _name; } }
        public DbType Type { get { return _type; } }
        public bool IsNullable { get { return _isNullable; } }

        public object DefaultValue { get; set; }
        public bool DropThereafter { get; set; }
        public int Length { get; set; }

        public AddColumnCommand(ICommand parent, string name, DbType type, bool isNullable)
            : base(parent)
        {
            _name = name;
            _type = type;
            _isNullable = isNullable;
        }
    }
}