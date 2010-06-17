namespace MigSharp
{
    public interface ICreatedTable : ICreatedTableBase
    {
        ICreatedTableBase IfNotExists();
    }
}