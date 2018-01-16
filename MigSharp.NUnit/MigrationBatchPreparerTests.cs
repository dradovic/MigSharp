using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FakeItEasy;
using MigSharp.Core;
using MigSharp.Process;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    internal class MigrationBatchPreparerTests
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [TestCaseSource("GetCases")]
        public void TestCases(IEnumerable<IMigrationMetadata> importedMigrations, IEnumerable<IAggregateMigrationMetadata> importedAggregateMigrations, IEnumerable<IMigrationMetadata> executedMigrations, IMigrationStepMetadata[] expectedSteps)
        {
            var importer = A.Fake<IMigrationImporter>();
            var versioning = A.Fake<IVersioning>();
            IReadOnlyCollection<ImportedMigration> ignoredMigrations;
            IReadOnlyCollection<ImportedAggregateMigration> ignoredAggregateMigrations;
            A.CallTo(() => importer.ImportAll(out ignoredMigrations, out ignoredAggregateMigrations)).AssignsOutAndRefParameters(
                importedMigrations.Select(m => new ImportedMigration(A.Fake<IMigration>(), m, false)).ToList(),
                importedAggregateMigrations.Select(m => new ImportedAggregateMigration(A.Fake<IMigration>(), m)).ToList());
            A.CallTo(() => versioning.ExecutedMigrations).Returns(executedMigrations);
            MigrationBatchPreparer preparer = new MigrationBatchPreparer(importer, versioning, A.Fake<IRuntimeConfiguration>());

            var batch = preparer.Prepare(long.MaxValue, new MigrationOptions());

            Assert.AreEqual(expectedSteps.Length, batch.Steps.Count, "Unexpected count of steps.");
            for (int i = 0; i < batch.Steps.Count; i++)
            {
                CollectionAssert.AreEqual(expectedSteps[i].Migrations.Select(GetMigrationDescription).ToArray(), batch.Steps[i].Migrations.Select(GetMigrationDescription).ToArray(), "Unexpected migrations in step {0}.", i);
            }
        }

        private static string GetMigrationDescription(IMigrationMetadata m)
        {
            string moduleName = (string.IsNullOrEmpty(m.ModuleName) ? MigrationExportAttribute.DefaultModuleName : m.ModuleName);
            return moduleName + ": " + m.Timestamp;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static IEnumerable<ITestCaseData> GetCases()
        {
            var m1 = A.Fake<IMigrationMetadata>();
            var m2 = A.Fake<IMigrationMetadata>();
            var m3 = A.Fake<IMigrationMetadata>();
            var x1 = A.Fake<IMigrationMetadata>();
            var a1 = A.Fake<IAggregateMigrationMetadata>();
            var a2 = A.Fake<IAggregateMigrationMetadata>();
            var a3 = A.Fake<IAggregateMigrationMetadata>();
            var step1 = A.Fake<IMigrationStepMetadata>();
            var step2 = A.Fake<IMigrationStepMetadata>();
            var step3 = A.Fake<IMigrationStepMetadata>();
            var stepAll = A.Fake<IMigrationStepMetadata>();
            var stepTo2 = A.Fake<IMigrationStepMetadata>();
            var stepX1 = A.Fake<IMigrationStepMetadata>();
            A.CallTo(() => m1.Timestamp).Returns(1);
            A.CallTo(() => m2.Timestamp).Returns(2);
            A.CallTo(() => m3.Timestamp).Returns(3);
            A.CallTo(() => x1.Timestamp).Returns(1);
            A.CallTo(() => x1.ModuleName).Returns("X");
            A.CallTo(() => a1.Timestamp).Returns(1);
            A.CallTo(() => a2.Timestamp).Returns(2);
            A.CallTo(() => a3.Timestamp).Returns(3);
            A.CallTo(() => step1.Migrations).Returns(new[] { m1 });
            A.CallTo(() => step2.Migrations).Returns(new[] { m2 });
            A.CallTo(() => step3.Migrations).Returns(new[] { m3 });
            A.CallTo(() => stepAll.Migrations).Returns(new[] { m1, m2, m3 });
            A.CallTo(() => stepTo2.Migrations).Returns(new[] { m1, m2 });
            A.CallTo(() => stepX1.Migrations).Returns(new[] { x1 });
            yield return new Case(Enumerable.Empty<IMigrationMetadata>(), Enumerable.Empty<IAggregateMigrationMetadata>(), Enumerable.Empty<IMigrationMetadata>(), Enumerable.Empty<IMigrationStepMetadata>()).SetName("Empty");
            yield return new Case(new[] { m1, m2, m3 }, Enumerable.Empty<IAggregateMigrationMetadata>(), Enumerable.Empty<IMigrationMetadata>(), new[] { step1, step2, step3 }).SetName("Executed none");
            yield return new Case(new[] { m1, m2, m3 }, Enumerable.Empty<IAggregateMigrationMetadata>(), new[] { m1, m2 }, new[] { step3 }).SetName("Latest missing");
            yield return new Case(new[] { m1, m2, m3 }, Enumerable.Empty<IAggregateMigrationMetadata>(), new[] { m1, m3 }, new[] { step2 }).SetName("Historic missing");
            yield return new Case(new[] { m1, m2, m3 }, Enumerable.Empty<IAggregateMigrationMetadata>(), new[] { m1, m2, m3 }, Enumerable.Empty<IMigrationStepMetadata>()).SetName("All up-to-date");
            yield return new Case(new[] { m1, m2, m3 }, new[] { a3 }, Enumerable.Empty<IMigrationMetadata>(), new[] { stepAll }).SetName("Aggregate all");
            yield return new Case(new[] { m1, m2, m3 }, new[] { a2 }, Enumerable.Empty<IMigrationMetadata>(), new[] { stepTo2, step3 }).SetName("Aggregate intermediate migrations");
            yield return new Case(new[] { m1, m2, m3 }, new[] { a2, a3 }, Enumerable.Empty<IMigrationMetadata>(), new[] { stepAll }).SetName("Aggregate maximally");
            yield return new Case(new[] { m1, m2, m3, x1 }, Enumerable.Empty<IAggregateMigrationMetadata>(), Enumerable.Empty<IMigrationMetadata>(), new[] { step1, stepX1, step2, step3 }).SetName("Migration from another module");
            yield return new Case(new[] { m1, m2, m3, x1 }, new[] { a2 }, Enumerable.Empty<IMigrationMetadata>(), new[] { stepX1, stepTo2, step3 }).SetName("Aggregate intermediate migrations excluding migration from another module");
        }

        private class Case : TestCaseData
        {
            public Case(IEnumerable<IMigrationMetadata> importedMigrations, IEnumerable<IAggregateMigrationMetadata> importedAggregateMigrations, IEnumerable<IMigrationMetadata> executedMigrations, IEnumerable<IMigrationStepMetadata> expectedSteps)
                : base(importedMigrations, importedAggregateMigrations, executedMigrations, expectedSteps)
            {
            }
        }
    }
}