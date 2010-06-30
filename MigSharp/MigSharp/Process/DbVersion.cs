using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;

using MigSharp.Core;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class DbVersion : IVersioning
    {
        private readonly string _tableName;
        private readonly DbVersionDataSet _dataSet;
        private readonly DbProviderFactory _factory;

        public bool IsEmpty { get { return _dataSet.DbVersion.Rows.Count == 0; } }

        private DbVersion(string tableName, DbVersionDataSet dataSet, DbProviderFactory factory)
        {
            Debug.Assert(dataSet.DbVersion.ModuleColumn.MaxLength == MigrationExportAttribute.MaximumModuleNameLength);

            _tableName = tableName;
            _dataSet = dataSet;
            _factory = factory;
        }

        public static DbVersion Create(ConnectionInfo connectionInfo, IProviderFactory providerFactory, IDbConnectionFactory connectionFactory)
        {
            string tableName = Options.VersioningTableName;

            // execute boostrap migration step to ensure that the dbVersion table exists
            var step = new MigrationStep(new BootstrapMigration(tableName), new BootstrapMetadata(), connectionInfo, providerFactory, connectionFactory);
            step.Execute(null, MigrationDirection.Up);

            // create and fill DataSet
            var dataSet = new DbVersionDataSet();
            DbProviderFactory factory = connectionFactory.GetDbProviderFactory(connectionInfo);
            using (IDbConnection connection = connectionFactory.OpenConnection(connectionInfo))
            {
                DbDataAdapter adapter = CreateAdapter(tableName, factory, connection, dataSet);
                adapter.Fill(dataSet.DbVersion); 
            }
            var dbVersion = new DbVersion(tableName, dataSet, factory);
            return dbVersion;
        }

        private static DbDataAdapter CreateAdapter(string tableName, DbProviderFactory factory, IDbConnection connection, DbVersionDataSet dataSet)
        {
            DbCommand selectCommand = factory.CreateCommand();
            selectCommand.Connection = (DbConnection)connection;
            selectCommand.CommandText = string.Format(CultureInfo.InvariantCulture, @"SELECT {0}, {1}, {2} FROM ""{3}""", // according to http://stackoverflow.com/questions/1544095/why-does-sql-server-management-studio-generate-code-using-square-brackets, double quotes seem to be the ANSI standard way of quoting tables
                dataSet.DbVersion.TimestampColumn.ColumnName,
                dataSet.DbVersion.ModuleColumn.ColumnName,
                dataSet.DbVersion.TagColumn.ColumnName,
                tableName);
            DbDataAdapter adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = selectCommand;
            return adapter;
        }

        /// <summary>
        /// Used for unit testing.
        /// </summary>
        internal static DbVersion Create(DbVersionDataSet dataSet)
        {
            return new DbVersion(Options.VersioningTableName, dataSet, null);
        }

        public bool IsContained(IMigrationMetadata metadata)
        {
            return _dataSet.DbVersion.FindByTimestampModule(metadata.Timestamp(), metadata.ModuleName) != null;
        }

        public void Update(IMigrationMetadata metadata, IDbConnection connection, IDbTransaction transaction, MigrationDirection direction)
        {
            Debug.Assert(!(metadata is BootstrapMetadata));

            if (direction == MigrationDirection.Up)
            {
                AddMigration(metadata);
            }
            else
            {
                Debug.Assert(direction == MigrationDirection.Down);
                DbVersionDataSet.DbVersionRow row = _dataSet.DbVersion.FindByTimestampModule(metadata.Timestamp(), metadata.ModuleName);
                Debug.Assert(row != null, "Only migrations that were applied previous are being undone.");
                row.Delete();
            }

            StoreChanges(connection, transaction);
        }

        private void AddMigration(IMigrationMetadata metadata)
        {
            _dataSet.DbVersion.AddDbVersionRow(metadata.Timestamp(), metadata.ModuleName, metadata.Tag);
        }

        private void StoreChanges(IDbConnection connection, IDbTransaction transaction)
        {
            DateTime start = DateTime.Now;
            DbDataAdapter adapter = CreateAdapter(_tableName, _factory, connection, _dataSet);
            adapter.SelectCommand.Transaction = (DbTransaction)transaction;
            DbCommandBuilder builder = _factory.CreateCommandBuilder();
            builder.DataAdapter = adapter;
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.DeleteCommand = builder.GetDeleteCommand();
            Log.Info(LogCategory.Performance, "Adapter creation took {0}ms", (DateTime.Now - start).TotalMilliseconds);

            adapter.Update(_dataSet.DbVersion); // write new row to database
            Log.Info(LogCategory.Performance, "Version update took {0}ms", (DateTime.Now - start).TotalMilliseconds);
        }

        internal void UpdateToInclude(IEnumerable<IMigrationMetadata> containedMigrations, ConnectionInfo connectionInfo, IDbConnectionFactory connectionFactory)
        {
            foreach (IMigrationMetadata metadata in containedMigrations)
            {
                AddMigration(metadata);
            }
            using (IDbConnection connection = connectionFactory.OpenConnection(connectionInfo))
            {
                connection.Open();
                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    StoreChanges(connection, transaction);
                    transaction.Commit();
                }
            }
        }

        private class BootstrapMigration : IReversibleMigration
        {
            private readonly string _tableName;

            public BootstrapMigration(string tableName)
            {
                _tableName = tableName;
            }

            public void Up(IDatabase db)
            {
                db.CreateTable(_tableName).IfNotExists()
                    .WithPrimaryKeyColumn("Timestamp", DbType.DateTime)
                    .WithPrimaryKeyColumn("Module", DbType.StringFixedLength).OfLength(MigrationExportAttribute.MaximumModuleNameLength)
                    .WithNullableColumn("Tag", DbType.String);
            }

            public void Down(IDatabase db)
            {
                db.Tables[_tableName].Drop();
            }
        }

        private class BootstrapMetadata : IMigrationMetadata
        {
            public int Year { get { throw new NotSupportedException(); } }
            public int Month { get { throw new NotSupportedException(); } }
            public int Day { get { throw new NotSupportedException(); } }
            public int Hour { get { throw new NotSupportedException(); } }
            public int Minute { get { throw new NotSupportedException(); } }
            public int Second { get { throw new NotSupportedException(); } }
            public string Tag { get { throw new NotSupportedException(); } }
            public string ModuleName { get { throw new NotSupportedException(); } }
        }
    }
}