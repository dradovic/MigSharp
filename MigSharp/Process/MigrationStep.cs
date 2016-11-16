using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using MigSharp.Core;
using MigSharp.Core.Entities;

namespace MigSharp.Process
{
    internal class MigrationStep : BootstrapMigrationStep, IMigrationStep
    {
        private string MigrationName { get { return Migration.GetName(); } }
        IMigrationStepMetadata IMigrationStep.Metadata { get { return Metadata; } }
        IMigrationStepMetadata IMigrationReporter.StepMetadata { get { return Metadata; } }

        public MigrationStep(IMigration migration, IMigrationStepMetadata metadata)
            : base(migration, metadata)
        {
        }

        public IMigrationReport Report(IMigrationContext context)
        {
            Database database = GetDatabaseContainingMigrationChanges(Metadata.Direction, context);
            return MigrationReport.Create(database, MigrationName, context);
        }

        public void Execute(IRuntimeConfiguration configuration, IVersioning versioning)
        {
            if (versioning == null) throw new ArgumentNullException("versioning");

            DateTime start = DateTime.Now;

            long timestamp = GetTimestamp();
            string tag = GetTag();
            using (IDbConnection connection = configuration.OpenConnection())
            {
                Debug.Assert(connection.State == ConnectionState.Open);

                using (IDbTransaction transaction = configuration.ConnectionInfo.SupportsTransactions ? connection.BeginTransaction() : null)
                {
                    IDbCommandExecutor executor;
                    using ((executor = configuration.SqlDispatcher.CreateExecutor(string.Format(CultureInfo.InvariantCulture, "Migration.{0}.{1}", Metadata.ModuleName, timestamp))) as IDisposable)
                    {
                        try
                        {
                            Execute(configuration.ProviderInfo, connection, transaction, Metadata.Direction, executor);
                        }
                        catch
                        {
                            Log.Error("An non-recoverable error occurred in migration '{0}'{1}{2} while executing {3}.",
                                timestamp,
                                Metadata.ModuleName != MigrationExportAttribute.DefaultModuleName ? " in module '" + Metadata.ModuleName + "'" : string.Empty,
                                tag,
                                Metadata.Direction);
                            throw;
                        }

                        // update versioning
                        versioning.Update(Metadata, connection, transaction, executor);
                    }

                    if (transaction != null)
                    {
                        transaction.Commit();
                    }
                }
            }

            Log.Verbose(LogCategory.Performance, "Migration of module '{0}' to {1}{2} took {3}s",
                Metadata.ModuleName,
                timestamp,
                tag,
                (DateTime.Now - start).TotalSeconds);
        }

        private long GetTimestamp()
        {
            return Metadata.Migrations.Max(m => m.Timestamp);
        }

        private string GetTag()
        {
            string tag;
            if (Metadata.Migrations.Count() > 1)
            {
                tag = " (aggregate migration)";
            }
            else
            {
                tag = Metadata.Migrations.Single().Tag;
                if (!string.IsNullOrEmpty(tag))
                {
                    tag = ": '" + tag + "'";
                }
            }
            return tag;
        }
    }
}