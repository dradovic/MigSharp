using System;

using MigSharp;

namespace Sample
{
    [MigrationExport(2010, 6, 15, 21, 55, 12)]
    internal class Migration1 : IMigration
    {
        #region Implementation of IMigration

        public void Up(IDatabase db)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}