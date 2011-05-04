using System.Globalization;

namespace MigSharp
{
    internal static class DefaultObjectNameProvider
    {
        public static string GetPrimaryKeyConstraintName(string tableName, string proposedConstraintName)
        {
            return string.IsNullOrEmpty(proposedConstraintName) ? "PK_" + tableName : proposedConstraintName;
        }

        public static string GetIndexName(string tableName, string firstColumnName, string proposedIndexName)
        {
            // we are using only the first columnName such that the constraint name does not grow too long
            return string.IsNullOrEmpty(proposedIndexName) ? string.Format(CultureInfo.InvariantCulture, "IX_{0}_{1}", tableName, firstColumnName) : proposedIndexName;
        }

        public static string GetForeignKeyConstraintName(string tableName, string referencedTableName, string proposedConstraintName)
        {
            return string.IsNullOrEmpty(proposedConstraintName) ? string.Format(CultureInfo.InvariantCulture, "FK_{0}_{1}", tableName, referencedTableName) : proposedConstraintName;
        }

        public static string GetUniqueConstraintName(string tableName, string firstColumnName, string proposedConstraintName)
        {
            // we are using only the first columnName such that the constraint name does not grow too long
            return string.IsNullOrEmpty(proposedConstraintName) ? string.Format(CultureInfo.InvariantCulture, "IX_{0}_{1}", tableName, firstColumnName) : proposedConstraintName;
        }
    }
}