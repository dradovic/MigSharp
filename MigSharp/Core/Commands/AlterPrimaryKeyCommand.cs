using System;

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
            if (string.IsNullOrEmpty(constraintName)) throw new ArgumentException("Empty constraintName.");

            _constraintName = constraintName;
        }
    }
}