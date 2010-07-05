using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class Database : IDatabase
    {
        private readonly IMigrationContext _context;
        private readonly MigrateCommand _root = new MigrateCommand();
        private readonly TableCollection _tables;

        internal ICommand Root { get { return _root; } }
        public IExistingTableCollection Tables { get { return _tables; } }

        public Database(IMigrationContext context)
        {
            _context = context;
            _tables = new TableCollection(_root);
        }

        public ICreatedTable CreateTable(string tableName)
        {
            var createTableCommand = new CreateTableCommand(_root, tableName);
            _root.Add(createTableCommand);
            return new CreatedTable(createTableCommand);
        }

        public ICustomQuery Execute(string query)
        {
            var customQueryCommand = new CustomQueryCommand(_root, query);
            _root.Add(customQueryCommand);
            return new CustomQuery(customQueryCommand);
        }

        public IMigrationContext Context { get { return _context; } }
    }
}