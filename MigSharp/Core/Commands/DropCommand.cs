using System;
using System.Collections.Generic;
using MigSharp.Core.Entities;
using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class DropCommand : Command, ITranslatableCommand
    {
        public Check Check { get; set; }

        public DropCommand(Command parent)
            : base(parent)
        {
        }

        public IEnumerable<string> ToSql(IProvider provider, IMigrationContext context)
        {
            AlterTableCommand parentAlterTableCommand;
            AlterSchemaCommand parentAlterSchemaCommand;
            AlterColumnCommand parentAlterColumnCommand;
            AlterPrimaryKeyCommand parentAlterPrimaryKeyCommand;
            AlterIndexCommand parentAlterIndexCommand;
            AlterUniqueConstraintCommand parentAlterUniqueConstraintCommand;
            AlterForeignKeyCommand parentAlterForeignKeyCommand;
            if ((parentAlterTableCommand = Parent as AlterTableCommand) != null)
            {
                return provider.DropTable(new TableName(parentAlterTableCommand.TableName, parentAlterTableCommand.Schema ?? context.GetDefaultSchema()), Check == Check.IfExists);
            }
            else if ((parentAlterSchemaCommand = Parent as AlterSchemaCommand) != null)
            {
                return provider.DropSchema(parentAlterSchemaCommand.Schema);
            }
            else if ((parentAlterColumnCommand = Parent as AlterColumnCommand) != null)
            {
                return provider.DropColumn(new TableName(parentAlterColumnCommand.Parent.TableName, parentAlterColumnCommand.Parent.Schema ?? context.GetDefaultSchema()), parentAlterColumnCommand.ColumnName);
            }
            else if ((parentAlterPrimaryKeyCommand = Parent as AlterPrimaryKeyCommand) != null)
            {
                string effectiveConstraintName = DefaultObjectNameProvider.GetPrimaryKeyConstraintName(parentAlterPrimaryKeyCommand.Parent.TableName, parentAlterPrimaryKeyCommand.ConstraintName);
                return provider.DropPrimaryKey(new TableName(parentAlterPrimaryKeyCommand.Parent.TableName, parentAlterPrimaryKeyCommand.Parent.Schema ?? context.GetDefaultSchema()), effectiveConstraintName);
            }
            else if ((parentAlterIndexCommand = Parent as AlterIndexCommand) != null)
            {
                return provider.DropIndex(new TableName(parentAlterIndexCommand.Parent.TableName, parentAlterIndexCommand.Parent.Schema ?? context.GetDefaultSchema()), parentAlterIndexCommand.IndexName);
            }
            else if ((parentAlterUniqueConstraintCommand = Parent as AlterUniqueConstraintCommand) != null)
            {
                return provider.DropUniqueConstraint(new TableName(parentAlterUniqueConstraintCommand.Parent.TableName, parentAlterUniqueConstraintCommand.Parent.Schema ?? context.GetDefaultSchema()), parentAlterUniqueConstraintCommand.ConstraintName);
            }
            else if ((parentAlterForeignKeyCommand = Parent as AlterForeignKeyCommand) != null)
            {
                return provider.DropForeignKey(new TableName(parentAlterForeignKeyCommand.Parent.TableName, parentAlterForeignKeyCommand.Parent.Schema ?? context.GetDefaultSchema()), parentAlterForeignKeyCommand.ConstraintName);
            }
            else
            {
                throw new InvalidOperationException("Unsupported parent command of a DropCommand.");
            }
        }
    }
}