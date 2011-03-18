using System;
using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class DropCommand : Command, IScriptableCommand
    {
        public DropCommand(ICommand parent) : base(parent)
        {
        }

        public IEnumerable<string> Script(IProvider provider, IRuntimeContext context)
        {
            AlterTableCommand parentAlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand;
            AlterPrimaryKeyCommand parentAlterPrimaryKeyCommand;
            AlterIndexCommand parentAlterIndexCommand;
            AlterUniqueConstraintCommand parentAlterUniqueConstraintCommand;
            AlterForeignKeyCommand parentAlterForeignKeyCommand;
            if ((parentAlterTableCommand = Parent as AlterTableCommand) != null)
            {
                return provider.DropTable(parentAlterTableCommand.TableName);
            }
            else if ((parentAlterColumnCommand = Parent as AlterColumnCommand) != null)
            {
                return provider.DropColumn(parentAlterColumnCommand.Parent.TableName, parentAlterColumnCommand.ColumnName);
            }
            else if ((parentAlterPrimaryKeyCommand = Parent as AlterPrimaryKeyCommand) != null)
            {
                string effectiveConstraintName = DefaultObjectNameProvider.GetPrimaryKeyConstraintName(parentAlterPrimaryKeyCommand.Parent.TableName, parentAlterPrimaryKeyCommand.ConstraintName);
                return provider.DropPrimaryKey(parentAlterPrimaryKeyCommand.Parent.TableName, effectiveConstraintName);
            }
            else if ((parentAlterIndexCommand = Parent as AlterIndexCommand) != null)
            {
                return provider.DropIndex(parentAlterIndexCommand.Parent.TableName, parentAlterIndexCommand.IndexName);
            }
            else if ((parentAlterUniqueConstraintCommand = Parent as AlterUniqueConstraintCommand) != null)
            {
                return provider.DropUniqueConstraint(parentAlterUniqueConstraintCommand.Parent.TableName, parentAlterUniqueConstraintCommand.ConstraintName);                
            }
            else if ((parentAlterForeignKeyCommand = Parent as AlterForeignKeyCommand) != null)
            {
                return provider.DropForeignKey(parentAlterForeignKeyCommand.Parent.TableName, parentAlterForeignKeyCommand.ConstraintName);
            }
            else
            {
                throw new InvalidOperationException("Unsupported parent command of a DropCommand.");                
            }
        }
    }
}