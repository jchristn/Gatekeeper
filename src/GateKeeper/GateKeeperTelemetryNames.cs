namespace GateKeeper
{
    /// <summary>
    /// Stable public names for every GateKeeper telemetry point: the meter and activity source,
    /// each metric instrument, each span, every attribute key, and the bounded attribute values.
    /// These names are a public contract consumed by dashboards and alerts. Do not rename them
    /// outside of a major version.
    /// Thread safety: all members are constants and are safe for concurrent use.
    /// </summary>
    public static class GateKeeperTelemetryNames
    {
        #region Sources

        /// <summary>
        /// Name of the <see cref="System.Diagnostics.Metrics.Meter"/> that emits all GateKeeper metrics.
        /// </summary>
        public const string MeterName = "GateKeeper";

        /// <summary>
        /// Name of the <see cref="System.Diagnostics.ActivitySource"/> that emits all GateKeeper spans.
        /// </summary>
        public const string ActivitySourceName = "GateKeeper";

        #endregion

        #region Metrics

        /// <summary>
        /// Counter of authorization requests, labeled by decision and decision basis.
        /// </summary>
        public const string AuthorizationRequests = "gatekeeper.authorization.requests";

        /// <summary>
        /// Histogram of end-to-end authorization duration in seconds, labeled by decision and decision basis.
        /// </summary>
        public const string AuthorizationDuration = "gatekeeper.authorization.duration";

        /// <summary>
        /// Histogram of authorization stage duration in seconds, labeled by stage and outcome.
        /// </summary>
        public const string AuthorizationStageDuration = "gatekeeper.authorization.stage.duration";

        /// <summary>
        /// Histogram of the number of permission entries that matched an authorization request.
        /// </summary>
        public const string AuthorizationMatchedEntries = "gatekeeper.authorization.matched_entries";

        /// <summary>
        /// Counter of authorization event handler dispatches, labeled by outcome and error type.
        /// </summary>
        public const string AuthorizationEventDispatches = "gatekeeper.authorization_event.dispatches";

        /// <summary>
        /// Histogram of authorization event handler execution duration in seconds, labeled by outcome.
        /// </summary>
        public const string AuthorizationEventDuration = "gatekeeper.authorization_event.duration";

        /// <summary>
        /// Histogram of time in seconds an authorization event waited on the thread pool before its handlers ran.
        /// </summary>
        public const string AuthorizationEventQueueDuration = "gatekeeper.authorization_event.queue.duration";

        /// <summary>
        /// Up/down counter of authorization event dispatches queued or running.
        /// </summary>
        public const string AuthorizationEventInFlight = "gatekeeper.authorization_event.in_flight";

        /// <summary>
        /// Counter of top-level management operations, labeled by entity, operation, outcome, and error type.
        /// </summary>
        public const string ManagementOperations = "gatekeeper.management.operations";

        /// <summary>
        /// Histogram of top-level management operation duration in seconds, labeled by entity, operation, and outcome.
        /// </summary>
        public const string ManagementDuration = "gatekeeper.management.duration";

        /// <summary>
        /// Counter of database client operations, labeled by system, operation, collection, outcome, and error type.
        /// </summary>
        public const string DbClientOperations = "gatekeeper.db.client.operations";

        /// <summary>
        /// Histogram of database client operation duration in seconds, labeled by system, operation, collection, and outcome.
        /// </summary>
        public const string DbClientOperationDuration = "gatekeeper.db.client.operation.duration";

        /// <summary>
        /// Counter of errors, labeled by component and error type.
        /// </summary>
        public const string Errors = "gatekeeper.errors";

        /// <summary>
        /// Counter of RbacServer instances constructed, labeled by outcome.
        /// </summary>
        public const string ServersCreated = "gatekeeper.servers.created";

        /// <summary>
        /// Histogram of RbacServer initialization (database and schema setup) duration in seconds, labeled by outcome.
        /// </summary>
        public const string ServerInitializationDuration = "gatekeeper.server.initialization.duration";

        /// <summary>
        /// Gauge that always reports 1, labeled with the GateKeeper library version.
        /// </summary>
        public const string BuildInfo = "gatekeeper.build.info";

        #endregion

        #region Spans

        /// <summary>
        /// Root span for RbacServer.Authorize.
        /// </summary>
        public const string SpanAuthorize = "GateKeeper Authorize";

        /// <summary>
        /// Span prefix for authorization stages. The full name is this prefix followed by the stage value.
        /// </summary>
        public const string SpanStagePrefix = "stage:";

        /// <summary>
        /// Span for the asynchronous authorization event dispatch.
        /// </summary>
        public const string SpanAuthorizationEvent = "GateKeeper AuthorizationEvent";

        /// <summary>
        /// Span for RbacServer construction (database and schema initialization).
        /// </summary>
        public const string SpanInitialize = "GateKeeper Initialize";

        /// <summary>
        /// Span prefix for management operations. The full name is "GateKeeper {entity}.{operation}".
        /// </summary>
        public const string SpanManagementPrefix = "GateKeeper ";

        /// <summary>
        /// Span prefix for database client calls. The full name is "sqlite {operation} {collection}".
        /// </summary>
        public const string SpanDbPrefix = "sqlite ";

        #endregion

        #region Attribute-Keys

        /// <summary>
        /// Authorization decision. Values: allow, deny, error.
        /// </summary>
        public const string AttributeDecision = "gatekeeper.authorization.decision";

        /// <summary>
        /// What the authorization decision was based on. Values: matched, default, none.
        /// </summary>
        public const string AttributeDecisionBasis = "gatekeeper.authorization.basis";

        /// <summary>
        /// Authorization request operation (span attribute only, never a metric label).
        /// </summary>
        public const string AttributeAuthorizationOperation = "gatekeeper.authorization.operation";

        /// <summary>
        /// Authorization request resource (span attribute only, never a metric label).
        /// </summary>
        public const string AttributeAuthorizationResource = "gatekeeper.authorization.resource";

        /// <summary>
        /// Number of matching permission entries (span attribute).
        /// </summary>
        public const string AttributeMatchedEntries = "gatekeeper.authorization.matched_entries";

        /// <summary>
        /// Effective DefaultPermit setting at the time of the request (span attribute).
        /// </summary>
        public const string AttributeDefaultPermit = "gatekeeper.default_permit";

        /// <summary>
        /// Authorization stage name.
        /// </summary>
        public const string AttributeStage = "gatekeeper.stage";

        /// <summary>
        /// Operation outcome. Values: success, error.
        /// </summary>
        public const string AttributeOutcome = "gatekeeper.outcome";

        /// <summary>
        /// Management entity. Values: user, role, resource, permission, user_role.
        /// </summary>
        public const string AttributeEntity = "gatekeeper.entity";

        /// <summary>
        /// Management operation name, for example add, remove, all, get_by_name.
        /// </summary>
        public const string AttributeManagementOperation = "gatekeeper.management.operation";

        /// <summary>
        /// Number of records returned by a management read (span attribute).
        /// </summary>
        public const string AttributeResultCount = "gatekeeper.result.count";

        /// <summary>
        /// Component that raised an error. Values: authorization, authorization_event, management, db, initialization.
        /// </summary>
        public const string AttributeComponent = "gatekeeper.component";

        /// <summary>
        /// GateKeeper library version.
        /// </summary>
        public const string AttributeVersion = "gatekeeper.version";

        /// <summary>
        /// OpenTelemetry error type (exception type name).
        /// </summary>
        public const string AttributeErrorType = "error.type";

        /// <summary>
        /// OpenTelemetry database system name.
        /// </summary>
        public const string AttributeDbSystemName = "db.system.name";

        /// <summary>
        /// OpenTelemetry database operation name.
        /// </summary>
        public const string AttributeDbOperationName = "db.operation.name";

        /// <summary>
        /// OpenTelemetry database collection (table) name.
        /// </summary>
        public const string AttributeDbCollectionName = "db.collection.name";

        #endregion

        #region Attribute-Values

        /// <summary>
        /// Decision value: authorized.
        /// </summary>
        public const string DecisionAllow = "allow";

        /// <summary>
        /// Decision value: not authorized.
        /// </summary>
        public const string DecisionDeny = "deny";

        /// <summary>
        /// Decision value: the request failed before a decision was reached.
        /// </summary>
        public const string DecisionError = "error";

        /// <summary>
        /// Basis value: one or more permission entries matched.
        /// </summary>
        public const string BasisMatched = "matched";

        /// <summary>
        /// Basis value: no entries matched, so DefaultPermit decided.
        /// </summary>
        public const string BasisDefault = "default";

        /// <summary>
        /// Basis value: no decision was reached.
        /// </summary>
        public const string BasisNone = "none";

        /// <summary>
        /// Outcome value: success.
        /// </summary>
        public const string OutcomeSuccess = "success";

        /// <summary>
        /// Outcome value: error.
        /// </summary>
        public const string OutcomeError = "error";

        /// <summary>
        /// Stage value: input sanitization and query construction.
        /// </summary>
        public const string StageBuildQuery = "build_query";

        /// <summary>
        /// Stage value: database query execution.
        /// </summary>
        public const string StageQuery = "query";

        /// <summary>
        /// Stage value: result mapping and decision evaluation.
        /// </summary>
        public const string StageEvaluate = "evaluate";

        /// <summary>
        /// Component value: authorization.
        /// </summary>
        public const string ComponentAuthorization = "authorization";

        /// <summary>
        /// Component value: authorization event dispatch.
        /// </summary>
        public const string ComponentAuthorizationEvent = "authorization_event";

        /// <summary>
        /// Component value: management operations.
        /// </summary>
        public const string ComponentManagement = "management";

        /// <summary>
        /// Component value: database client.
        /// </summary>
        public const string ComponentDb = "db";

        /// <summary>
        /// Component value: server initialization.
        /// </summary>
        public const string ComponentInitialization = "initialization";

        /// <summary>
        /// Entity value: user.
        /// </summary>
        public const string EntityUser = "user";

        /// <summary>
        /// Entity value: role.
        /// </summary>
        public const string EntityRole = "role";

        /// <summary>
        /// Entity value: resource.
        /// </summary>
        public const string EntityResource = "resource";

        /// <summary>
        /// Entity value: permission.
        /// </summary>
        public const string EntityPermission = "permission";

        /// <summary>
        /// Entity value: user-to-role mapping.
        /// </summary>
        public const string EntityUserRole = "user_role";

        /// <summary>
        /// Management operation value: add.
        /// </summary>
        public const string OperationAdd = "add";

        /// <summary>
        /// Management operation value: remove.
        /// </summary>
        public const string OperationRemove = "remove";

        /// <summary>
        /// Management operation value: remove by name.
        /// </summary>
        public const string OperationRemoveByName = "remove_by_name";

        /// <summary>
        /// Management operation value: remove all permissions for a resource.
        /// </summary>
        public const string OperationRemoveByResource = "remove_by_resource";

        /// <summary>
        /// Management operation value: remove all mappings for a user.
        /// </summary>
        public const string OperationRemoveByUser = "remove_by_user";

        /// <summary>
        /// Management operation value: remove all mappings for a role.
        /// </summary>
        public const string OperationRemoveByRole = "remove_by_role";

        /// <summary>
        /// Management operation value: retrieve all.
        /// </summary>
        public const string OperationAll = "all";

        /// <summary>
        /// Management operation value: get first by name.
        /// </summary>
        public const string OperationGetByName = "get_by_name";

        /// <summary>
        /// Management operation value: get by resource.
        /// </summary>
        public const string OperationGetByResource = "get_by_resource";

        /// <summary>
        /// Management operation value: get by user.
        /// </summary>
        public const string OperationGetByUser = "get_by_user";

        /// <summary>
        /// Management operation value: get by role.
        /// </summary>
        public const string OperationGetByRole = "get_by_role";

        /// <summary>
        /// Management operation value: get by user and role.
        /// </summary>
        public const string OperationGetByUserRole = "get_by_user_role";

        /// <summary>
        /// Management operation value: existence check by name.
        /// </summary>
        public const string OperationExistsByName = "exists_by_name";

        /// <summary>
        /// Management operation value: existence check by user and role.
        /// </summary>
        public const string OperationExists = "exists";

        /// <summary>
        /// Database system value.
        /// </summary>
        public const string DbSystemSqlite = "sqlite";

        /// <summary>
        /// Database operation value: select rows.
        /// </summary>
        public const string DbOperationSelect = "SELECT";

        /// <summary>
        /// Database operation value: insert a row.
        /// </summary>
        public const string DbOperationInsert = "INSERT";

        /// <summary>
        /// Database operation value: delete one or more rows.
        /// </summary>
        public const string DbOperationDelete = "DELETE";

        /// <summary>
        /// Database operation value: existence check.
        /// </summary>
        public const string DbOperationExists = "EXISTS";

        /// <summary>
        /// Database operation value: database and schema initialization.
        /// </summary>
        public const string DbOperationInitialize = "INITIALIZE";

        /// <summary>
        /// Database collection value: users table.
        /// </summary>
        public const string DbCollectionUsers = "users";

        /// <summary>
        /// Database collection value: roles table.
        /// </summary>
        public const string DbCollectionRoles = "roles";

        /// <summary>
        /// Database collection value: resources table.
        /// </summary>
        public const string DbCollectionResources = "resources";

        /// <summary>
        /// Database collection value: permissions table.
        /// </summary>
        public const string DbCollectionPermissions = "permissions";

        /// <summary>
        /// Database collection value: userroles table.
        /// </summary>
        public const string DbCollectionUserRoles = "userroles";

        /// <summary>
        /// Database collection value: the five-table join used by Authorize.
        /// </summary>
        public const string DbCollectionAuthorization = "authorization";

        /// <summary>
        /// Database collection value: database schema (initialization).
        /// </summary>
        public const string DbCollectionSchema = "schema";

        #endregion
    }
}
