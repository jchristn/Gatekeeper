# GateKeeper Telemetry

GateKeeper measures itself and hands the numbers to whatever your application already uses for observability. It emits metrics and traces through the .NET base class library: a `Meter` and an `ActivitySource`, both named **`GateKeeper`**. GateKeeper takes no dependency on OpenTelemetry, Radiant, or any exporter, never opens a network connection, and never needs configuration. When nothing subscribes, each instrumented call does a listener check and moves on.

**GateKeeper emits, your host collects.** This document lists everything GateKeeper emits and shows how to collect it.

## Contents

- [Answering the on-call questions](#answering-the-on-call-questions)
- [Subscribing](#subscribing)
- [Metrics catalog](#metrics-catalog)
- [Spans catalog](#spans-catalog)
- [Attribute values](#attribute-values)
- [Recommended PromQL](#recommended-promql)
- [Recommended alerts](#recommended-alerts)
- [Dashboard map](#dashboard-map)
- [Privacy and cardinality](#privacy-and-cardinality)
- [Cost when unobserved](#cost-when-unobserved)
- [What is not emitted](#what-is-not-emitted)

## Answering the on-call questions

| Question | Where to look |
| --- | --- |
| Is authorization slow? | `gatekeeper_authorization_duration_seconds` p95 |
| Which part of authorization is slow? | `gatekeeper_authorization_stage_duration_seconds` by `gatekeeper_stage` (`build_query`, `query`, `evaluate`) |
| Is the database the cause? | `gatekeeper_db_client_operation_duration_seconds` by `db_operation_name` / `db_collection_name`, plus the `sqlite ...` client spans |
| Are requests failing, and why? | `gatekeeper_errors_total` by `gatekeeper_component` and `error_type`; Error-status spans in Tempo |
| Is the allow/deny mix changing? | `gatekeeper_authorization_requests_total` by `gatekeeper_authorization_decision` |
| Are decisions falling through to `DefaultPermit`? | `gatekeeper_authorization_requests_total{gatekeeper_authorization_basis="default"}` |
| Is my audit/event handler failing or backing up? | `gatekeeper_authorization_event_dispatches_total{gatekeeper_outcome="error"}`, `gatekeeper_authorization_event_in_flight`, `gatekeeper_authorization_event_queue_duration_seconds` |
| Are admin changes failing? | `gatekeeper_management_operations_total{gatekeeper_outcome="error"}` by entity and operation |
| Which version is deployed? | `gatekeeper_build_info` |

## Subscribing

The two names are the whole contract. They are also available as constants: `GateKeeperTelemetryNames.MeterName` and `GateKeeperTelemetryNames.ActivitySourceName`.

### Radiant

```csharp
using Radiant;

RadiantSettings settings = new RadiantSettings("my-service");
settings.Sources.AddMeter("GateKeeper");
settings.Sources.AddActivitySource("GateKeeper");
// if the host also runs Watson:
settings.Sources.AddMeter("Watson");
settings.Sources.AddActivitySource("Watson");

using (RadiantHost host = RadiantHost.Start(settings))
{
    RbacServer server = new RbacServer("gatekeeper.db");
    // run the application
}
```

### OpenTelemetry SDK

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using MeterProvider meters = Sdk.CreateMeterProviderBuilder()
    .AddMeter("GateKeeper")
    .AddOtlpExporter()             // or .AddPrometheusExporter()
    .Build();

using TracerProvider tracer = Sdk.CreateTracerProviderBuilder()
    .AddSource("GateKeeper")
    .AddOtlpExporter()
    .Build();
```

### No SDK (in-process)

`System.Diagnostics.Metrics.MeterListener` and `System.Diagnostics.ActivityListener` work directly. See `src/Test.Shared/TelemetryCapture.cs` for a complete example.

### Configuration

GateKeeper has no telemetry settings. Turn collection on or off by subscribing or not subscribing to the names above. Sampling, export, histogram bucket overrides, and resource attributes (service name, environment) all belong to the host's collector. Histograms publish bucket *advice* (100 µs to 10 s for durations), which OpenTelemetry SDKs 1.10 and later honor. A host can override it with a view.

### Trace context

GateKeeper spans start from `Activity.Current`. When `Authorize` runs inside a Watson request handler or any other traced operation, the `GateKeeper Authorize` span nests under it, and the whole request appears as one trace. The `AuthorizationEvent` handlers run on the thread pool. The ambient context flows into that background hand-off, so the `GateKeeper AuthorizationEvent` span is a child of the `GateKeeper Authorize` span that raised it.

## Metrics catalog

Instrument names are dotted. A Prometheus exporter rewrites them to snake case (shown in the last column) and converts dots in label keys to underscores.

| Instrument | Type | Unit | Labels | Description | Prometheus series |
| --- | --- | --- | --- | --- | --- |
| `gatekeeper.authorization.requests` | Counter | `{request}` | `gatekeeper.authorization.decision`, `gatekeeper.authorization.basis` | Authorization requests | `gatekeeper_authorization_requests_total` |
| `gatekeeper.authorization.duration` | Histogram | `s` | `gatekeeper.authorization.decision`, `gatekeeper.authorization.basis` | End-to-end `Authorize` duration | `gatekeeper_authorization_duration_seconds_*` |
| `gatekeeper.authorization.stage.duration` | Histogram | `s` | `gatekeeper.stage`, `gatekeeper.outcome` | Per-stage authorization duration | `gatekeeper_authorization_stage_duration_seconds_*` |
| `gatekeeper.authorization.matched_entries` | Histogram | `{entry}` | none | Permission entries matched per request (successful requests only) | `gatekeeper_authorization_matched_entries_*` |
| `gatekeeper.authorization_event.dispatches` | Counter | `{dispatch}` | `gatekeeper.outcome`, `error.type` (errors only) | `AuthorizationEvent` dispatches (only when a handler is attached) | `gatekeeper_authorization_event_dispatches_total` |
| `gatekeeper.authorization_event.duration` | Histogram | `s` | `gatekeeper.outcome` | Handler execution time | `gatekeeper_authorization_event_duration_seconds_*` |
| `gatekeeper.authorization_event.queue.duration` | Histogram | `s` | none | Time an event waited for a thread-pool thread (the `queued` stage) | `gatekeeper_authorization_event_queue_duration_seconds_*` |
| `gatekeeper.authorization_event.in_flight` | UpDownCounter | `{dispatch}` | none | Event dispatches queued or running | `gatekeeper_authorization_event_in_flight` |
| `gatekeeper.management.operations` | Counter | `{operation}` | `gatekeeper.entity`, `gatekeeper.management.operation`, `gatekeeper.outcome`, `error.type` (errors only) | Top-level manager calls | `gatekeeper_management_operations_total` |
| `gatekeeper.management.duration` | Histogram | `s` | `gatekeeper.entity`, `gatekeeper.management.operation`, `gatekeeper.outcome` | Top-level manager call duration | `gatekeeper_management_duration_seconds_*` |
| `gatekeeper.db.client.operations` | Counter | `{operation}` | `db.system.name`, `db.operation.name`, `db.collection.name`, `gatekeeper.outcome`, `error.type` (errors only) | SQLite calls | `gatekeeper_db_client_operations_total` |
| `gatekeeper.db.client.operation.duration` | Histogram | `s` | `db.system.name`, `db.operation.name`, `db.collection.name`, `gatekeeper.outcome` | SQLite call duration | `gatekeeper_db_client_operation_duration_seconds_*` |
| `gatekeeper.errors` | Counter | `{error}` | `gatekeeper.component`, `error.type` | Errors by component | `gatekeeper_errors_total` |
| `gatekeeper.servers.created` | Counter | `{server}` | `gatekeeper.outcome` | `RbacServer` constructions | `gatekeeper_servers_created_total` |
| `gatekeeper.server.initialization.duration` | Histogram | `s` | `gatekeeper.outcome` | Database open plus schema initialization | `gatekeeper_server_initialization_duration_seconds_*` |
| `gatekeeper.build.info` | ObservableGauge | none | `gatekeeper.version` | Always `1`; carries the library version | `gatekeeper_build_info` |

Notes:

- **Top-level only.** Managers call each other internally. For example, `Users.Add` calls `Users.ExistsByName`, and `Users.Remove` calls `UserRoles.RemoveUserRolesByUser`. The management metrics count only the call your code made. The nested calls still appear as child spans, and each SQLite call they make is still counted in the `db.client` metrics.
- **Errors are counted once per component.** A SQLite failure inside `Users.All()` increments `gatekeeper.errors` with `component=db` and again with `component=management`, because both layers saw it fail. Filter by component when summing.
- **Runtime metrics** (GC, thread pool, allocations) are not GateKeeper's to emit. Subscribe the host to the .NET runtime meter (`System.Runtime` on .NET 9+) or use Radiant's runtime metrics.

## Spans catalog

| Span name | Kind | Parent | Attributes | Status |
| --- | --- | --- | --- | --- |
| `GateKeeper Initialize` | Internal | caller | `error.type` on failure | Ok / Error |
| `GateKeeper Authorize` | Internal | caller (for example, Watson's request span) | `gatekeeper.authorization.operation`, `gatekeeper.authorization.resource`, `gatekeeper.default_permit`, `gatekeeper.authorization.decision`, `gatekeeper.authorization.basis`, `gatekeeper.authorization.matched_entries`, `error.type` | Ok for allow **and** deny; Error only when the call throws |
| `stage:build_query` | Internal | `GateKeeper Authorize` | `error.type` on failure | Ok / Error |
| `stage:query` | Internal | `GateKeeper Authorize` | `error.type` on failure | Ok / Error |
| `stage:evaluate` | Internal | `GateKeeper Authorize` | `error.type` on failure | Ok / Error |
| `GateKeeper AuthorizationEvent` | Internal | `GateKeeper Authorize` (across the thread-pool hand-off) | `error.type` on failure | Ok / Error |
| `GateKeeper {entity}.{operation}` (for example, `GateKeeper user.add`) | Internal | caller, or the outer manager call | `gatekeeper.entity`, `gatekeeper.management.operation`, `gatekeeper.result.count` (list reads), `error.type` | Ok / Error |
| `sqlite {db.operation.name} {db.collection.name}` (for example, `sqlite SELECT authorization`) | Client | the stage or manager span | `db.system.name`, `db.operation.name`, `db.collection.name`, `error.type` | Ok / Error |

Failed spans carry an `exception` event with `exception.type`, `exception.message`, and `exception.stacktrace`.

Example `Authorize` waterfall:

```
GET /documents/{id}                              (Watson server span)
└─ GateKeeper Authorize                          decision=allow basis=matched
   ├─ stage:build_query
   ├─ stage:query
   │  └─ sqlite SELECT authorization             (Client)
   ├─ stage:evaluate
   └─ GateKeeper AuthorizationEvent              (thread pool, only if a handler is attached)
```

## Attribute values

All metric label values come from fixed sets:

| Key | Values |
| --- | --- |
| `gatekeeper.authorization.decision` | `allow`, `deny`, `error` |
| `gatekeeper.authorization.basis` | `matched` (one or more entries matched), `default` (no match, `DefaultPermit` decided), `none` (no decision because of an error) |
| `gatekeeper.stage` | `build_query`, `query`, `evaluate` |
| `gatekeeper.outcome` | `success`, `error` |
| `gatekeeper.entity` | `user`, `role`, `resource`, `permission`, `user_role` |
| `gatekeeper.management.operation` | `add`, `remove`, `remove_by_name`, `remove_by_resource`, `remove_by_user`, `remove_by_role`, `all`, `get_by_name`, `get_by_resource`, `get_by_user`, `get_by_role`, `get_by_user_role`, `exists_by_name`, `exists` |
| `gatekeeper.component` | `authorization`, `authorization_event`, `management`, `db`, `initialization` |
| `db.system.name` | `sqlite` |
| `db.operation.name` | `SELECT`, `INSERT`, `DELETE`, `EXISTS`, `INITIALIZE` |
| `db.collection.name` | `users`, `roles`, `resources`, `permissions`, `userroles`, `authorization` (the five-table join used by `Authorize`), `schema` (initialization) |
| `error.type` | Full .NET exception type name, for example `System.ArgumentNullException` or `Microsoft.Data.Sqlite.SqliteException` |
| `gatekeeper.version` | Library version, for example `2.2.0` |

## Recommended PromQL

```promql
# Authorization rate by decision
sum by (gatekeeper_authorization_decision) (rate(gatekeeper_authorization_requests_total[5m]))

# Authorization p95 / p99
histogram_quantile(0.95, sum by (le) (rate(gatekeeper_authorization_duration_seconds_bucket[5m])))
histogram_quantile(0.99, sum by (le) (rate(gatekeeper_authorization_duration_seconds_bucket[5m])))

# Where authorization time goes: p95 per stage
histogram_quantile(0.95, sum by (le, gatekeeper_stage) (rate(gatekeeper_authorization_stage_duration_seconds_bucket[5m])))

# Authorization error ratio
sum(rate(gatekeeper_authorization_requests_total{gatekeeper_authorization_decision="error"}[5m]))
  / sum(rate(gatekeeper_authorization_requests_total[5m]))

# Share of decisions made by DefaultPermit (no matching grant)
sum(rate(gatekeeper_authorization_requests_total{gatekeeper_authorization_basis="default"}[5m]))
  / sum(rate(gatekeeper_authorization_requests_total{gatekeeper_authorization_decision!="error"}[5m]))

# SQLite p95 by operation and table
histogram_quantile(0.95, sum by (le, db_operation_name, db_collection_name) (rate(gatekeeper_db_client_operation_duration_seconds_bucket[5m])))

# Errors by component and type
sum by (gatekeeper_component, error_type) (rate(gatekeeper_errors_total[5m]))

# Event handler backlog and failures
gatekeeper_authorization_event_in_flight
sum(rate(gatekeeper_authorization_event_dispatches_total{gatekeeper_outcome="error"}[5m]))
```

## Recommended alerts

```yaml
groups:
  - name: gatekeeper
    rules:
      - alert: GateKeeperAuthorizationErrors
        expr: |
          sum(rate(gatekeeper_authorization_requests_total{gatekeeper_authorization_decision="error"}[5m]))
            / clamp_min(sum(rate(gatekeeper_authorization_requests_total[5m])), 1e-9) > 0.01
        for: 5m
        labels: { severity: page }
        annotations:
          summary: "More than 1% of GateKeeper authorization calls are failing"

      - alert: GateKeeperAuthorizationSlow
        expr: |
          histogram_quantile(0.95, sum by (le) (rate(gatekeeper_authorization_duration_seconds_bucket[5m]))) > 0.05
        for: 10m
        labels: { severity: warn }
        annotations:
          summary: "GateKeeper authorization p95 above 50 ms (check the stage and sqlite panels)"

      - alert: GateKeeperDatabaseErrors
        expr: sum(rate(gatekeeper_db_client_operations_total{gatekeeper_outcome="error"}[5m])) > 0
        for: 5m
        labels: { severity: page }
        annotations:
          summary: "GateKeeper SQLite calls are failing"

      - alert: GateKeeperEventHandlerFailing
        expr: sum(rate(gatekeeper_authorization_event_dispatches_total{gatekeeper_outcome="error"}[5m])) > 0
        for: 5m
        labels: { severity: warn }
        annotations:
          summary: "An AuthorizationEvent handler (for example, audit logging) is throwing"

      - alert: GateKeeperEventBacklog
        expr: max(gatekeeper_authorization_event_in_flight) > 1000
        for: 5m
        labels: { severity: warn }
        annotations:
          summary: "AuthorizationEvent dispatches are backing up on the thread pool"

      - alert: GateKeeperInitializationFailed
        expr: sum(increase(gatekeeper_servers_created_total{gatekeeper_outcome="error"}[15m])) > 0
        labels: { severity: page }
        annotations:
          summary: "An RbacServer failed to initialize its database"
```

Adjust the thresholds to your workload. A local SQLite authorization query normally finishes in well under a millisecond.

## Dashboard map

GateKeeper is a library and ships no compose stack or dashboards of its own. The host service owns those. Add an **Authorization** dashboard to the host's product folder in Grafana with these rows:

| Row | Panels |
| --- | --- |
| Decisions | Request rate by decision; DefaultPermit share; error ratio |
| Latency | Authorization p50/p95/p99; p95 per stage; matched entries distribution |
| Database | SQLite rate and p95 by operation/table; SQLite error rate by `error_type` |
| Events | Dispatch rate by outcome; in-flight; queue p95; handler p95 |
| Administration | Management rate by entity/operation; management error rate |
| Errors | `gatekeeper_errors_total` by component and `error_type` |
| Build | `gatekeeper_build_info` by version |

From a slow request on the host's HTTP dashboard, open its trace in Tempo. The `GateKeeper Authorize` span and its `stage:` and `sqlite` children show where the authorization time went.

## Privacy and cardinality

- **No usernames.** The username passed to `Authorize`, and user names in general, never appear on any span or metric. The SQL text of the authorization query is not recorded because it contains the username.
- **Request operation and resource are on spans only.** `gatekeeper.authorization.operation` and `gatekeeper.authorization.resource` describe the request and are recorded on the `Authorize` span, never as metric labels, because an application can use unbounded resource names.
- **No entity names, GUIDs, or metadata** are recorded. The `metadata` object passed to `Authorize` is never inspected.
- Every metric label comes from the fixed sets above, so series counts stay bounded no matter how many users, roles, or resources exist.

## Cost when unobserved

Each instrumented method checks `ActivitySource.HasListeners()` and `Instrument.Enabled` before doing any work. When nothing subscribes, no spans are created and no measurements are recorded. The remaining overhead is one `Stopwatch` timestamp pair and a delegate allocation per call. That is negligible next to the SQLite I/O it surrounds. Every recording path is wrapped so that a failing or throwing listener can never change a decision or replace the original exception. This is covered by the `ThrowingListenersDoNotBreakLibrary` test.

## What is not emitted

- **Configuration gauges.** `DefaultPermit` is a per-instance field, so it is recorded on each `Authorize` span (`gatekeeper.default_permit`), and its effect shows up in the `basis="default"` series. It is not a process-wide gauge.
- **Entity counts** (number of users, roles, and so on). Reporting them would mean running database queries inside a metrics callback. Use `management.operations` add/remove rates instead.
- **Logs.** GateKeeper is a library with no background work of its own beyond the event hand-off, which is already covered by metrics and spans. It writes no logs.
