using NMig.Commands;
using NMig.Core;

namespace NMig
{
    public class Column : DbObject
    {
        private readonly IRecorder _recorder;

        internal Column(string name, IRecorder recorder) : base(name)
        {
            _recorder = recorder;
        }

        public void Rename(string newName)
        {
            _recorder.Record(new Rename(this, newName));
        }
    }
}