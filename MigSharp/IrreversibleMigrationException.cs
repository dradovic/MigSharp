using System;
using System.Runtime.Serialization;

namespace MigSharp
{
    /// <summary>
    /// This exception is thrown when a requested downgrade path contains an irreversible migration.
    /// </summary>
    [Serializable]
    public class IrreversibleMigrationException : Exception
    {
        public IrreversibleMigrationException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public IrreversibleMigrationException(string message) :
            base(message)
        {
        }

        public IrreversibleMigrationException()
        {
        }

        protected IrreversibleMigrationException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}