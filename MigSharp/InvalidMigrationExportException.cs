using System;
using System.Runtime.Serialization;

namespace MigSharp
{
    /// <summary>
    /// This exception is thrown when some of the exported migrations are invalid.
    /// </summary>
    [Serializable]
    public class InvalidMigrationExportException : Exception
    {
        public InvalidMigrationExportException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public InvalidMigrationExportException(string message) :
            base(message)
        {
        }

        public InvalidMigrationExportException()
        {
        }

        protected InvalidMigrationExportException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}