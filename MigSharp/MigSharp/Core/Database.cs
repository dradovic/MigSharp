using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class Database : IDatabase
    {
        private readonly MigrateCommand _root = new MigrateCommand();
        private readonly TableCollection _tables;

        internal ICommand Root { get { return _root; } }
        public IExistingTableCollection Tables { get { return _tables; } }

        public Database()
        {
            _tables = new TableCollection(_root);
        }

        public INewTable CreateTable(string tableName)
        {
            var createTableCommand = new CreateTableCommand();
            _root.Add(createTableCommand);
            var table = new NewTable(tableName, createTableCommand);
            return table;
        }
    }
}