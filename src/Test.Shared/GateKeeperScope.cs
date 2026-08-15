namespace GateKeeper.Test.Shared
{
    using System;
    using System.IO;
    using GateKeeper;

    /// <summary>
    /// Disposable test scope that owns an <see cref="RbacServer"/> backed by a unique,
    /// throw-away SQLite database file. Each test case gets its own scope so cases are
    /// fully isolated and can run in parallel under the xUnit and NUnit hosts.
    /// </summary>
    public sealed class GateKeeperScope : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// The RBAC server under test.
        /// </summary>
        public RbacServer Server { get; private set; }

        /// <summary>
        /// Full path to the backing database file.
        /// </summary>
        public string DatabaseFile
        {
            get { return _DbFile; }
        }

        #endregion

        #region Private-Members

        private readonly string _DbFile;
        private bool _Disposed;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create a new scope with a unique backing database file.
        /// </summary>
        public GateKeeperScope()
        {
            _DbFile = Path.Combine(
                Path.GetTempPath(),
                "gatekeeper-test-" + Guid.NewGuid().ToString("N") + ".db");

            TryDeleteAll(_DbFile);
            Server = new RbacServer(_DbFile);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose the scope and best-effort delete the backing database file.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            Server = null!;

            // WatsonORM/SQLite holds no persistent handle once the server is unreferenced,
            // but force finalization so any lingering connection is released before delete.
            GC.Collect();
            GC.WaitForPendingFinalizers();

            TryDeleteAll(_DbFile);
        }

        #endregion

        #region Private-Methods

        private static void TryDeleteAll(string dbFile)
        {
            TryDelete(dbFile);
            TryDelete(dbFile + "-wal");
            TryDelete(dbFile + "-shm");
            TryDelete(dbFile + "-journal");
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // best-effort cleanup; a locked temp file is harmless
            }
        }

        #endregion
    }
}
