using System;
using System.Collections.Generic;
using System.Diagnostics;

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

            AlterTableCommand parentAlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand;
            if ((parentAlterTableCommand = parentCommand as AlterTableCommand) != null)
            {
                return provider.RenameTable(parentAlterTableCommand.TableName, _newName);
            }
            else if ((parentAlterColumnCommand = parentCommand as AlterColumnCommand) != null)
            {
                return provider.RenameColumn(parentAlterColumnCommand.ColumnName, _newName);
            }
            else
            {
                Debug.Assert(false, "The parent command of a RenameNode should either be a alterColumnCommand or a alterTableCommand."); // TODO: make this as an argument check
                return new string[] { };
            }
        }
    }
}