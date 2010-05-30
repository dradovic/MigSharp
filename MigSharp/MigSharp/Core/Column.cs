using System;

using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class Column : IExistingColumn
    {
        private readonly AlterColumnCommand _alterColumnCommand;

        internal Column(AlterColumnCommand alterColumnCommand)
        {
            _alterColumnCommand = alterColumnCommand;
        }

        public void Rename(string newName)
        {
            _alterColumnCommand.Add(new RenameCommand(_alterColumnCommand, newName));
        }

        public void DropDefaultConstraint()
        {
            _alterColumnCommand.Add(new DropDefaultConstraintCommand(_alterColumnCommand));
        }
    }
}