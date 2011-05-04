using System.Collections.Generic;

using MigSharp.Providers;

using System.Linq;

namespace MigSharp.Core.Commands
{
    internal class AddUniqueConstraintCommand : Command, IScriptableCommand
    {
        private readonly string _constraintName;
        private readonly List<string> _columnNames = new List<string>();

        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }

        public AddUniqueConstraintCommand(AlterTableCommand parent, string constraintName)
            : base(parent)
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
                throw new InvalidCommandException("At least one column must be added to the AddUniqueConstraint command.");
            }
            string effectiveConstraintName = GetEffectiveConstraintName();
            return provider.AddUniqueConstraint(Parent.TableName, _columnNames, effectiveConstraintName);
        }

        private string GetEffectiveConstraintName()
        {
            return DefaultObjectNameProvider.GetUniqueConstraintName(Parent.TableName, _columnNames.First(), _constraintName);
        }
    }
}