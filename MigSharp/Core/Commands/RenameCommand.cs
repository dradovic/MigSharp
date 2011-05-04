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

        public IEnumerable<string> Script(IProvider provider, IRuntimeContext context)
        {
            AlterTableCommand parentAlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand;
            if ((parentAlterTableCommand = Parent as AlterTableCommand) != null)
            {
                return provider.RenameTable(parentAlterTableCommand.TableName, _newName);
            }
            else if ((parentAlterColumnCommand = Parent as AlterColumnCommand) != null)
            {
                return provider.RenameColumn(parentAlterColumnCommand.Parent.TableName, parentAlterColumnCommand.ColumnName, _newName);
            }
            else
            {
                throw new InvalidOperationException("The parent command of a RenameCommand must either be an AlterColumnCommand or an AlterTableCommand.");
            }
        }
    }
}