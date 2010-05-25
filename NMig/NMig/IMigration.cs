namespace NMig
{
    public interface IMigration
    {
        void Up(Database db);
    }
}