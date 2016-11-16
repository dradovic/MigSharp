namespace MigSharp
{
    internal static class RuntimeContextExtensions
    {
        public static string GetDefaultSchema(this IMigrationContext context)
        {
            if (context.StepMetadata != null && context.StepMetadata.UseModuleNameAsDefaultSchema)
            {
                return context.StepMetadata.ModuleName;
            }
            return null;
        }
    }
}