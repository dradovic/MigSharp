using System.Collections.Generic;
using System.Linq;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class CreateTableCommand : TableCommand, ITranslatableCommand
    {
        private readonly string _primaryKeyConstraintName;

        public CreateTableCommand(MigrateCommand parent, string tableName, string primaryKeyConstraintName)
            : base(parent, tableName)
        {
            _primaryKeyConstraintName = primaryKeyConstraintName;
        }

        public CreateTableCommand(AlterSchemaCommand parent, string tableName, string primaryKeyConstraintName)
            : base(parent, tableName)
        {
            _primaryKeyConstraintName = primaryKeyConstraintName;
        }

        public IEnumerable<string> ToSql(IProvider provider, IMigrationContext context)
        {
            string effectivePkConstraintName = GetEffectivePkConstraintName();
            List<CreateColumnCommand> createColumnCommands = GetCreateColumnCommands().ToList();
            if (createColumnCommands.Count == 0)
            {
                throw new InvalidCommandException("At least one column must be added to the CreateTable command.");
            }
            return provider.CreateTable(
                new TableName(TableName, Schema ?? context.GetDefaultSchema()),
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
            if (GetCreateColumnCommands().Any(c => c.IsPrimaryKey))
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