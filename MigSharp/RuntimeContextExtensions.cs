namespace MigSharp
{
    internal static class RuntimeContextExtensions
    {
        public static string GetDefaultSchema(this IMigrationContext context)
        {
            if (context.MigrationMetadata != null && context.MigrationMetadata.UseModuleNameAsDefaultSchema)
            {
                return context.MigrationMetadata.ModuleName;
            }
            return null;
        }
    }
}