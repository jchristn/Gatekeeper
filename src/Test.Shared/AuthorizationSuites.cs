namespace GateKeeper.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using GateKeeper;
    using Touchstone.Core;

    /// <summary>
    /// End-to-end authorization behavior: allow/deny resolution, the DefaultPermit
    /// fallback, the asynchronous authorization event, and cascade deletes.
    /// </summary>
    public static class AuthorizationSuites
    {
        #region Authorization

        /// <summary>
        /// Core Authorize() decision coverage over the canonical graph.
        /// </summary>
        /// <returns>Authorization suite.</returns>
        public static TestSuiteDescriptor AuthorizationSuite()
        {
            const string suiteId = "Authorization";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Authorization - Decisions",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "AllowedCreate", "Permitted operation returns true",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("alice", "create", "documents"), "alice create documents");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AllowedRead", "Second permitted operation returns true",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("alice", "read", "documents"), "alice read documents");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ExplicitDenyReturnsFalse", "Operation with only an explicit deny returns false",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.False(scope.Server.Authorize("alice", "delete", "documents"), "alice delete documents denied");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AllowWinsOverDeny", "When both allow and deny match, allow wins",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("alice", "share", "documents"), "allow wins over deny");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "OtherRoleGrant", "A different role's grant authorizes its user",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("bob", "update", "documents"), "bob update documents");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "MissingGrantReturnsFalse", "A user whose role lacks the grant is denied",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.False(scope.Server.Authorize("bob", "create", "documents"), "bob cannot create");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "WrongResourceReturnsFalse", "A grant on another resource does not apply",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.False(scope.Server.Authorize("carol", "read", "documents"), "carol cannot read documents");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "CrossResourceGrantApplies", "viewer can read reports",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("carol", "read", "reports"), "carol read reports");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "UnknownUserReturnsFalse", "Unknown user is denied",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.False(scope.Server.Authorize("dave", "create", "documents"), "unknown user denied");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "UnknownResourceReturnsFalse", "Unknown resource is denied",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.False(scope.Server.Authorize("alice", "create", "unknown-resource"), "unknown resource denied");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "UnknownOperationReturnsFalse", "Unknown operation is denied",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.False(scope.Server.Authorize("alice", "teleport", "documents"), "unknown operation denied");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "RevokedRoleRevokesAccess", "Removing the user-role mapping revokes access",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Graph g = TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("alice", "create", "documents"), "alice initially allowed");
                            scope.Server.UserRoles.Remove(g.Alice, g.Admin);
                            TestAssert.False(scope.Server.Authorize("alice", "create", "documents"), "alice denied after revoke");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "MetadataOverloadAllows", "Authorize with metadata still returns the correct decision",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("alice", "create", "documents", new { RequestId = 42 }), "metadata overload");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullUsernameThrows", "Authorize throws ArgumentNullException on null username",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Authorize(null!, "create", "documents"), "null username");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullOperationThrows", "Authorize throws ArgumentNullException on null operation",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Authorize("alice", null!, "documents"), "null operation");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NullResourceThrows", "Authorize throws ArgumentNullException on null resource",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.Throws<ArgumentNullException>(
                                () => scope.Server.Authorize("alice", "create", null!), "null resource");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region DefaultPermit

        /// <summary>
        /// DefaultPermit fallback behavior when no permission matches.
        /// </summary>
        /// <returns>DefaultPermit suite.</returns>
        public static TestSuiteDescriptor DefaultPermitSuite()
        {
            const string suiteId = "DefaultPermit";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Authorization - DefaultPermit",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "FalseDeniesUnmatched", "DefaultPermit=false denies an unmatched request",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            scope.Server.DefaultPermit = false;
                            TestAssert.False(scope.Server.Authorize("alice", "no-such-op", "documents"), "unmatched denied");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "TrueAllowsUnmatched", "DefaultPermit=true allows an unmatched request",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            scope.Server.DefaultPermit = true;
                            TestAssert.True(scope.Server.Authorize("alice", "no-such-op", "documents"), "unmatched allowed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "TrueDoesNotOverrideExplicitDeny", "DefaultPermit=true does not override an explicit deny match",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            scope.Server.DefaultPermit = true;
                            // 'delete' has a matching deny row, so the fallback never applies.
                            TestAssert.False(scope.Server.Authorize("alice", "delete", "documents"), "explicit deny still wins");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "TrueAllowsUnknownUser", "DefaultPermit=true allows even an unknown user (no match)",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            scope.Server.DefaultPermit = true;
                            TestAssert.True(scope.Server.Authorize("nobody", "create", "documents"), "unknown user allowed by fallback");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Authorization-Event

        /// <summary>
        /// AuthorizationEvent payload coverage.
        /// </summary>
        /// <returns>Event suite.</returns>
        public static TestSuiteDescriptor AuthorizationEventSuite()
        {
            const string suiteId = "Events";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Authorization - Events",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "FiresWithRequestFields", "Event fires and carries the request fields",
                        async ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            AuthorizationEventArgs e = await CaptureEventAsync(
                                scope.Server, () => scope.Server.Authorize("alice", "create", "documents"), ct).ConfigureAwait(false);
                            TestAssert.Equal("alice", e.Username, "Username");
                            TestAssert.Equal("create", e.Operation, "Operation");
                            TestAssert.Equal("documents", e.Resource, "Resource");
                        }),

                    new TestCaseDescriptor(suiteId, "AuthorizedTrueForGrant", "Event reports Authorized=true for a granted request",
                        async ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            AuthorizationEventArgs e = await CaptureEventAsync(
                                scope.Server, () => scope.Server.Authorize("alice", "create", "documents"), ct).ConfigureAwait(false);
                            TestAssert.True(e.Authorized, "Authorized true");
                            TestAssert.NotNull(e.MatchingEntries, "MatchingEntries present");
                            TestAssert.True(e.MatchingEntries.Count > 0, "Has matching entries");
                        }),

                    new TestCaseDescriptor(suiteId, "AuthorizedFalseForDenied", "Event reports Authorized=false for a denied request",
                        async ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            AuthorizationEventArgs e = await CaptureEventAsync(
                                scope.Server, () => scope.Server.Authorize("alice", "delete", "documents"), ct).ConfigureAwait(false);
                            TestAssert.False(e.Authorized, "Authorized false");
                        }),

                    new TestCaseDescriptor(suiteId, "MetadataFlowsThrough", "Event carries the supplied metadata",
                        async ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            AuthorizationEventArgs e = await CaptureEventAsync(
                                scope.Server, () => scope.Server.Authorize("alice", "create", "documents", "trace-123"), ct).ConfigureAwait(false);
                            TestAssert.NotNull(e.Metadata, "Metadata present");
                            TestAssert.Equal("trace-123", e.Metadata!.ToString(), "Metadata value");
                        }),

                    new TestCaseDescriptor(suiteId, "UnknownUserFiresWithNoMatches", "Event fires for an unknown user with zero matching entries",
                        async ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            AuthorizationEventArgs e = await CaptureEventAsync(
                                scope.Server, () => scope.Server.Authorize("nobody", "create", "documents"), ct).ConfigureAwait(false);
                            TestAssert.False(e.Authorized, "Unknown user not authorized");
                            TestAssert.NotNull(e.MatchingEntries, "MatchingEntries present");
                            TestAssert.Equal(0, e.MatchingEntries.Count, "No matching entries");
                        }),
                });
        }

        #endregion

        #region Cascade-Delete

        /// <summary>
        /// Cascade delete coverage: removing a principal cleans up its mappings/permissions.
        /// </summary>
        /// <returns>Cascade suite.</returns>
        public static TestSuiteDescriptor CascadeDeleteSuite()
        {
            const string suiteId = "Cascade";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Cascade Delete",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "DeleteUserRemovesMappings", "Deleting a user removes its user-role mappings",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Graph g = TestData.Standard(scope.Server);
                            scope.Server.Users.Remove(g.Alice);
                            List<UserRole> forRole = scope.Server.UserRoles.GetByRole(g.Admin);
                            TestAssert.False(forRole.Exists(m => m.UserGUID == g.Alice.GUID), "alice mapping gone");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "DeleteRoleRemovesMappings", "Deleting a role removes its user-role mappings",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Graph g = TestData.Standard(scope.Server);
                            scope.Server.Roles.Remove(g.Admin);
                            List<UserRole> forUser = scope.Server.UserRoles.GetByUser(g.Alice);
                            TestAssert.False(forUser.Exists(m => m.RoleGUID == g.Admin.GUID), "admin mapping gone");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "DeleteResourceRemovesPermissions", "Deleting a resource removes its permissions",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Graph g = TestData.Standard(scope.Server);
                            scope.Server.Resources.Remove(g.Documents);
                            List<Permission> forResource = scope.Server.Permissions.GetByResource(g.Documents);
                            TestAssert.Equal(0, forResource.Count, "documents permissions gone");
                            TestAssert.False(scope.Server.Permissions.ExistsByName("admin-doc-create"), "named permission gone");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "DeleteResourceRevokesAuthorization", "After deleting a resource, authorization against it is denied",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Graph g = TestData.Standard(scope.Server);
                            scope.Server.Resources.Remove(g.Documents);
                            TestAssert.False(scope.Server.Authorize("alice", "create", "documents"), "no access after resource delete");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Private-Methods

        private static async Task<AuthorizationEventArgs> CaptureEventAsync(
            RbacServer server, Action trigger, CancellationToken cancellationToken)
        {
            TaskCompletionSource<AuthorizationEventArgs> tcs =
                new TaskCompletionSource<AuthorizationEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<AuthorizationEventArgs> handler = (sender, e) => tcs.TrySetResult(e);
            server.AuthorizationEvent += handler;
            try
            {
                trigger();

                Task delay = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                Task completed = await Task.WhenAny(tcs.Task, delay).ConfigureAwait(false);
                if (completed != tcs.Task)
                {
                    throw new InvalidOperationException("AuthorizationEvent did not fire within the timeout.");
                }

                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                server.AuthorizationEvent -= handler;
            }
        }

        #endregion
    }
}
