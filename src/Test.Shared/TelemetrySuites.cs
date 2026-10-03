namespace GateKeeper.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using GateKeeper;
    using Touchstone.Core;

    using N = GateKeeper.GateKeeperTelemetryNames;

    /// <summary>
    /// Proves GateKeeper emits its documented metrics and spans for every inventory category:
    /// authorization (and each stage), the asynchronous event hand-off, management operations,
    /// the SQLite client, initialization, build info, and the failure paths. Also proves the
    /// library keeps working with no listener and with listeners that throw.
    /// </summary>
    public static class TelemetrySuites
    {
        #region Telemetry

        /// <summary>
        /// Telemetry emission coverage.
        /// </summary>
        /// <returns>Telemetry suite.</returns>
        public static TestSuiteDescriptor TelemetrySuite()
        {
            const string suiteId = "Telemetry";
            return new TestSuiteDescriptor(
                suiteId: suiteId,
                displayName: "Telemetry - Metrics and Spans",
                cases: new List<TestCaseDescriptor>
                {
                    new TestCaseDescriptor(suiteId, "SourceNamesStable", "Meter and activity source names are the documented public contract",
                        ct =>
                        {
                            TestAssert.Equal("GateKeeper", N.MeterName, "Meter name");
                            TestAssert.Equal("GateKeeper", N.ActivitySourceName, "Activity source name");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AuthorizeAllowMatched", "Allowed request emits root span, stage spans, db client span, and metrics",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            using TelemetryCapture capture = new TelemetryCapture();

                            TestAssert.True(scope.Server.Authorize("alice", "create", "documents"), "alice create documents");

                            Activity root = Single(capture.Spans(N.SpanAuthorize), "Authorize span");
                            TestAssert.Equal(ActivityStatusCode.Ok, root.Status, "Authorize span status");
                            TestAssert.Equal(N.DecisionAllow, root.GetTagItem(N.AttributeDecision)?.ToString(), "span decision");
                            TestAssert.Equal(N.BasisMatched, root.GetTagItem(N.AttributeDecisionBasis)?.ToString(), "span basis");
                            TestAssert.Equal("create", root.GetTagItem(N.AttributeAuthorizationOperation)?.ToString(), "span operation");
                            TestAssert.Equal("documents", root.GetTagItem(N.AttributeAuthorizationResource)?.ToString(), "span resource");
                            TestAssert.Equal(capture.Root.SpanId, root.ParentSpanId, "Authorize nests under caller span");

                            foreach (string stage in new[] { N.StageBuildQuery, N.StageQuery, N.StageEvaluate })
                            {
                                Activity stageSpan = Single(capture.Spans(N.SpanStagePrefix + stage), "stage span " + stage);
                                TestAssert.Equal(root.SpanId, stageSpan.ParentSpanId, "stage " + stage + " parent");
                                TestAssert.True(capture.Measurements(N.AuthorizationStageDuration).Any(m =>
                                    m.Has(N.AttributeStage, stage, N.AttributeOutcome, N.OutcomeSuccess)), "stage histogram " + stage);
                            }

                            Activity db = Single(capture.Spans(N.SpanDbPrefix + N.DbOperationSelect + " " + N.DbCollectionAuthorization), "db span");
                            TestAssert.Equal(ActivityKind.Client, db.Kind, "db span kind");
                            TestAssert.Equal(N.DbSystemSqlite, db.GetTagItem(N.AttributeDbSystemName)?.ToString(), "db.system.name");
                            TestAssert.Equal(Single(capture.Spans(N.SpanStagePrefix + N.StageQuery), "query stage").SpanId, db.ParentSpanId, "db span under query stage");

                            AssertMeasured(capture, N.AuthorizationRequests, N.AttributeDecision, N.DecisionAllow, N.AttributeDecisionBasis, N.BasisMatched);
                            AssertMeasured(capture, N.AuthorizationDuration, N.AttributeDecision, N.DecisionAllow, N.AttributeDecisionBasis, N.BasisMatched);
                            TestAssert.Equal(1.0, Single(capture.Measurements(N.AuthorizationMatchedEntries), "matched entries").Value, "matched entries value");
                            AssertMeasured(capture, N.DbClientOperations, N.AttributeDbOperationName, N.DbOperationSelect,
                                N.AttributeDbCollectionName, N.DbCollectionAuthorization, N.AttributeOutcome, N.OutcomeSuccess);
                            AssertMeasured(capture, N.DbClientOperationDuration, N.AttributeDbCollectionName, N.DbCollectionAuthorization);
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AuthorizeExplicitDeny", "Explicit deny is recorded as deny/matched",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            using TelemetryCapture capture = new TelemetryCapture();

                            TestAssert.False(scope.Server.Authorize("alice", "delete", "documents"), "alice delete denied");
                            AssertMeasured(capture, N.AuthorizationRequests, N.AttributeDecision, N.DecisionDeny, N.AttributeDecisionBasis, N.BasisMatched);
                            TestAssert.Equal(ActivityStatusCode.Ok, Single(capture.Spans(N.SpanAuthorize), "span").Status, "deny is not an error");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AuthorizeDefaultDeny", "No matching entries with DefaultPermit=false is recorded as deny/default",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            using TelemetryCapture capture = new TelemetryCapture();

                            TestAssert.False(scope.Server.Authorize("carol", "delete", "reports"), "carol delete reports");
                            AssertMeasured(capture, N.AuthorizationRequests, N.AttributeDecision, N.DecisionDeny, N.AttributeDecisionBasis, N.BasisDefault);
                            TestAssert.Equal(0.0, Single(capture.Measurements(N.AuthorizationMatchedEntries), "matched entries").Value, "zero matched");
                            TestAssert.Equal(false, Single(capture.Spans(N.SpanAuthorize), "span").GetTagItem(N.AttributeDefaultPermit), "default_permit attribute");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AuthorizeDefaultPermit", "No matching entries with DefaultPermit=true is recorded as allow/default",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            scope.Server.DefaultPermit = true;
                            using TelemetryCapture capture = new TelemetryCapture();

                            TestAssert.True(scope.Server.Authorize("carol", "delete", "reports"), "default permit");
                            AssertMeasured(capture, N.AuthorizationRequests, N.AttributeDecision, N.DecisionAllow, N.AttributeDecisionBasis, N.BasisDefault);
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "AuthorizeInvalidArgumentIsError", "A rejected request is recorded as error with error.type and an Error span",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            using TelemetryCapture capture = new TelemetryCapture();

                            TestAssert.Throws<ArgumentNullException>(() => scope.Server.Authorize(null!, "read", "documents"), "null user");

                            Activity span = Single(capture.Spans(N.SpanAuthorize), "Authorize span");
                            TestAssert.Equal(ActivityStatusCode.Error, span.Status, "span status");
                            TestAssert.Equal(typeof(ArgumentNullException).FullName, span.GetTagItem(N.AttributeErrorType)?.ToString(), "span error.type");
                            TestAssert.True(span.Events.Any(e => e.Name == "exception"), "exception event recorded");
                            AssertMeasured(capture, N.AuthorizationRequests, N.AttributeDecision, N.DecisionError, N.AttributeDecisionBasis, N.BasisNone);
                            AssertMeasured(capture, N.Errors, N.AttributeComponent, N.ComponentAuthorization, N.AttributeErrorType, typeof(ArgumentNullException).FullName!);
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NoUsernameInTelemetry", "The username never appears on any span attribute or metric label",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Users.Add(new User("zz-private-user"));
                            using TelemetryCapture capture = new TelemetryCapture();

                            scope.Server.Authorize("zz-private-user", "read", "documents");
                            scope.Server.Users.GetFirstByName("zz-private-user");

                            foreach (Activity a in capture.Spans())
                                foreach (KeyValuePair<string, object?> tag in a.TagObjects)
                                    TestAssert.True(tag.Value?.ToString() != "zz-private-user", "username leaked on span " + a.DisplayName + " tag " + tag.Key);

                            foreach (CapturedMeasurement m in capture.AllMeasurements())
                                foreach (KeyValuePair<string, object?> tag in m.Tags)
                                    TestAssert.True(tag.Value?.ToString() != "zz-private-user", "username leaked on metric " + m.Name);
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ManagementEveryEntity", "Each manager emits a span and a top-level operation metric per call",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            using TelemetryCapture capture = new TelemetryCapture();
                            RbacServer s = scope.Server;

                            User u = s.Users.Add(new User("alice"));
                            Role r = s.Roles.Add(new Role("admin"));
                            Resource res = s.Resources.Add(new Resource("documents"));
                            s.Permissions.Add(new Permission("p1", r, res, "read", true));
                            s.UserRoles.Add(u, r);
                            List<Permission> perms = s.Permissions.GetByResource(res);

                            foreach (string entity in new[] { N.EntityUser, N.EntityRole, N.EntityResource, N.EntityPermission, N.EntityUserRole })
                            {
                                AssertMeasured(capture, N.ManagementOperations, N.AttributeEntity, entity, N.AttributeManagementOperation, N.OperationAdd, N.AttributeOutcome, N.OutcomeSuccess);
                                AssertMeasured(capture, N.ManagementDuration, N.AttributeEntity, entity, N.AttributeManagementOperation, N.OperationAdd);
                                TestAssert.True(capture.Spans(N.SpanManagementPrefix + entity + "." + N.OperationAdd).Count == 1, "add span for " + entity);
                            }

                            // Internal existence checks appear as nested spans but are not counted as top-level operations.
                            TestAssert.False(capture.Measurements(N.ManagementOperations).Any(m =>
                                m.Has(N.AttributeManagementOperation, N.OperationExistsByName)), "nested exists_by_name not counted");
                            Activity add = Single(capture.Spans(N.SpanManagementPrefix + N.EntityUser + "." + N.OperationAdd), "user add span");
                            TestAssert.True(capture.Spans(N.SpanManagementPrefix + N.EntityUser + "." + N.OperationExistsByName)
                                .Any(a => a.ParentSpanId == add.SpanId), "nested exists span under add");

                            Activity get = Single(capture.Spans(N.SpanManagementPrefix + N.EntityPermission + "." + N.OperationGetByResource), "get_by_resource span");
                            TestAssert.Equal(perms.Count, (int)get.GetTagItem(N.AttributeResultCount)!, "result count attribute");

                            AssertMeasured(capture, N.DbClientOperations, N.AttributeDbOperationName, N.DbOperationInsert, N.AttributeDbCollectionName, N.DbCollectionUserRoles);
                            AssertMeasured(capture, N.DbClientOperations, N.AttributeDbOperationName, N.DbOperationExists, N.AttributeDbCollectionName, N.DbCollectionUsers);
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ManagementFailures", "Duplicate add and missing remove are recorded as errors with error.type",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            scope.Server.Users.Add(new User("alice"));
                            using TelemetryCapture capture = new TelemetryCapture();

                            TestAssert.Throws<ArgumentException>(() => scope.Server.Users.Add(new User("alice")), "duplicate add");
                            TestAssert.Throws<KeyNotFoundException>(() => scope.Server.Roles.RemoveByName("missing"), "missing remove");

                            AssertMeasured(capture, N.ManagementOperations, N.AttributeEntity, N.EntityUser, N.AttributeManagementOperation, N.OperationAdd,
                                N.AttributeOutcome, N.OutcomeError, N.AttributeErrorType, typeof(ArgumentException).FullName!);
                            AssertMeasured(capture, N.ManagementOperations, N.AttributeEntity, N.EntityRole, N.AttributeManagementOperation, N.OperationRemoveByName,
                                N.AttributeOutcome, N.OutcomeError, N.AttributeErrorType, typeof(KeyNotFoundException).FullName!);
                            AssertMeasured(capture, N.Errors, N.AttributeComponent, N.ComponentManagement);
                            TestAssert.Equal(ActivityStatusCode.Error,
                                Single(capture.Spans(N.SpanManagementPrefix + N.EntityUser + "." + N.OperationAdd), "add span").Status, "add span status");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "DbFailure", "A failing SQLite call is recorded on the db client metrics and an Error client span",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            BreakDatabase(scope.DatabaseFile);
                            using TelemetryCapture capture = new TelemetryCapture();

                            bool threw = false;
                            try { scope.Server.Users.All(); } catch (Exception) { threw = true; }
                            TestAssert.True(threw, "query against a broken database throws");

                            AssertMeasured(capture, N.DbClientOperations, N.AttributeDbOperationName, N.DbOperationSelect,
                                N.AttributeDbCollectionName, N.DbCollectionUsers, N.AttributeOutcome, N.OutcomeError);
                            TestAssert.True(capture.Measurements(N.DbClientOperations).Any(m => m.Tag(N.AttributeErrorType) != null), "db error.type");
                            AssertMeasured(capture, N.Errors, N.AttributeComponent, N.ComponentDb);
                            Activity db = Single(capture.Spans(N.SpanDbPrefix + N.DbOperationSelect + " " + N.DbCollectionUsers), "db span");
                            TestAssert.Equal(ActivityStatusCode.Error, db.Status, "db span status");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "Initialization", "Constructing a server emits the initialize span, schema db span, and creation metrics",
                        ct =>
                        {
                            string file = Path.Combine(Path.GetTempPath(), "gatekeeper-telemetry-" + Guid.NewGuid().ToString("N") + ".db");
                            try
                            {
                                using TelemetryCapture capture = new TelemetryCapture();
                                RbacServer server = new RbacServer(file);

                                Activity init = Single(capture.Spans(N.SpanInitialize), "initialize span");
                                TestAssert.Equal(ActivityStatusCode.Ok, init.Status, "initialize status");
                                Activity schema = Single(capture.Spans(N.SpanDbPrefix + N.DbOperationInitialize + " " + N.DbCollectionSchema), "schema span");
                                TestAssert.Equal(init.SpanId, schema.ParentSpanId, "schema under initialize");
                                AssertMeasured(capture, N.ServersCreated, N.AttributeOutcome, N.OutcomeSuccess);
                                AssertMeasured(capture, N.ServerInitializationDuration, N.AttributeOutcome, N.OutcomeSuccess);
                            }
                            finally
                            {
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                try { File.Delete(file); } catch { }
                            }

                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "InitializationFailure", "A rejected construction is recorded as an initialization error",
                        ct =>
                        {
                            using TelemetryCapture capture = new TelemetryCapture();
                            TestAssert.Throws<ArgumentNullException>(() => new RbacServer(string.Empty), "empty db file");

                            AssertMeasured(capture, N.ServersCreated, N.AttributeOutcome, N.OutcomeError);
                            AssertMeasured(capture, N.Errors, N.AttributeComponent, N.ComponentInitialization);
                            TestAssert.Equal(ActivityStatusCode.Error, Single(capture.Spans(N.SpanInitialize), "initialize span").Status, "initialize status");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "EventDispatchPropagatesContext", "The background event hand-off joins the Authorize trace and records dispatch metrics",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            scope.Server.AuthorizationEvent += (sender, e) => { };
                            using TelemetryCapture capture = new TelemetryCapture();

                            scope.Server.Authorize("alice", "read", "documents");
                            capture.WaitFor(() => capture.Measurements(N.AuthorizationEventDispatches).Count > 0
                                && capture.Spans(N.SpanAuthorizationEvent).Count > 0, "event dispatch");

                            Activity root = Single(capture.Spans(N.SpanAuthorize), "Authorize span");
                            Activity evt = Single(capture.Spans(N.SpanAuthorizationEvent), "event span");
                            TestAssert.Equal(root.TraceId, evt.TraceId, "same trace");
                            TestAssert.Equal(root.SpanId, evt.ParentSpanId, "event span parented to Authorize");
                            AssertMeasured(capture, N.AuthorizationEventDispatches, N.AttributeOutcome, N.OutcomeSuccess);
                            AssertMeasured(capture, N.AuthorizationEventDuration, N.AttributeOutcome, N.OutcomeSuccess);
                            TestAssert.True(capture.Measurements(N.AuthorizationEventQueueDuration).Count == 1, "queue duration recorded");
                            TestAssert.Equal(0.0, capture.Measurements(N.AuthorizationEventInFlight).Sum(m => m.Value), "in-flight returns to zero");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "EventHandlerFailure", "A throwing event handler is recorded as a dispatch error without affecting the decision",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            scope.Server.AuthorizationEvent += (sender, e) => throw new InvalidOperationException("handler failure");
                            using TelemetryCapture capture = new TelemetryCapture();

                            TestAssert.True(scope.Server.Authorize("alice", "read", "documents"), "decision unaffected");
                            capture.WaitFor(() => capture.Measurements(N.AuthorizationEventDispatches).Count > 0
                                && capture.Spans(N.SpanAuthorizationEvent).Count > 0, "event dispatch");

                            AssertMeasured(capture, N.AuthorizationEventDispatches, N.AttributeOutcome, N.OutcomeError,
                                N.AttributeErrorType, typeof(InvalidOperationException).FullName!);
                            AssertMeasured(capture, N.Errors, N.AttributeComponent, N.ComponentAuthorizationEvent);
                            TestAssert.Equal(ActivityStatusCode.Error, Single(capture.Spans(N.SpanAuthorizationEvent), "event span").Status, "event span status");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "BuildInfo", "The build info gauge reports 1 with the library version",
                        ct =>
                        {
                            using TelemetryCapture capture = new TelemetryCapture();
                            capture.RecordObservableInstruments();

                            CapturedMeasurement info = Single(capture.Measurements(N.BuildInfo), "build info");
                            TestAssert.Equal(1.0, info.Value, "build info value");
                            TestAssert.True(!string.IsNullOrEmpty(info.Tag(N.AttributeVersion)), "version label");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "NoListenerDoesNotThrow", "All instrumented paths work when nothing in this flow subscribes",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("alice", "read", "documents"), "authorize");
                            TestAssert.True(scope.Server.Users.All().Count == 3, "users all");
                            scope.Server.Users.RemoveByName("carol");
                            TestAssert.False(scope.Server.Users.ExistsByName("carol"), "removed");
                            return Task.CompletedTask;
                        }),

                    new TestCaseDescriptor(suiteId, "ThrowingListenersDoNotBreakLibrary", "Listeners that throw on every callback never break authorization or management",
                        ct =>
                        {
                            using GateKeeperScope scope = new GateKeeperScope();
                            using TelemetryCapture capture = new TelemetryCapture(throwFromListeners: true);
                            Activity? before = Activity.Current;

                            TestData.Standard(scope.Server);
                            TestAssert.True(scope.Server.Authorize("alice", "read", "documents"), "allow still works");
                            TestAssert.False(scope.Server.Authorize("alice", "delete", "documents"), "deny still works");
                            TestAssert.Throws<ArgumentException>(() => scope.Server.Users.Add(new User("alice")), "original exception surfaces");
                            TestAssert.True(Activity.Current == before, "Activity.Current restored");
                            return Task.CompletedTask;
                        }),
                });
        }

        #endregion

        #region Private-Methods

        private static T Single<T>(List<T> items, string what)
        {
            if (items.Count != 1)
                throw new InvalidOperationException("Expected exactly one " + what + " but found " + items.Count + ".");
            return items[0];
        }

        private static void AssertMeasured(TelemetryCapture capture, string instrument, params string[] pairs)
        {
            List<CapturedMeasurement> measurements = capture.Measurements(instrument);
            if (!measurements.Any(m => m.Has(pairs)))
            {
                string seen = string.Join("; ", measurements.Select(m => string.Join(",", m.Tags.Select(t => t.Key + "=" + t.Value))));
                throw new InvalidOperationException(
                    "No " + instrument + " measurement with [" + string.Join(",", pairs) + "]. Seen: " + (seen.Length > 0 ? seen : "(none)"));
            }
        }

        private static void BreakDatabase(string file)
        {
            // Overwrite the SQLite file with non-database bytes so every subsequent call fails.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.WriteAllBytes(file, System.Text.Encoding.ASCII.GetBytes(new string('x', 4096)));
        }

        #endregion
    }
}
