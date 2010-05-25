using NMig.Commands;
using NMig.Core;

namespace NMig
{
    public class Table : DbObject
    {
        private readonly IRecorder _recorder;
        private readonly ColumnCollection _columns;

        internal Table(string name, IRecorder recorder) : base(name)
        {
            _recorder = recorder;
            _columns = new ColumnCollection(_recorder);
        }

        public IColumnCollection Columns { get { return _columns; } }

        public void Rename(string newName)
        {
            _recorder.Record(new Rename(this, newName));
        }
    }
}