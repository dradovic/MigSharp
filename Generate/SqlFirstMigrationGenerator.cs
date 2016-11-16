using System.Globalization;
using Microsoft.SqlServer.Management.Smo;

namespace MigSharp.Generate
{
    internal class SqlFirstMigrationGenerator : SqlMigrationGeneratorBase
    {
        protected override string ClassName { get { return "Migration1"; } }
        protected override string ExportAttribute { get { return "MigrationExport"; } }

        public SqlFirstMigrationGenerator(Server server, Database database, GeneratorOptions options) 
            : base(server, database, options)
        {
        }
    }
}