namespace MigSharp.Core.Commands
{
    internal class AlterIndexCommand : Command
    {
        private readonly string _indexName;

        public string IndexName { get { return _indexName; } }
        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }

        public AlterIndexCommand(AlterTableCommand parent, string indexName)
            : base(parent)
        {
            _indexName = indexName;
        }
    }
}