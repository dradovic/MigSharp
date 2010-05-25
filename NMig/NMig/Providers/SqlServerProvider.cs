namespace NMig.Providers
{
    internal class SqlServerProvider : IProvider
    {
        public string RenameTable(string oldName, string newName)
        {
            return string.Format("sp_rename N'{0}', N'{1}'", oldName, newName); // TODO: escape names
        }

        public string RenameColumn(string oldName, string newName)
        {
            return string.Format("sp_rename N'{0}', N'{1}', 'COLUMN'", oldName, newName); // TODO: escape names
        }
    }
}