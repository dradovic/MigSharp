namespace MigSharp.Core.Commands
{
    internal class AlterForeignKeyCommand : Command
    {
        private readonly string _constraintName;

        public string ConstraintName { get { return _constraintName; } }
        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }

        public AlterForeignKeyCommand(AlterTableCommand parent, string constraintName) : base(parent)
        {
            _constraintName = constraintName;
        }
    }
}