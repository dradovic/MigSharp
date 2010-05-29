namespace MigSharp
{
    public interface IMigration
    {
        void Up(IDatabase db);
    }
}