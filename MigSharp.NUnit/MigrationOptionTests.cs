using System.Data;
using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    public class MigrationOptionTests
    {
        private static readonly DbPlatform Platform = DbPlatform.SqlServer2012;
        private const DbType Type = DbType.Int16;

        [Test]
        public void VerifyIsWarningSuppressedIsFalseByDefault()
        {
            var options = new MigrationOptions();
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, 0, 0));
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, 1, 0));
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningAlways()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(Platform, Type, SuppressCondition.Always);
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, null, null));
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, 1, null));
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningWithoutSize()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(Platform, Type, SuppressCondition.WhenSpecifiedWithoutSize);
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, null, null));
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, 1, null));
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningWithSize()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(Platform, Type, SuppressCondition.WhenSpecifiedWithSize);
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, null, null));
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, 1, null));
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningWithSizeAndScale()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(Platform, Type, SuppressCondition.WhenSpecifiedWithSizeAndScale);
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, null, null));
            Assert.IsFalse(options.IsWarningSuppressed(Platform, Type, 1, null));
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, 1, 1));
        }

        [Test]
        public void VerifySuppressWarningCombined()
        {
            var options = new MigrationOptions();
            options.SuppressWarning(Platform, Type, SuppressCondition.WhenSpecifiedWithoutSize);
            options.SuppressWarning(Platform, Type, SuppressCondition.WhenSpecifiedWithSize);
            options.SuppressWarning(Platform, Type, SuppressCondition.WhenSpecifiedWithSizeAndScale);
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, null, null));
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, 1, null));
            Assert.IsTrue(options.IsWarningSuppressed(Platform, Type, 1, 1));
        }
    }
}