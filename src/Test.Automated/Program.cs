namespace GateKeeper.Test.Automated
{
    using System;
    using System.Threading.Tasks;

    using GateKeeper.Test.Shared;
    using Touchstone.Cli;

    /// <summary>
    /// Console runner for the GateKeeper Touchstone test suites.
    /// Exit code 0 = all passed; exit code 1 = at least one failure.
    /// Optional argument: <c>--results &lt;path&gt;</c> to write a JSON results file.
    /// </summary>
    public static class Program
    {
        #region Entrypoint

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Process exit code.</returns>
        public static async Task<int> Main(string[] args)
        {
            string? resultsPath = ParseResultsPath(args);
            int exitCode = await ConsoleRunner.RunAsync(GateKeeperSuites.All, null, resultsPath).ConfigureAwait(false);
            return exitCode;
        }

        #endregion

        #region Private-Methods

        private static string? ParseResultsPath(string[]? args)
        {
            if (args == null) return null;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                int equalsIndex = arg.IndexOf('=');
                if (equalsIndex > 0)
                {
                    if (arg.Substring(0, equalsIndex).Equals("--results", StringComparison.OrdinalIgnoreCase))
                    {
                        return arg.Substring(equalsIndex + 1);
                    }
                }
                else if (arg.Equals("--results", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        #endregion
    }
}
