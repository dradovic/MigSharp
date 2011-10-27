using System.Collections.Generic;
using System.Linq;

using MigSharp.Core;
using MigSharp.Process;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class MigrationSelectorTests
    {
        private const string DefaultModuleName = MigrationExportAttribute.DefaultModuleName;

        [Test]
        public void TestRegularCase()
        {
            var migration = MockRepository.GenerateStub<IMigration>();
            ImportedMigration[] importedMigrations = {
                new ImportedMigration(migration, new MigrationMetadata(1, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(2, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(3, DefaultModuleName, null)),
            };
            IMigrationMetadata[] executedMigrations = {
                new MigrationMetadata(1, DefaultModuleName, null),
                new MigrationMetadata(2, DefaultModuleName, null),
            };

            var selector = new MigrationSelector(importedMigrations, executedMigrations);

            IEnumerable<ApplicableMigration> applicableMigrations;
            IEnumerable<IMigrationMetadata> unidentifiedMigrations;
            selector.GetMigrationsTo(long.MaxValue, s => true, out applicableMigrations, out unidentifiedMigrations);

            Assert.AreEqual(1, applicableMigrations.Count());
            Assert.AreEqual(3, applicableMigrations.First().Metadata.Timestamp);
            Assert.AreEqual(MigrationDirection.Up, applicableMigrations.First().Metadata.Direction);
            CollectionAssert.IsEmpty(unidentifiedMigrations);
        }

        [Test]
        public void TestGapsAreFound()
        {
            var migration = MockRepository.GenerateStub<IMigration>();
            ImportedMigration[] importedMigrations = {
                new ImportedMigration(migration, new MigrationMetadata(1, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(2, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(3, DefaultModuleName, null)),
            };
            IMigrationMetadata[] executedMigrations = {
                new MigrationMetadata(1, DefaultModuleName, null),
                new MigrationMetadata(3, DefaultModuleName, null),
            };

            var selector = new MigrationSelector(importedMigrations, executedMigrations);

            IEnumerable<ApplicableMigration> applicableMigrations;
            IEnumerable<IMigrationMetadata> unidentifiedMigrations;
            selector.GetMigrationsTo(long.MaxValue, s => true, out applicableMigrations, out unidentifiedMigrations);

            Assert.AreEqual(1, applicableMigrations.Count());
            Assert.AreEqual(2, applicableMigrations.First().Metadata.Timestamp);
            Assert.AreEqual(MigrationDirection.Up, applicableMigrations.First().Metadata.Direction);
            CollectionAssert.IsEmpty(unidentifiedMigrations);
        }

        [Test]
        public void TestReverting()
        {
            var migration = MockRepository.GenerateStub<IReversibleMigration>();
            ImportedMigration[] importedMigrations = {
                new ImportedMigration(migration, new MigrationMetadata(1, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(2, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(3, DefaultModuleName, null)),
            };
            IMigrationMetadata[] executedMigrations = {
                new MigrationMetadata(1, DefaultModuleName, null),
                new MigrationMetadata(2, DefaultModuleName, null),
                new MigrationMetadata(3, DefaultModuleName, null),
            };

            var selector = new MigrationSelector(importedMigrations, executedMigrations);

            IEnumerable<ApplicableMigration> applicableMigrations;
            IEnumerable<IMigrationMetadata> unidentifiedMigrations;
            selector.GetMigrationsTo(2, s => true, out applicableMigrations, out unidentifiedMigrations);

            Assert.AreEqual(1, applicableMigrations.Count());
            Assert.AreEqual(3, applicableMigrations.First().Metadata.Timestamp);
            Assert.AreEqual(MigrationDirection.Down, applicableMigrations.First().Metadata.Direction);
            CollectionAssert.IsEmpty(unidentifiedMigrations);
        }

        [Test, ExpectedException(typeof(IrreversibleMigrationException))]
        public void TestRevertingThrowsWhenImpossible()
        {
            var migration = MockRepository.GenerateStub<IMigration>();
            ImportedMigration[] importedMigrations = {
                new ImportedMigration(migration, new MigrationMetadata(1, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(2, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(3, DefaultModuleName, null)),
            };
            IMigrationMetadata[] executedMigrations = {
                new MigrationMetadata(1, DefaultModuleName, null),
                new MigrationMetadata(2, DefaultModuleName, null),
                new MigrationMetadata(3, DefaultModuleName, null),
            };

            var selector = new MigrationSelector(importedMigrations, executedMigrations);

            IEnumerable<ApplicableMigration> applicableMigrations;
            IEnumerable<IMigrationMetadata> unidentifiedMigrations;
            selector.GetMigrationsTo(2, s => true, out applicableMigrations, out unidentifiedMigrations);
        }

        [Test]
        public void TestUnidentifiedMigrations()
        {
            var migration = MockRepository.GenerateStub<IReversibleMigration>();
            ImportedMigration[] importedMigrations = {
                new ImportedMigration(migration, new MigrationMetadata(1, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(2, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(3, DefaultModuleName, null)),
            };
            IMigrationMetadata[] executedMigrations = {
                new MigrationMetadata(1, DefaultModuleName, null),
                new MigrationMetadata(2, DefaultModuleName, null),
                new MigrationMetadata(13, DefaultModuleName, null),
            };

            var selector = new MigrationSelector(importedMigrations, executedMigrations);

            IEnumerable<ApplicableMigration> applicableMigrations;
            IEnumerable<IMigrationMetadata> unidentifiedMigrations;
            selector.GetMigrationsTo(long.MaxValue, s => true, out applicableMigrations, out unidentifiedMigrations);

            Assert.AreEqual(1, applicableMigrations.Count());
            Assert.AreEqual(3, applicableMigrations.First().Metadata.Timestamp);
            Assert.AreEqual(MigrationDirection.Up, applicableMigrations.First().Metadata.Direction);
            Assert.AreEqual(1, unidentifiedMigrations.Count());
            Assert.AreEqual(13, unidentifiedMigrations.First().Timestamp);
        }

        [Test, ExpectedException(typeof(InvalidMigrationExportException), ExpectedMessage = "The migration with timestamp 1 and module name '" + MigrationExportAttribute.DefaultModuleName + "' is defined more than once.")]
        public void TestDuplicateMigrationsThrowInvalidMigrationException()
        {
            var migration = MockRepository.GenerateStub<IMigration>();
            ImportedMigration[] importedMigrations = {
                new ImportedMigration(migration, new MigrationMetadata(1, DefaultModuleName, null)),
                new ImportedMigration(migration, new MigrationMetadata(1, DefaultModuleName, null)),
            };
            IMigrationMetadata[] executedMigrations = {
            };

            var selector = new MigrationSelector(importedMigrations, executedMigrations);
            Assert.IsNotNull(selector, "Just to satisfy R# and FxCop.");
        }
    }
}