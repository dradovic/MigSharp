using System;
using System.Collections.Generic;
using System.Globalization;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class RenameCommand : Command, ITranslatableCommand
    {
        private readonly string _newName;

        public RenameCommand(Command parent, string newName) 
            : base(parent)
        {
            _newName = newName;
        }

        public IEnumerable<string> ToSql(IProvider provider, IMigrationContext context)
        {
            AlterTableCommand parentAlterTableCommand;
            AlterColumnCommand parentAlterColumnCommand;
            AlterPrimaryKeyCommand parentAlterPrimaryKeyCommand;
            if ((parentAlterTableCommand = Parent as AlterTableCommand) != null)
            {
                return provider.RenameTable(new TableName(parentAlterTableCommand.TableName, parentAlterTableCommand.Schema ?? context.GetDefaultSchema()), _newName);
            }
            else if ((parentAlterColumnCommand = Parent as AlterColumnCommand) != null)
            {
                return provider.RenameColumn(new TableName(parentAlterColumnCommand.Parent.TableName, parentAlterColumnCommand.Parent.Schema ?? context.GetDefaultSchema()), parentAlterColumnCommand.ColumnName, _newName);
            }
            else if ((parentAlterPrimaryKeyCommand = Parent as AlterPrimaryKeyCommand) != null)
            {
                return provider.RenamePrimaryKey(new TableName(parentAlterPrimaryKeyCommand.Parent.TableName, parentAlterPrimaryKeyCommand.Parent.Schema ?? context.GetDefaultSchema()), parentAlterPrimaryKeyCommand.ConstraintName, _newName);
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unknown parent command of a RenameCommand: {0}.", Parent.GetType()));
            }
        }
    }
}