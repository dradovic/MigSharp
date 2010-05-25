namespace NMig
{
    internal class ColumnCollection : AdHocCollection<Column>, IColumnCollection
    {
        public ColumnCollection(IRecorder recorder) : base(recorder)
        {
        }

        protected override Column CreateItem(string name, IRecorder recorder)
        {
            return new Column(name, recorder);
        }
    }
}