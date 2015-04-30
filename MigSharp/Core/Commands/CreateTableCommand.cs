using System.Collections.Generic;
using System.Data;
using System.Linq;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class CreateTableCommand : Command, ITranslatableCommand
    {
        private readonly string _tableName;
        private readonly string _primaryKeyConstraintName;

        public string TableName { get { return _tableName; } }

        public CreateTableCommand(ICommand parent, string tableName, string primaryKeyConstraintName)
            : base(parent)
        {
            _tableName = tableName;
            _primaryKeyConstraintName = primaryKeyConstraintName;
        }

        public IEnumerable<string> ToSql(IProvider provider, IRuntimeContext context)
        {
            string effectivePkConstraintName = GetEffectivePkConstraintName();
            List<CreateColumnCommand> createColumnCommands = GetCreateColumnCommands().ToList();
            if (createColumnCommands.Count == 0)
            {
                throw new InvalidCommandException("At least one column must be added to the CreateTable command.");
            }
            return provider.CreateTable(
                _tableName,
                createColumnCommands.Select(c => new CreatedColumn(
                                                     c.ColumnName,
                                                     new DataType(c.Type, c.Size, c.Scale),
                                                     c.IsNullable,
                                                     c.IsPrimaryKey,
                                                     GetEffectiveUniqueConstraintName(c),
                                                     c.IsIdentity,
                                                     c.DefaultValue,
                                                     c.IsRowVersion)),
                effectivePkConstraintName);
        }

        private IEnumerable<CreateColumnCommand> GetCreateColumnCommands()
        {
            return Children.OfType<CreateColumnCommand>();
        }

        private string GetEffectivePkConstraintName()
        {
            if (GetCreateColumnCommands().Where(c => c.IsPrimaryKey).Any())
            {
                return DefaultObjectNameProvider.GetPrimaryKeyConstraintName(TableName, _primaryKeyConstraintName);
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetEffectiveUniqueConstraintName(CreateColumnCommand c)
        {
            return c.IsUnique ? DefaultObjectNameProvider.GetUniqueConstraintName(TableName, c.ColumnName, c.UniqueConstraint) : string.Empty;
        }
    }
}