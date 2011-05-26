using System;

using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.NUnit.Providers
{
    [TestFixture]
    public class ObjectNameHelperTests
    {
        [Test]
        public void VerifyGetObjectName()
        {
            Assert.AreEqual("Customer_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 128), "Basic.");
            Assert.AreEqual("Customer_Id_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 128, "Id"), "Basic with one additional name.");
            Assert.AreEqual("Customer_Id_Name_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 128, "Id", "Name"), "Basic with two additional names.");
            Assert.AreEqual("Cus-mer_Id_Name_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 18, "Id", "Name"), "One char too long.");
            Assert.AreEqual("Cus-er_Id_Name_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 17, "Id", "Name"), "Two chars too long.");
            Assert.AreEqual("Cu-er_Id_Name_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 16, "Id", "Name"), "Three chars too long.");
            Assert.AreEqual("Cu-r_Id_Name_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 15, "Id", "Name"), "Four chars too long.");
            Assert.AreEqual("C-r_Id_Name_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 14, "Id", "Name"), "Five chars too long.");
            Assert.AreEqual("C-r_Id_N-e_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 13, "Id", "Name"), "Six chars too long.");
            Assert.AreEqual("C_Id_N-e_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 12, "Id", "Name"), "Seven chars too long.");
            Assert.AreEqual("C_Id_N-e_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 11, "Id", "Name"), "Eight chars too long.");
            Assert.AreEqual("C_Id_N_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 10, "Id", "Name"), "Nine chars too long.");
            Assert.AreEqual("C_Id_N_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 9, "Id", "Name"), "Ten chars too long.");
            Assert.AreEqual("C_I_N_DF", ObjectNameHelper.GetObjectName("Customer", "DF", 8, "Id", "Name"), "Eleven chars too long.");
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyGetObjectNameThrowsIfTooShort()
        {
            ObjectNameHelper.GetObjectName("Customer", "DF", 7, "Id", "Name");
        }
    }
}