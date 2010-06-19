using System;
using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class RenameCommand : Command, IScriptableCommand
    {
        private readonly string _newName;

        public RenameCommand(ICommand parent, string newName) 
            : base(parent)
        {
            _newName = newName;
        }

        public IEnumerable<string> Script(IProvider provider, IProviderMetadata metadata)
        {
            AlterTableCommand parentAlterTableCommand = Parent as AlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand = Parent as AlterColumnCommand;
            if (parentAlterTableCommand == null && parentAlterColumnCommand == null) throw new InvalidOperationException("The parent command of a RenameCommand must either be an AlterColumnCommand or an AlterTableCommand.");

            if ((parentAlterTableCommand) != null)
            {
                return provider.RenameTable(parentAlterTableCommand.TableName, _newName);
            }
            else
            {
                return provider.RenameColumn(parentAlterColumnCommand.Parent.TableName, parentAlterColumnCommand.ColumnName, _newName);
            }
        }
    }
}