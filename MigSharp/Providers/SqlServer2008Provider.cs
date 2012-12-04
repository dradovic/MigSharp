using System.Data;

namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.SqlServer2008, "System.Data.SqlClient", MaximumDbObjectNameLength = MaximumDbObjectNameLength)]
    [Supports(DbType.Date)]
    //[Supports(DbType.Time)] due to a ADO.NET bug (see: https://connect.microsoft.com/VisualStudio/feedback/details/381934/sqlparameter-dbtype-dbtype-time-sets-the-parameter-to-sqldbtype-datetime-instead-of-sqldbtype-time and http://stackoverflow.com/questions/771465/failed-to-convert-parameter-value-from-a-timespan-to-a-datetime)
    [Supports(DbType.DateTime2)]
    [Supports(DbType.DateTimeOffset)]
    internal class SqlServer2008Provider : SqlServer2005Provider
    {
    }
}