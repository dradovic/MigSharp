using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MigSharp.Core;
using MigSharp.Process;

namespace MigSharp.Providers
{
    internal class RecordingProvider : IProvider, IRecordedMigration
    {
        private readonly List<UsedDataType> _dataTypes = new List<UsedDataType>();
        private readonly List<string> _newObjectNames = new List<string>();
        private readonly HashSet<string> _methods = new HashSet<string>();

        #region IRecordedMigration

        public IEnumerable<UsedDataType> DataTypes { get { return _dataTypes; } }

        public IEnumerable<string> NewObjectNames { get { return _newObjectNames; } }

        public IEnumerable<string> Methods { get { return _methods; } }

        #endregion

        #region IProvider

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ExistsTable(string databaseName, TableName tableName)
        {
            // ExistsTable is a special case: it must be supported by all providers as it is used to create
            // the versioning table. Also, it is only a querying method.
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ConvertToSql(object value, DbType targetDbType)
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> CreateTable(TableName tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            AddMethodName();
            AddNewObjectNames(tableName.Name, primaryKeyConstraintName);
            AddNewObjectNames(columns.Select(c => c.Name));
            AddNewObjectNames(columns.Select(c => c.UniqueConstraint).Where(name => !string.IsNullOrEmpty(name)));
            foreach (CreatedColumn column in columns)
            {
                _dataTypes.Add(new UsedDataType(column.DataType, column.IsPrimaryKey, column.IsIdentity));
            }
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> DropTable(TableName tableName, bool checkIfExists)
        {
            if (checkIfExists)
            {
                AddMethodName("DropTableIfExists");
            }
            else
            {
                AddMethodName();
            }
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> AddColumn(TableName tableName, Column column)
        {
            AddMethodName();
            AddNewObjectNames(column.Name);
            _dataTypes.Add(new UsedDataType(column.DataType, false, false));
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> RenameTable(TableName oldName, string newName)
        {
            AddMethodName();
            AddNewObjectNames(newName);
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> RenameColumn(TableName tableName, string oldName, string newName)
        {
            AddMethodName();
            AddNewObjectNames(newName);
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> DropColumn(TableName tableName, string columnName)
        {
            AddMethodName();
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> AlterColumn(TableName tableName, Column column)
        {
            AddMethodName();
            _dataTypes.Add(new UsedDataType(column.DataType, false, false));
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> AddIndex(TableName tableName, IEnumerable<string> columnNames, string indexName)
        {
            AddMethodName();
            AddNewObjectNames(indexName);
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> DropIndex(TableName tableName, string indexName)
        {
            AddMethodName();
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> AddForeignKey(TableName tableName, TableName referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName, bool cascadeOnDelete)
        {
            AddMethodName();
            AddNewObjectNames(constraintName);
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> DropForeignKey(TableName tableName, string constraintName)
        {
            AddMethodName();
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> AddPrimaryKey(TableName tableName, IEnumerable<string> columnNames, string constraintName)
        {
            AddMethodName();
            AddNewObjectNames(constraintName);
            // FEATURE: try to find out the data types of the pk columns add them to the _primaryKeyDataTypes list
            // This can only be done if the same migration has added the table, and if the columnNames are tracked, along with their data type.
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> RenamePrimaryKey(TableName tableName, string oldName, string newName)
        {
            AddMethodName();
            AddNewObjectNames(newName);
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> DropPrimaryKey(TableName tableName, string constraintName)
        {
            AddMethodName();
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> AddUniqueConstraint(TableName tableName, IEnumerable<string> columnNames, string constraintName)
        {
            AddMethodName();
            AddNewObjectNames(constraintName);
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> DropUniqueConstraint(TableName tableName, string constraintName)
        {
            AddMethodName();
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> DropDefault(TableName tableName, Column column)
        {
            AddMethodName();
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> CreateSchema(string schemaName)
        {
            AddMethodName();
            return Enumerable.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<string> DropSchema(string schemaName)
        {
            AddMethodName();
            return Enumerable.Empty<string>();
        }

        #endregion

        private void AddNewObjectNames(params string[] names)
        {
            AddNewObjectNames((IEnumerable<string>)names);
        }

        private void AddNewObjectNames(IEnumerable<string> names)
        {
            _newObjectNames.AddRange(names);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddMethodName()
        {
            var stackTrace = new StackTrace();
            StackFrame callerFrame = stackTrace.GetFrame(1);
            MethodBase callingMethod = callerFrame.GetMethod();
            if (callingMethod.DeclaringType != typeof(RecordingProvider))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unexpected calling method '{0}.{1}'.", callingMethod.DeclaringType, callingMethod.Name));
            }
            string methodName = callingMethod.Name;
            Debug.Assert(typeof(IProvider).GetMethod(methodName) != null, string.Format("IProvider has not '{0}' method.", methodName));
            AddMethodName(methodName);
        }

        private void AddMethodName(string methodName)
        {
            if (!_methods.Contains(methodName))
            {
                _methods.Add(methodName);
            }
        }
    }
}