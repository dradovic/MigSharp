namespace MigSharp.Core.Commands
{
    internal class RenameCommand : Command
    {
        private readonly string _newName;

        public string NewName { get { return _newName; } }

        public RenameCommand(string newName)
        {
            _newName = newName;
        }
    }
}