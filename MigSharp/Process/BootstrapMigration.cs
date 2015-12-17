using System.Data;
using MigSharp.Core;

namespace MigSharp.Process
{
    internal class BootstrapMigration : IMigration
    {
        internal const string TimestampColumnName = "Timestamp";
        internal const string ModuleColumnName = "Module";
        internal const string TagColumnName = "Tag";

        private readonly TableName _versioningTable;

        public BootstrapMigration(TableName versioningTable)
        {
            _versioningTable = versioningTable;
        }

        public void Up(IDatabase db)
        {
            ICreatedTable table;
            if (_versioningTable.Schema != null)
            {
                table = db.Schemata[_versioningTable.Schema].CreateTable(_versioningTable.Name);
            }
            else
            {
                table = db.CreateTable(_versioningTable.Name);
            }
            table
                .WithPrimaryKeyColumn(TimestampColumnName, DbType.Int64)
                .WithPrimaryKeyColumn(ModuleColumnName, DbType.String).OfSize(MigrationExportAttribute.MaximumModuleNameLength)
                .WithNullableColumn(TagColumnName, DbType.String).OfSize(2000);
        }
    }
}