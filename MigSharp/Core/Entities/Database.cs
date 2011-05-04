using System;

using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class Database : IDatabase
    {
        private readonly IMigrationContext _context;
        private readonly MigrateCommand _root = new MigrateCommand();
        private readonly TableCollection _tables;

        internal ICommand Root { get { return _root; } }

        public IMigrationContext Context { get { return _context; } }
        public IExistingTableCollection Tables { get { return _tables; } }

        public Database(IMigrationContext context)
        {
            _context = context;
            _tables = new TableCollection(_root);
        }

        public ICreatedTable CreateTable(string tableName, string primaryKeyConstraintName)
        {
            var createTableCommand = new CreateTableCommand(_root, tableName, primaryKeyConstraintName);
            _root.Add(createTableCommand);
            return new CreatedTable(createTableCommand);
        }

        public void Execute(string query)
        {
            var command = new CustomQueryCommand(_root, query);
            _root.Add(command);
        }

        public void Execute(Action<IRuntimeContext> action)
        {
            var command = new CallCommand(_root, action);
            _root.Add(command);
        }
    }
}