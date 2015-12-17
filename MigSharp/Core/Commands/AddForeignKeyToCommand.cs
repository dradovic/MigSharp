using System.Collections.Generic;
using System.Linq;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class AddForeignKeyToCommand : Command, ITranslatableCommand
    {
        private readonly string _referencedTableName;
        private readonly string _constraintName;
        private readonly IList<KeyValuePair<string, string>> _columnNames = new List<KeyValuePair<string, string>>();
        private string _referencedTableSchema;

        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }
        public IList<KeyValuePair<string, string>> ColumnNames { get { return _columnNames; } }
        public bool CascadeOnDelete { get; set; }
        public string ReferencedTableSchema { get { return _referencedTableSchema; } set { _referencedTableSchema = value; } }

        public AddForeignKeyToCommand(AlterTableCommand parent, string referencedTableName, string constraintName)
            : base(parent)
        {
            _referencedTableName = referencedTableName;
            _constraintName = constraintName;
        }

        public IEnumerable<string> ToSql(IProvider provider, IMigrationContext context)
        {
            if (_columnNames.Count == 0)
            {
                throw new InvalidCommandException("At least one column must be added to the AddForeignKeyTo command.");
            }
            string effectiveConstraintName = GetEffectiveConstraintName();
            return provider.AddForeignKey(new TableName(Parent.TableName, Parent.Schema ?? context.GetDefaultSchema()), new TableName(_referencedTableName, ReferencedTableSchema ?? Parent.Schema ?? context.GetDefaultSchema()), _columnNames.Select(p => new ColumnReference(p.Key, p.Value)), effectiveConstraintName, CascadeOnDelete);
        }

        private string GetEffectiveConstraintName()
        {
            return DefaultObjectNameProvider.GetForeignKeyConstraintName(Parent.TableName, _referencedTableName, _constraintName);
        }
    }
}