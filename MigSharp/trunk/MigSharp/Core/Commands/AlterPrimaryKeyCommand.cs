namespace MigSharp.Core.Commands
{
    internal class AlterPrimaryKeyCommand : Command
    {
        private readonly string _constraintName;

        public string ConstraintName { get { return _constraintName; } }
        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }

        public AlterPrimaryKeyCommand(AlterTableCommand parent, string constraintName)
            : base(parent)
        {
            _constraintName = constraintName;
        }
    }
}