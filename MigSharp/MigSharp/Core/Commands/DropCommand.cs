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

        public IEnumerable<string> Script(IProvider provider, IProviderMetadata metadata)
        {
            AlterTableCommand parentAlterTableCommand = Parent as AlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand = Parent as AlterColumnCommand;
            if (parentAlterTableCommand == null && parentAlterColumnCommand == null) throw new InvalidOperationException("The parent command of a DropCommand must either be an AlterColumnCommand or an AlterTableCommand.");

            if ((parentAlterTableCommand) != null)
            {
                return provider.DropTable(parentAlterTableCommand.TableName);
            }
            else
            {
                return provider.DropColumn(parentAlterColumnCommand.Parent.TableName, parentAlterColumnCommand.ColumnName);
            }
        }
    }
}