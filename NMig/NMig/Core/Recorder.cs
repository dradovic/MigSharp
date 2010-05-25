using System.Collections.Generic;

namespace NMig.Core
{
    internal class Recorder : IRecorder
    {
        private readonly List<Command> _commands = new List<Command>();

        public void Record(Command command)
        {
            _commands.Add(command);
        }

        public IEnumerable<Command> GetCommands()
        {
            return _commands;
        }
    }
}