using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FakeItEasy;
using JetBrains.Annotations;
using MigSharp.Process;
using MigSharp.Providers;
using NUnit.Framework;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class ValidatorFactoryTests
    {
        [TestCaseSource("GetTestCases")]
        public void CheckProviderValidation(DbPlatform platformUnderExecution, DbAltererOptions options, int expectedTotalNumberOfSupportedProviders, int expectedValidationRuns)
        {
            // arrange
            var providerLocator = new ProviderLocator(new ProviderFactory());
            int totalNumberOfSupportedProviders = options.SupportedPlatforms.Sum(n => providerLocator.GetAllForMinimumRequirement(n).Count());
            var validatorFactory = new ValidatorFactory(providerLocator.GetExactly(platformUnderExecution), options, providerLocator);
            Validator validator = validatorFactory.Create();

            var reporter = A.Fake<IMigrationReporter>();
            string errors;
            string warnings;

            // act
            validator.Validate(new[] { reporter }, out errors, out warnings);

            // assert
            Assert.AreEqual(expectedTotalNumberOfSupportedProviders, totalNumberOfSupportedProviders, "Wrong total number of providers.");
            A.CallTo(() => reporter.Report(A<IMigrationContext>._)).MustHaveHappened(Repeated.Exactly.Times(expectedValidationRuns));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [UsedImplicitly]
        private static IEnumerable<ITestCaseData> GetTestCases()
        {
            yield return new TestCase
                {
                    DbPlatformUnderExecution = DbPlatform.MySql5,
                    Options = new DbAltererOptions
                        {
                            SupportedPlatforms =
                                {
                                    DbPlatform.SqlServer2012,
                                    DbPlatform.Oracle12c
                                }
                        },
                    ExpectedTotalNumberOfSupportedProviders = 2,
                    ExpectedValidationRuns = 3
                }.SetName("ProviderUnderExecutionIsValidatedAndSupportedProvidersToo");

            yield return new TestCase
                {
                    DbPlatformUnderExecution = DbPlatform.MySql5,
                    Options = new DbAltererOptions
                        {
                            SupportedPlatforms =
                                {
                                    DbPlatform.SqlServer2008,
                                    DbPlatform.Oracle12c
                                }
                        },
                    ExpectedTotalNumberOfSupportedProviders = 3,
                    ExpectedValidationRuns = 4
                }.SetName("ProviderUnderExecutionIsValidatedAndSupportedProvidersToo (with a reachable minimum requirement)");

            yield return new TestCase
                {
                    DbPlatformUnderExecution = DbPlatform.SqlServer2008,
                    Options = new DbAltererOptions
                        {
                            SupportedPlatforms =
                                {
                                    DbPlatform.SqlServer2008
                                }
                        },
                    ExpectedTotalNumberOfSupportedProviders = 2,
                    ExpectedValidationRuns = 2
                }.SetName("ProviderUnderExecutionIsNotValidatedTwice");

            yield return new TestCase
                {
                    DbPlatformUnderExecution = DbPlatform.SqlServer2008,
                    Options = new DbAltererOptions
                        {
                            Validate = false,
                            SupportedPlatforms =
                                {
                                    DbPlatform.SqlServer2008
                                }
                        },
                    ExpectedTotalNumberOfSupportedProviders = 2,
                    ExpectedValidationRuns = 1
                }.SetName("CheckValidateIsRepected");
        }

        private class TestCase : TestCaseData, ITestCaseData
        {
            public DbPlatform DbPlatformUnderExecution { get; set; }
            public DbAltererOptions Options { get; set; }
            public int ExpectedTotalNumberOfSupportedProviders { get; set; }
            public int ExpectedValidationRuns { get; set; }

            public new object[] Arguments { get { return new object[] { DbPlatformUnderExecution, Options, ExpectedTotalNumberOfSupportedProviders, ExpectedValidationRuns }; } }
        }
    }
}