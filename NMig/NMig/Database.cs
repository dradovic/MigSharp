using System.Collections.Generic;

using NMig.Core;

namespace NMig
{
    public class Database
    {
        private readonly Recorder _recorder = new Recorder();
        private readonly TableCollection _tables;

        public ITableCollection Tables { get { return _tables; } }

        public Database()
        {
            _tables = new TableCollection(_recorder);
        }

        internal IEnumerable<Command> GetCommands()
        {
            return _recorder.GetCommands();
        }
    }
}