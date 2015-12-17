using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class AddPrimaryKeyCommand : Command, ITranslatableCommand
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

        public IEnumerable<string> ToSql(IProvider provider, IMigrationContext context)
        {
            if (_columnNames.Count == 0)
            {
                throw new InvalidCommandException("At least one column must be added to the AddPrimaryKey command.");
            }
            string effectiveConstraintName = GetEffectiveConstraintName();
            return provider.AddPrimaryKey(new TableName(Parent.TableName, Parent.Schema ?? context.GetDefaultSchema()), _columnNames, effectiveConstraintName);
        }

        private string GetEffectiveConstraintName()
        {
            return DefaultObjectNameProvider.GetPrimaryKeyConstraintName(Parent.TableName, _constraintName);
        }
    }
}