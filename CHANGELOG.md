# Change Log

## Current Version

v2.2.1

- Updated test dependencies: Touchstone.Core/Cli/XunitAdapter/NunitAdapter 0.1.12 -> 0.2.0, NUnit 4.6.1 -> 5.0.0, NUnit3TestAdapter 6.2.0 -> 6.3.0, Microsoft.NET.Test.Sdk 18.9.0 -> 18.10.1
- No changes to the library's API or runtime dependencies (WatsonORM.Sqlite, SQLitePCLRaw.bundle_e_sqlite3, and System.Diagnostics.DiagnosticSource are already current); no known vulnerable packages

## Previous Versions

v2.2.0

- Added built-in observability. GateKeeper emits metrics and traces through the BCL `Meter` and `ActivitySource` named `GateKeeper`, with no OpenTelemetry, Radiant, or exporter dependency. See `TELEMETRY.md`.
  - Authorization: request counter and duration by decision (`allow`/`deny`/`error`) and basis (`matched`/`default`/`none`), per-stage duration (`build_query`, `query`, `evaluate`), matched-entry distribution
  - `AuthorizationEvent` dispatch: outcome counter, handler duration, queue wait, in-flight gauge, and a span that joins the `Authorize` trace across the thread-pool hand-off
  - Management operations (all five managers): top-level operation counter and duration by entity, operation, and outcome
  - SQLite client: operation counter and duration by operation, table, and outcome, plus `sqlite ...` client spans
  - Errors by component and `error.type`, server initialization metrics, and a `gatekeeper.build.info` gauge
  - All names are exposed as constants on `GateKeeperTelemetryNames`
- `Authorize` no longer writes the generated SQL query (which contains the username) to the console
- Added a `System.Diagnostics.DiagnosticSource` dependency for the netstandard2.0, netstandard2.1, and net8.0 targets (net10.0 uses the in-box assembly)
- Added telemetry tests covering every instrumented category and failure path

v2.1.1

- Updated dependencies and remediated a transitive SQLitePCLRaw vulnerability (GHSA-2m69-gcr7-jv3q) by pinning the SQLitePCLRaw native provider to the 3.x line

v2.0.0

- Breaking changes and major refactor
- Content sanitization on insert and authorization evaluation
- Event handler for authorization decisions including evaluation metadata
- Automatic cleanup of subordinate objects (for instance, deleting a user deletes any associated role maps)

v1.x 

- initial release, bugfixes
