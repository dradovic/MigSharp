using System;
using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class RenameCommand : Command, IScriptableCommand
    {
        private readonly string _newName;

        public RenameCommand(string newName)
        {
            _newName = newName;
        }

        public IEnumerable<string> Script(IProvider provider, ICommand parentCommand)
        {
            if (parentCommand == null) throw new ArgumentNullException("parentCommand");
            AlterTableCommand parentAlterTableCommand = parentCommand as AlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand = parentCommand as AlterColumnCommand;
            if (parentAlterTableCommand == null && parentAlterColumnCommand == null) throw new ArgumentException("The parent command of a RenameNode should either be a alterColumnCommand or a alterTableCommand.");

            if ((parentAlterTableCommand) != null)
            {
                return provider.RenameTable(parentAlterTableCommand.TableName, _newName);
            }
            else
            {
                return provider.RenameColumn(parentAlterColumnCommand.ColumnName, _newName);
            }
        }
    }
}