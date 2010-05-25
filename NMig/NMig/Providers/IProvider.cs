namespace NMig.Providers
{
    public interface IProvider
    {
        string RenameTable(string oldName, string newName);
        string RenameColumn(string oldName, string newName);
    }
}