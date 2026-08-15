namespace GateKeeper.Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;

    using GateKeeper.Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;
    using global::NUnit.Framework;

    /// <summary>
    /// NUnit host for the GateKeeper Touchstone suites. Each shared TestCaseDescriptor
    /// is enumerated as a distinct NUnit test case.
    /// </summary>
    [TestFixture]
    public sealed class GateKeeperNunitTests
    {
        /// <summary>
        /// Enumerator over all non-skipped Touchstone test cases.
        /// </summary>
        /// <returns>Enumerator of TestCaseDescriptor values.</returns>
        public static IEnumerable TestCases()
        {
            return new TouchstoneTestCaseSource(GateKeeperSuites.All);
        }

        /// <summary>
        /// Executes a single Touchstone test case under NUnit.
        /// </summary>
        /// <param name="testCase">The test case to execute.</param>
        /// <returns>Task representing the test run.</returns>
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task RunTouchstoneCase(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
