namespace MigSharp
{
    public interface IExistingColumn
    {
        void Rename(string newName);
        void DropDefaultConstraint();
    }
}