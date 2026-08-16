namespace GateKeeper.Test.Shared
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using GateKeeper;
    using Touchstone.Core;

    /// <summary>
    /// Central registry of shared Touchstone test suites for GateKeeper. All test logic
    /// lives in <see cref="Test.Shared"/> and is executed identically by Test.Automated
    /// (console), Test.Xunit, and Test.Nunit. Add new suites to <see cref="All"/> and they
    /// automatically light up across every runner.
    /// </summary>
    public static class GateKeeperSuites
    {
        #region Public-Members

        /// <summary>
        /// All test suites. Adapters enumerate this property to expose tests to their runner.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    ServerSuite(),

                    ManagerSuites.UserManagerSuite(),
                    ManagerSuites.RoleManagerSuite(),
                    ManagerSuites.ResourceManagerSuite(),
                    ManagerSuites.PermissionManagerSuite(),
                    ManagerSuites.UserRoleManagerSuite(),

                    AuthorizationSuites.AuthorizationSuite(),
                    AuthorizationSuites.DefaultPermitSuite(),
                    AuthorizationSuites.AuthorizationEventSuite(),
                    AuthorizationSuites.CascadeDeleteSuite(),
                    AuthorizationSuites.SqlInjectionSuite(),

                    ModelSuites.UserModelSuite(),
                    ModelSuites.RoleModelSuite(),
                    ModelSuites.ResourceModelSuite(),
                    ModelSuites.PermissionModelSuite(),
                    ModelSuites.UserRoleModelSuite(),
                    ModelSuites.SanitizationSuite(),
                };
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Server construction and top-level surface.
        /// </summary>
        /// <returns>Server suite.</returns>
        public static TestSuiteDescriptor ServerSuite()
        {
            const string suiteId = "Server";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "RbacServer - Construction and Surface",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "Instantiation", "Server instantiates with a temp database file",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.NotNull(scope.Server, "Server instance");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "VersionNotEmpty", "Version returns a non-empty string",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.True(!string.IsNullOrEmpty(scope.Server.Version), "Version populated");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "DefaultPermitFalse", "DefaultPermit defaults to false",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestAssert.False(scope.Server.DefaultPermit, "DefaultPermit default");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ManagersPresent", "All five managers are exposed and non-null",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            RbacServer s = scope.Server;
                            TestAssert.NotNull(s.Users, "Users manager");
                            TestAssert.NotNull(s.Roles, "Roles manager");
                            TestAssert.NotNull(s.Resources, "Resources manager");
                            TestAssert.NotNull(s.Permissions, "Permissions manager");
                            TestAssert.NotNull(s.UserRoles, "UserRoles manager");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullDbFileThrows", "Constructor throws ArgumentNullException on null db file",
                        ct =>
                        {
                            TestAssert.Throws<System.ArgumentNullException>(
                                () => new RbacServer(null!), "Null db file");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "EmptyDbFileThrows", "Constructor throws ArgumentNullException on empty db file",
                        ct =>
                        {
                            TestAssert.Throws<System.ArgumentNullException>(
                                () => new RbacServer(string.Empty), "Empty db file");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion
    }
}
