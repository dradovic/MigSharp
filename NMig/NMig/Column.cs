using NMig.Core.Commands;

namespace NMig
{
    public class Column
    {
        private readonly AlterColumnCommand _alterColumnCommand;

        internal Column(AlterColumnCommand alterColumnCommand)
        {
            _alterColumnCommand = alterColumnCommand;
        }

        public void Rename(string newName)
        {
            _alterColumnCommand.Add(new RenameCommand(newName));
        }
    }
}