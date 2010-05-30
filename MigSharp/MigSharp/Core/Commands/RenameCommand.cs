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

        public IEnumerable<string> Script(IProvider provider)
        {
            AlterTableCommand parentAlterTableCommand = Parent as AlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand = Parent as AlterColumnCommand;
            if (parentAlterTableCommand == null && parentAlterColumnCommand == null) throw new InvalidOperationException("The parent command of a RenameCommand must either be a alterColumnCommand or a alterTableCommand.");

            if ((parentAlterTableCommand) != null)
            {
                return provider.RenameTable(parentAlterTableCommand.TableName, _newName);
            }
            else
            {
                parentAlterTableCommand = parentAlterColumnCommand.Parent as AlterTableCommand;
                if (parentAlterTableCommand == null) throw new InvalidOperationException("The parent command of the AlterColumnCommand must be a AlterTableCommand."); // TODO: make sure this is true by design
                return provider.RenameColumn(parentAlterTableCommand.TableName, parentAlterColumnCommand.ColumnName, _newName);
            }
        }
    }
}