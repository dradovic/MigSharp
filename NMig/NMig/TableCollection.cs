namespace NMig
{
    internal class TableCollection : AdHocCollection<Table>, ITableCollection
    {
        internal TableCollection(IRecorder recorder) : base(recorder)
        {
        }

        protected override Table CreateItem(string name, IRecorder recorder)
        {
            return new Table(name, recorder);
        }
    }
}