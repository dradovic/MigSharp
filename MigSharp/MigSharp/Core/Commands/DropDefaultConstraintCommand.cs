using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class DropDefaultConstraintCommand : Command, IScriptableCommand
    {
        private new AlterColumnCommand Parent { get { return (AlterColumnCommand)base.Parent; } }

        public DropDefaultConstraintCommand(AlterColumnCommand parent) : base(parent)
        {
        }

        public IEnumerable<string> Script(IProvider provider)
        {
            return provider.DropDefaultConstraint(Parent.Parent.TableName, Parent.ColumnName);
        }
    }
}