namespace MigSharp.Process
{
    public interface IMigrationMetaData
    {
        int Year { get; }
        int Month { get; }
        int Day { get; }
        int Hour { get; }
        int Minute { get; }
        int Second { get; }
    }
}