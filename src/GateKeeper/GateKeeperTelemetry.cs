namespace GateKeeper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;

    /// <summary>
    /// Internal emitter for GateKeeper metrics and spans. Emits only through the BCL
    /// Meter and ActivitySource, so it costs almost nothing when no listener subscribes.
    /// Every recording path is best-effort and never throws into the caller.
    /// </summary>
    internal static class GateKeeperTelemetry
    {
        #region Internal-Members

        internal static readonly string Version = ResolveVersion();

        internal static readonly ActivitySource Source = new ActivitySource(GateKeeperTelemetryNames.ActivitySourceName, Version);

        internal static readonly Meter Meter = new Meter(GateKeeperTelemetryNames.MeterName, Version);

        #endregion

        #region Private-Members

        private static readonly double[] _DurationBuckets = new double[]
        {
            0.0001, 0.00025, 0.0005, 0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10
        };

        private static readonly long[] _CountBuckets = new long[] { 0, 1, 2, 3, 5, 10, 25, 50, 100 };

        private static readonly Counter<long> _AuthorizationRequests = Meter.CreateCounter<long>(
            GateKeeperTelemetryNames.AuthorizationRequests, "{request}", "Authorization requests by decision and decision basis.");

        private static readonly Histogram<double> _AuthorizationDuration = CreateDuration(
            GateKeeperTelemetryNames.AuthorizationDuration, "End-to-end authorization duration.");

        private static readonly Histogram<double> _AuthorizationStageDuration = CreateDuration(
            GateKeeperTelemetryNames.AuthorizationStageDuration, "Authorization stage duration.");

        private static readonly Histogram<long> _AuthorizationMatchedEntries = Meter.CreateHistogram<long>(
            GateKeeperTelemetryNames.AuthorizationMatchedEntries, "{entry}", "Permission entries matched per authorization request.",
            null, new InstrumentAdvice<long> { HistogramBucketBoundaries = _CountBuckets });

        private static readonly Counter<long> _EventDispatches = Meter.CreateCounter<long>(
            GateKeeperTelemetryNames.AuthorizationEventDispatches, "{dispatch}", "Authorization event handler dispatches by outcome.");

        private static readonly Histogram<double> _EventDuration = CreateDuration(
            GateKeeperTelemetryNames.AuthorizationEventDuration, "Authorization event handler execution duration.");

        private static readonly Histogram<double> _EventQueueDuration = CreateDuration(
            GateKeeperTelemetryNames.AuthorizationEventQueueDuration, "Time an authorization event waited before its handlers ran.");

        private static readonly UpDownCounter<long> _EventInFlight = Meter.CreateUpDownCounter<long>(
            GateKeeperTelemetryNames.AuthorizationEventInFlight, "{dispatch}", "Authorization event dispatches queued or running.");

        private static readonly Counter<long> _ManagementOperations = Meter.CreateCounter<long>(
            GateKeeperTelemetryNames.ManagementOperations, "{operation}", "Top-level management operations by entity, operation, and outcome.");

        private static readonly Histogram<double> _ManagementDuration = CreateDuration(
            GateKeeperTelemetryNames.ManagementDuration, "Top-level management operation duration.");

        private static readonly Counter<long> _DbOperations = Meter.CreateCounter<long>(
            GateKeeperTelemetryNames.DbClientOperations, "{operation}", "Database client operations by operation, collection, and outcome.");

        private static readonly Histogram<double> _DbDuration = CreateDuration(
            GateKeeperTelemetryNames.DbClientOperationDuration, "Database client operation duration.");

        private static readonly Counter<long> _Errors = Meter.CreateCounter<long>(
            GateKeeperTelemetryNames.Errors, "{error}", "Errors by component and error type.");

        private static readonly Counter<long> _ServersCreated = Meter.CreateCounter<long>(
            GateKeeperTelemetryNames.ServersCreated, "{server}", "RbacServer instances constructed by outcome.");

        private static readonly Histogram<double> _InitializationDuration = CreateDuration(
            GateKeeperTelemetryNames.ServerInitializationDuration, "RbacServer database and schema initialization duration.");

        internal static readonly ObservableGauge<long> BuildInfo = Meter.CreateObservableGauge<long>(
            GateKeeperTelemetryNames.BuildInfo,
            () => new Measurement<long>(1, new KeyValuePair<string, object>(GateKeeperTelemetryNames.AttributeVersion, Version)),
            null,
            "GateKeeper library build information. Always 1.");

        [ThreadStatic]
        private static int _ManagementDepth;

        #endregion

        #region Internal-Methods

        internal static long Timestamp()
        {
            return Stopwatch.GetTimestamp();
        }

        internal static double ElapsedSeconds(long startTimestamp)
        {
            return (Stopwatch.GetTimestamp() - startTimestamp) / (double)Stopwatch.Frequency;
        }

        internal static Activity StartActivity(string name, ActivityKind kind)
        {
            if (!Source.HasListeners()) return null;

            Activity previous = Activity.Current;
            try
            {
                return Source.StartActivity(name, kind);
            }
            catch
            {
                // A throwing listener can leave a half-started span as Activity.Current. Restore it.
                RestoreCurrent(previous);
                return null;
            }
        }

        internal static void SetTag(Activity activity, string key, object value)
        {
            if (activity == null) return;
            try
            {
                activity.SetTag(key, value);
            }
            catch
            {
            }
        }

        internal static void MarkSuccess(Activity activity)
        {
            if (activity == null) return;
            try
            {
                activity.SetStatus(ActivityStatusCode.Ok);
            }
            catch
            {
            }
        }

        internal static void MarkError(Activity activity, Exception ex)
        {
            if (activity == null || ex == null) return;
            try
            {
                activity.SetTag(GateKeeperTelemetryNames.AttributeErrorType, ErrorType(ex));
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.AddException(ex);
            }
            catch
            {
            }
        }

        internal static void StopActivity(Activity activity)
        {
            if (activity == null) return;
            try
            {
                activity.Dispose();
            }
            catch
            {
                if (Activity.Current == activity) RestoreCurrent(activity.Parent);
            }
        }

        internal static string ErrorType(Exception ex)
        {
            if (ex == null) return null;
            return ex.GetType().FullName;
        }

        internal static void RecordError(string component, Exception ex)
        {
            try
            {
                if (!_Errors.Enabled) return;
                TagList tags = new TagList();
                tags.Add(GateKeeperTelemetryNames.AttributeComponent, component);
                tags.Add(GateKeeperTelemetryNames.AttributeErrorType, ErrorType(ex));
                _Errors.Add(1, tags);
            }
            catch
            {
            }
        }

        internal static void RecordAuthorization(string decision, string basis, double seconds, int? matchedEntries)
        {
            try
            {
                TagList tags = new TagList();
                tags.Add(GateKeeperTelemetryNames.AttributeDecision, decision);
                tags.Add(GateKeeperTelemetryNames.AttributeDecisionBasis, basis);
                if (_AuthorizationRequests.Enabled) _AuthorizationRequests.Add(1, tags);
                if (_AuthorizationDuration.Enabled) _AuthorizationDuration.Record(seconds, tags);
                if (matchedEntries.HasValue && _AuthorizationMatchedEntries.Enabled) _AuthorizationMatchedEntries.Record(matchedEntries.Value);
            }
            catch
            {
            }
        }

        internal static T RunAuthorizationStage<T>(string stage, Func<T> func)
        {
            Activity activity = StartActivity(GateKeeperTelemetryNames.SpanStagePrefix + stage, ActivityKind.Internal);
            long start = Timestamp();
            try
            {
                T result = func();
                RecordStage(stage, GateKeeperTelemetryNames.OutcomeSuccess, ElapsedSeconds(start));
                MarkSuccess(activity);
                return result;
            }
            catch (Exception ex)
            {
                RecordStage(stage, GateKeeperTelemetryNames.OutcomeError, ElapsedSeconds(start));
                MarkError(activity, ex);
                throw;
            }
            finally
            {
                StopActivity(activity);
            }
        }

        internal static void EventQueued()
        {
            try
            {
                if (_EventInFlight.Enabled) _EventInFlight.Add(1);
            }
            catch
            {
            }
        }

        internal static void EventStarted(long queuedTimestamp)
        {
            try
            {
                if (_EventQueueDuration.Enabled) _EventQueueDuration.Record(ElapsedSeconds(queuedTimestamp));
            }
            catch
            {
            }
        }

        internal static void EventCompleted(double seconds, Exception ex)
        {
            try
            {
                string outcome = ex == null ? GateKeeperTelemetryNames.OutcomeSuccess : GateKeeperTelemetryNames.OutcomeError;
                TagList tags = new TagList();
                tags.Add(GateKeeperTelemetryNames.AttributeOutcome, outcome);
                if (_EventDuration.Enabled) _EventDuration.Record(seconds, tags);
                if (ex != null) tags.Add(GateKeeperTelemetryNames.AttributeErrorType, ErrorType(ex));
                if (_EventDispatches.Enabled) _EventDispatches.Add(1, tags);
                if (_EventInFlight.Enabled) _EventInFlight.Add(-1);
            }
            catch
            {
            }

            if (ex != null) RecordError(GateKeeperTelemetryNames.ComponentAuthorizationEvent, ex);
        }

        internal static void RecordInitialization(double seconds, Exception ex)
        {
            try
            {
                TagList tags = new TagList();
                tags.Add(GateKeeperTelemetryNames.AttributeOutcome, ex == null ? GateKeeperTelemetryNames.OutcomeSuccess : GateKeeperTelemetryNames.OutcomeError);
                if (_InitializationDuration.Enabled) _InitializationDuration.Record(seconds, tags);
                if (_ServersCreated.Enabled) _ServersCreated.Add(1, tags);
            }
            catch
            {
            }

            if (ex != null) RecordError(GateKeeperTelemetryNames.ComponentInitialization, ex);
        }

        internal static T RunManagement<T>(string entity, string operation, Func<T> func)
        {
            Activity activity = StartActivity(GateKeeperTelemetryNames.SpanManagementPrefix + entity + "." + operation, ActivityKind.Internal);
            SetTag(activity, GateKeeperTelemetryNames.AttributeEntity, entity);
            SetTag(activity, GateKeeperTelemetryNames.AttributeManagementOperation, operation);

            bool topLevel = _ManagementDepth == 0;
            _ManagementDepth++;
            long start = Timestamp();
            try
            {
                T result = func();
                int? count = ResultCount(result);
                if (count.HasValue) SetTag(activity, GateKeeperTelemetryNames.AttributeResultCount, count.Value);
                if (topLevel) RecordManagement(entity, operation, ElapsedSeconds(start), null);
                MarkSuccess(activity);
                return result;
            }
            catch (Exception ex)
            {
                if (topLevel) RecordManagement(entity, operation, ElapsedSeconds(start), ex);
                MarkError(activity, ex);
                throw;
            }
            finally
            {
                _ManagementDepth--;
                StopActivity(activity);
            }
        }

        internal static void RunManagement(string entity, string operation, Action action)
        {
            RunManagement<object>(entity, operation, () =>
            {
                action();
                return null;
            });
        }

        internal static T RunDb<T>(string operation, string collection, Func<T> func)
        {
            Activity activity = StartActivity(GateKeeperTelemetryNames.SpanDbPrefix + operation + " " + collection, ActivityKind.Client);
            SetTag(activity, GateKeeperTelemetryNames.AttributeDbSystemName, GateKeeperTelemetryNames.DbSystemSqlite);
            SetTag(activity, GateKeeperTelemetryNames.AttributeDbOperationName, operation);
            SetTag(activity, GateKeeperTelemetryNames.AttributeDbCollectionName, collection);

            long start = Timestamp();
            try
            {
                T result = func();
                RecordDb(operation, collection, ElapsedSeconds(start), null);
                MarkSuccess(activity);
                return result;
            }
            catch (Exception ex)
            {
                RecordDb(operation, collection, ElapsedSeconds(start), ex);
                MarkError(activity, ex);
                throw;
            }
            finally
            {
                StopActivity(activity);
            }
        }

        internal static void RunDb(string operation, string collection, Action action)
        {
            RunDb<object>(operation, collection, () =>
            {
                action();
                return null;
            });
        }

        #endregion

        #region Private-Methods

        private static Histogram<double> CreateDuration(string name, string description)
        {
            return Meter.CreateHistogram<double>(name, "s", description, null,
                new InstrumentAdvice<double> { HistogramBucketBoundaries = _DurationBuckets });
        }

        private static void RecordStage(string stage, string outcome, double seconds)
        {
            try
            {
                if (!_AuthorizationStageDuration.Enabled) return;
                TagList tags = new TagList();
                tags.Add(GateKeeperTelemetryNames.AttributeStage, stage);
                tags.Add(GateKeeperTelemetryNames.AttributeOutcome, outcome);
                _AuthorizationStageDuration.Record(seconds, tags);
            }
            catch
            {
            }
        }

        private static void RecordManagement(string entity, string operation, double seconds, Exception ex)
        {
            try
            {
                TagList tags = new TagList();
                tags.Add(GateKeeperTelemetryNames.AttributeEntity, entity);
                tags.Add(GateKeeperTelemetryNames.AttributeManagementOperation, operation);
                tags.Add(GateKeeperTelemetryNames.AttributeOutcome, ex == null ? GateKeeperTelemetryNames.OutcomeSuccess : GateKeeperTelemetryNames.OutcomeError);
                if (_ManagementDuration.Enabled) _ManagementDuration.Record(seconds, tags);
                if (ex != null) tags.Add(GateKeeperTelemetryNames.AttributeErrorType, ErrorType(ex));
                if (_ManagementOperations.Enabled) _ManagementOperations.Add(1, tags);
            }
            catch
            {
            }

            if (ex != null) RecordError(GateKeeperTelemetryNames.ComponentManagement, ex);
        }

        private static void RecordDb(string operation, string collection, double seconds, Exception ex)
        {
            try
            {
                TagList tags = new TagList();
                tags.Add(GateKeeperTelemetryNames.AttributeDbSystemName, GateKeeperTelemetryNames.DbSystemSqlite);
                tags.Add(GateKeeperTelemetryNames.AttributeDbOperationName, operation);
                tags.Add(GateKeeperTelemetryNames.AttributeDbCollectionName, collection);
                tags.Add(GateKeeperTelemetryNames.AttributeOutcome, ex == null ? GateKeeperTelemetryNames.OutcomeSuccess : GateKeeperTelemetryNames.OutcomeError);
                if (_DbDuration.Enabled) _DbDuration.Record(seconds, tags);
                if (ex != null) tags.Add(GateKeeperTelemetryNames.AttributeErrorType, ErrorType(ex));
                if (_DbOperations.Enabled) _DbOperations.Add(1, tags);
            }
            catch
            {
            }

            if (ex != null) RecordError(GateKeeperTelemetryNames.ComponentDb, ex);
        }

        private static void RestoreCurrent(Activity activity)
        {
            try
            {
                Activity.Current = activity;
            }
            catch
            {
            }
        }

        private static int? ResultCount(object result)
        {
            if (result == null) return null;
            System.Collections.ICollection collection = result as System.Collections.ICollection;
            if (collection != null) return collection.Count;
            return null;
        }

        private static string ResolveVersion()
        {
            try
            {
                Version version = typeof(GateKeeperTelemetry).Assembly.GetName().Version;
                return version == null ? "0.0.0" : version.ToString(3);
            }
            catch
            {
                return "0.0.0";
            }
        }

        #endregion
    }
}
