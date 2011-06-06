using System;
using System.Collections.Generic;
using System.Globalization;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class RenameCommand : Command, ITranslatableCommand
    {
        private readonly string _newName;

        public RenameCommand(ICommand parent, string newName) 
            : base(parent)
        {
            _newName = newName;
        }

        public IEnumerable<string> ToSql(IProvider provider, IRuntimeContext context)
        {
            AlterTableCommand parentAlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand;
            AlterPrimaryKeyCommand parentAlterPrimaryKeyCommand;
            if ((parentAlterTableCommand = Parent as AlterTableCommand) != null)
            {
                return provider.RenameTable(parentAlterTableCommand.TableName, _newName);
            }
            else if ((parentAlterColumnCommand = Parent as AlterColumnCommand) != null)
            {
                return provider.RenameColumn(parentAlterColumnCommand.Parent.TableName, parentAlterColumnCommand.ColumnName, _newName);
            }
            else if ((parentAlterPrimaryKeyCommand = Parent as AlterPrimaryKeyCommand) != null)
            {
                return provider.RenamePrimaryKey(parentAlterPrimaryKeyCommand.Parent.TableName, parentAlterPrimaryKeyCommand.ConstraintName, _newName);
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unknown parent command of a RenameCommand: {0}.", Parent.GetType()));
            }
        }
    }
}