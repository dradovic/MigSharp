using System.Data;

namespace MigSharp.Process
{
    internal class BootstrapMigration : IReversibleMigration
    {
        internal const string TimestampColumnName = "Timestamp";
        internal const string ModuleColumnName = "Module";
        internal const string TagColumnName = "Tag";

        private readonly string _tableName;

        public BootstrapMigration(string tableName)
        {
            _tableName = tableName;
        }

        public void Up(IDatabase db)
        {
            db.CreateTable(_tableName)
                .WithPrimaryKeyColumn(TimestampColumnName, DbType.Int64)
                .WithPrimaryKeyColumn(ModuleColumnName, DbType.String).OfSize(MigrationExportAttribute.MaximumModuleNameLength)
                .WithNullableColumn(TagColumnName, DbType.String).OfSize(2000);
        }

        public void Down(IDatabase db)
        {
            db.Tables[_tableName].Drop();
        }
    }
}