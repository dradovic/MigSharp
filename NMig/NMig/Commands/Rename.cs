using NMig.Core;

namespace NMig.Commands
{
    internal class Rename : Command
    {
        private readonly string _newName;

        public string NewName { get { return _newName; } }

        public Rename(DbObject target, string newName)
            : base(target)
        {
            _newName = newName;
        }
    }
}