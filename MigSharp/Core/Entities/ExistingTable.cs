using System;
using System.Data;
using System.Linq;

using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingTable : IExistingTable, IExistingTableWithAddedColumn
    {
        private readonly AlterTableCommand _command;
        private readonly ColumnCollection _columns;
        private readonly IndexesCollection _indexes;
        private readonly UniqueConstraintCollection _uniqueConstraints;
        private readonly ForeignKeyCollection _foreignKeyConstraints;

        string IExistingTableBase.TableName { get { return _command.TableName; } }

        public IExistingColumnCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    // this should never be the case as the fluent interface should not expose the Columns property
                    // for new tables
                    throw new InvalidOperationException("Cannot access existing Columns of a newly created table.");
                }
                return _columns;
            }
        }

        public IExistingPrimaryKey PrimaryKey(string constraintName)
        {
            if (string.IsNullOrEmpty(constraintName)) throw new ArgumentException("Empty constraintName.");

            var command = new AlterPrimaryKeyCommand(_command, constraintName);
            _command.Add(command);
            return new ExistingPrimaryKey(command);
        }

        public IIndexesCollection Indexes
        {
            get
            {
                if (_indexes == null)
                {
                    // this should never be the case as the fluent interface should not expose the Columns property
                    // for new tables
                    throw new InvalidOperationException("Cannot access existing Indexes of a newly created table.");
                }
                return _indexes;
            }
        }

        public IUniqueConstraintCollection UniqueConstraints
        {
            get
            {
                if (_uniqueConstraints == null)
                {
                    // this should never be the case as the fluent interface should not expose the Columns property
                    // for new tables
                    throw new InvalidOperationException("Cannot access existing UniqueConstraints of a newly created table.");
                }
                return _uniqueConstraints;
            }
        }

        public IForeignKeyCollection ForeignKeys
        {
            get
            {
                if (_foreignKeyConstraints == null)
                {
                    // this should never be the case as the fluent interface should not expose the Columns property
                    // for new tables
                    throw new InvalidOperationException("Cannot access existing ForeignKeys of a newly created table.");
                }
                return _foreignKeyConstraints;
            }
        }

        internal ExistingTable(AlterTableCommand command)
        {
            _command = command;
            _columns = new ColumnCollection(command);
            _indexes = new IndexesCollection(command);
            _uniqueConstraints = new UniqueConstraintCollection(command);
            _foreignKeyConstraints = new ForeignKeyCollection(command);
        }

        void IExistingTable.Rename(string newName)
        {
            _command.Add(new RenameCommand(_command, newName));
        }

        void IExistingTable.Drop()
        {
            _command.Add(new DropCommand(_command));
        }

        public IAddedPrimaryKey AddPrimaryKey(string constraintName)
        {
            var command = new AddPrimaryKeyCommand(_command, constraintName);
            _command.Add(command);
            return new AddedPrimaryKey(command);
        }

        public IAddedIndex AddIndex(string indexName)
        {
            var command = new AddIndexCommand(_command, indexName);
            _command.Add(command);
            return new AddedIndex(command);
        }

        IAddedForeignKey IExistingTable.AddForeignKeyTo(string referencedTableName, string constraintName)
        {
            var command = new AddForeignKeyToCommand(_command, referencedTableName, constraintName);
            _command.Add(command);
            return new AddedForeignKey(command);
        }

        IAddedUniqueConstraint IExistingTable.AddUniqueConstraint(string constraintName)
        {
            var command = new AddUniqueConstraintCommand(_command, constraintName);
            _command.Add(command);
            return new AddedUniqueConstraint(command);
        }

        IExistingTableWithAddedColumn IExistingTableBase.AddNotNullableColumn(string name, DbType type)
        {
            _command.Add(new AddColumnCommand(_command, name, type, false));
            return this;
        }

        IExistingTableWithAddedColumn IExistingTableBase.AddNullableColumn(string name, DbType type)
        {
            _command.Add(new AddColumnCommand(_command, name, type, true));
            return this;
        }

        IExistingTableWithAddedColumn IExistingTableWithAddedColumn.OfSize(int size, int? scale)
        {
            var command = (AddColumnCommand)_command.Children.Last();
            command.Size = size;
            command.Scale = scale;
            return this;
        }

        IExistingTableBase IExistingTableWithAddedColumn.HavingDefault<T>(T value)
        {
            return HavingDefault(value, false);
        }

        IExistingTableBase IExistingTableWithAddedColumn.HavingDefault(string value)
        {
            return HavingDefault(value, false);
        }

        IExistingTableBase IExistingTableWithAddedColumn.HavingTemporaryDefault<T>(T value)
        {
            return HavingDefault(value, true);
        }

        IExistingTableBase IExistingTableWithAddedColumn.HavingTemporaryDefault(string value)
        {
            return HavingDefault(value, true);
        }

        private IExistingTableBase HavingDefault(object value, bool dropThereafter)
        {
            var command = (AddColumnCommand)_command.Children.Last();
            command.DefaultValue = value;
            command.DropThereafter = dropThereafter;
            return this;
        }
    }
}