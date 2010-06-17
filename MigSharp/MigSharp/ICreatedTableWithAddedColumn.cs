namespace MigSharp
{
    public interface ICreatedTableWithAddedColumn : ICreatedTableBase
    {
        ICreatedTableWithAddedColumn OfLength(int length);        
    }
}