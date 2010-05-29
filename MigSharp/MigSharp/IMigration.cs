namespace MigSharp
{
    public interface IMigration
    {
        void Up(Database db);
    }
}