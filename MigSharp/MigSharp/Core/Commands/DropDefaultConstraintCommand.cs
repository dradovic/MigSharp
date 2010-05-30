using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class DropDefaultConstraintCommand : Command, IScriptableCommand
    {
        public DropDefaultConstraintCommand(AlterColumnCommand parent) : base(parent)
        {
        }

        public IEnumerable<string> Script(IProvider provider)
        {
            AlterColumnCommand alterColumnCommand = (AlterColumnCommand)Parent; // TODO: provider type-safe Parents
            AlterTableCommand alterTableCommand = (AlterTableCommand)alterColumnCommand.Parent; // TODO: provider type-safe Parents
            return provider.DropDefaultConstraint(alterTableCommand.TableName, alterColumnCommand.ColumnName);
        }
    }
}