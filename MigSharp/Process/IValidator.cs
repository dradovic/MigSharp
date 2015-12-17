using System.Collections.Generic;

namespace MigSharp.Process
{
    internal interface IValidator
    {
        void Validate(IEnumerable<IMigrationReporter> reporters, out string errors, out string warnings);
    }
}