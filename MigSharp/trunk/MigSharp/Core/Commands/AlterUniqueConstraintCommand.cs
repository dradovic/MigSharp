namespace MigSharp.Core.Commands
{
    internal class AlterUniqueConstraintCommand : Command
    {
        private readonly string _constraintName;

        public string ConstraintName { get { return _constraintName; } }
        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }

        public AlterUniqueConstraintCommand(AlterTableCommand command, string constraintName) : base(command)
        {
            _constraintName = constraintName;
        }
    }
}