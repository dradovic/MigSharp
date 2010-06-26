using System;

using MigSharp.NUnit.Integration;
using MigSharp.Process;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("Smoke")]
    public class CustomVersioningTests
    {
        [Test]
        public void TestCustomVersioning()
        {
            IVersioning versioning = MockRepository.GenerateMock<IVersioning>();
            DateTime timestamp1 = typeof(Migration1).GetTimestamp();
            DateTime timestamp2 = typeof(Migration2).GetTimestamp();
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp() == timestamp1))).Return(true);
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp() == timestamp2))).Return(false);

            Migrator migrator = new Migrator("", "");            
            migrator.UseCustomVersioning(versioning);
            IMigrationBatch batch = migrator.FetchPendingMigrations(typeof(Migration1).Assembly);

            versioning.VerifyAllExpectations();
            Assert.AreEqual(1, batch.Count);
        }

        // TODO: test custom bootstrapping
    }
}