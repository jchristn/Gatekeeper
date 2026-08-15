namespace GateKeeper.Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;

    using GateKeeper.Test.Shared;
    using Touchstone.Core;
    using Touchstone.XunitAdapter;
    using global::Xunit;

    /// <summary>
    /// xUnit host for the GateKeeper Touchstone suites. Each shared TestCaseDescriptor
    /// is surfaced as its own xUnit theory row, so individual failures are reported
    /// one-per-test in IDE and CI output.
    /// </summary>
    public sealed class GateKeeperTheoryTests
    {
        /// <summary>
        /// Provides every non-skipped TestCaseDescriptor from the shared suites as xUnit theory data.
        /// </summary>
        public static TouchstoneTheoryData TestCases
        {
            get { return new TouchstoneTheoryData(GateKeeperSuites.All); }
        }

        /// <summary>
        /// Executes a single Touchstone test case under xUnit.
        /// </summary>
        /// <param name="testCase">The test case to execute.</param>
        /// <returns>Task representing the test run.</returns>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task RunTouchstoneCase(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
