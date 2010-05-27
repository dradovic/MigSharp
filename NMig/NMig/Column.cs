using NMig.Core.Commands;

namespace NMig
{
    public class Column : DbObject
    {
        private readonly AlterColumnCommand _alterColumnCommand;

        internal Column(string name, AlterColumnCommand alterColumnCommand) : base(name)
        {
            _alterColumnCommand = alterColumnCommand;
        }

        public void Rename(string newName)
        {
            _alterColumnCommand.Add(new RenameCommand(newName));
        }
    }
}