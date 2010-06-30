using System;
using System.Runtime.Serialization;

namespace MigSharp
{
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