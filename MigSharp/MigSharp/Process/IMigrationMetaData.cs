namespace MigSharp.Process
{
    public interface IMigrationMetadata
    {
        int Year { get; }
        int Month { get; }
        int Day { get; }
        int Hour { get; }
        int Minute { get; }
        int Second { get; }
        string Tag { get; }
        string ModuleName { get; }
    }
}