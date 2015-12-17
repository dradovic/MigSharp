using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MigSharp.Core;
using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.NUnit.Providers
{
    [TestFixture, Category("smoke")]
    public class RecordingProviderTests
    {
        [Test]
        public void VerifyAllProviderMethodsArePreventedFromBeingInlined() // this is important as the RecordingProvider.AddMethodName expects a certain call-stack
        {
            foreach (MethodInfo providerMethod in typeof(IProvider).GetMethods())
            {
                MethodInfo method = typeof(RecordingProvider).GetMethod(providerMethod.Name);
                Assert.AreEqual(MethodImplAttributes.NoInlining, method.GetMethodImplementationFlags(), string.Format(CultureInfo.CurrentCulture, "Method '{0}' is missing the [MethodImpl(MethodImplOptions.NoInlining)] attribute.", method.Name));
            }
        }

        [Test]
        public void VerifyMethodNamesAreRecorded()
        {
            var provider = new RecordingProvider();
// ReSharper disable ReturnValueOfPureMethodIsNotUsed : needed to force enumeration
            provider.RenameColumn(new TableName("Table", null), "OldColumn", "NewColumn").ToList();
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
            CollectionAssert.AreEquivalent(new[] { "RenameColumn" }, provider.Methods.ToList());
        }

        [Test]
        public void VerifyNewObjectNamesAreRecorded()
        {
            var provider = new RecordingProvider();
// ReSharper disable ReturnValueOfPureMethodIsNotUsed : needed to force enumeration
            provider.CreateTable(new TableName("Table", null), new[] { new CreatedColumn("Column", new DataType(DbType.Boolean), false, true, string.Empty, false, null, false) }, "MyPK").ToList();
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
            CollectionAssert.AreEquivalent(new[] { "Table", "Column", "MyPK" }, provider.NewObjectNames.ToList());
        }

        [Test]
        public void VerifyDateTypesAreRecorded()
        {
            var provider = new RecordingProvider();
            provider.CreateTable(new TableName("Table", null), new[]
            {
                new CreatedColumn("Primary Key Column", new DataType(DbType.Int32), false, true, string.Empty, false, null, false),
                new CreatedColumn("Identity Column", new DataType(DbType.Int64), false, false, string.Empty, true, null, false),
                new CreatedColumn("Column", new DataType(DbType.String, 10), false, false, string.Empty, false, null, false),
// ReSharper disable ReturnValueOfPureMethodIsNotUsed : needed to force enumeration
            }, "MyPK").ToList();
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
            CollectionAssert.AreEquivalent(new[]
            {
                new UsedDataType(new DataType(DbType.Int32), true, false),
                new UsedDataType(new DataType(DbType.Int64), false, true),
                new UsedDataType(new DataType(DbType.String, 10), false, false),
            }, provider.DataTypes.ToList());
        }
    }
}