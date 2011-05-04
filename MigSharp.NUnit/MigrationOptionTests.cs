using System.Data;

using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    public class MigrationOptionTests
    {
        private const string ProviderName = "Test Provider";
        private const DbType Type = DbType.Int16;

        [Test]
        public void VerityIsWarningSuppressedIsFalseByDefault()
        {
            var options = new MigrationOptions();
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 0, 0));
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 1, 0));
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningAlways()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(ProviderName, Type, SuppressCondition.Always);
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 0, 0));
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 1, 0));
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningWithoutSize()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(ProviderName, Type, SuppressCondition.WhenSpecifiedWithoutSize);
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 0, 0));
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 1, 0));
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningWithSize()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(ProviderName, Type, SuppressCondition.WhenSpecifiedWithSize);
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 0, 0));
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 1, 0));
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningWithSizeAndScale()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(ProviderName, Type, SuppressCondition.WhenSpecifiedWithSizeAndScale);
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 0, 0));
            Assert.IsFalse(options.IsWarningSuppressed(ProviderName, Type, 1, 0));
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningCombined()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(ProviderName, Type, SuppressCondition.WhenSpecifiedWithoutSize);
            options.SuppressWarning(ProviderName, Type, SuppressCondition.WhenSpecifiedWithSize);
            options.SuppressWarning(ProviderName, Type, SuppressCondition.WhenSpecifiedWithSizeAndScale);
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 0, 0));
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 1, 0));
            Assert.IsTrue(options.IsWarningSuppressed(ProviderName, Type, 1, 1));
        }
    }
}