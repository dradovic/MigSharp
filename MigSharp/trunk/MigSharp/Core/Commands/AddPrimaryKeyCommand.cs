using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class AddPrimaryKeyCommand : Command, IScriptableCommand
    {
        private readonly string _constraintName;
        private readonly List<string> _columnNames = new List<string>();

        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }

        public AddPrimaryKeyCommand(AlterTableCommand parent, string constraintName) : base(parent)
        {
            _constraintName = constraintName;
        }

        public void AddColumn(string columnName)
        {
            _columnNames.Add(columnName);
        }

        public IEnumerable<string> Script(IProvider provider, IRuntimeContext context)
        {
            if (_columnNames.Count == 0)
            {
                throw new InvalidCommandException("At least one column must be added to the AddPrimaryKey command.");
            }
            string effectiveConstraintName = GetEffectiveConstraintName();
            return provider.AddPrimaryKey(Parent.TableName, _columnNames, effectiveConstraintName);
        }

        private string GetEffectiveConstraintName()
        {
            return DefaultObjectNameProvider.GetPrimaryKeyConstraintName(Parent.TableName, _constraintName);
        }
    }
}