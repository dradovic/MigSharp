using System;
using System.Data.Common;

namespace MigSharp.NUnit.Integration
{
    internal static class ExceptionExtensions
    {
        public static bool IsDbException(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");

            // unfortunately the SqlCeException (used by SQL Server CE 3.5) does not derive from DbException
            return exception is DbException || exception.GetType().Name == "SqlCeException";
        }
    }
}