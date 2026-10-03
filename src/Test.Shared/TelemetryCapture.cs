namespace GateKeeper.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Threading;

    using GateKeeper;

    /// <summary>
    /// In-memory listener for the GateKeeper meter and activity source, scoped to one test.
    /// The constructor starts a root test span; only spans and measurements recorded inside
    /// that trace are captured, so captures stay isolated while xUnit and NUnit run cases in parallel.
    /// Create, use, and dispose a capture on the same synchronous flow.
    /// </summary>
    public sealed class TelemetryCapture : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Name of the activity source used for the root test span.
        /// </summary>
        public const string TestSourceName = "GateKeeper.Tests";

        /// <summary>
        /// Activity source used for the root test span.
        /// </summary>
        public static readonly ActivitySource TestSource = new ActivitySource(TestSourceName);

        /// <summary>
        /// Root test span. Every captured span belongs to its trace.
        /// </summary>
        public Activity Root
        {
            get { return _Root; }
        }

        #endregion

        #region Private-Members

        private readonly object _Lock = new object();
        private readonly List<Activity> _Activities = new List<Activity>();
        private readonly List<CapturedMeasurement> _Measurements = new List<CapturedMeasurement>();
        private readonly ActivityListener _ActivityListener;
        private readonly MeterListener _MeterListener;
        private readonly Activity _Root;
        private readonly ActivityTraceId _TraceId;
        private readonly bool _ThrowFromListeners;
        private bool _Disposed;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Start capturing.
        /// </summary>
        /// <param name="throwFromListeners">When true, every listener callback for this trace throws instead of recording, to prove instrumentation never breaks the library.</param>
        /// <exception cref="InvalidOperationException">Thrown when the root span cannot be started.</exception>
        public TelemetryCapture(bool throwFromListeners = false)
        {
            _ThrowFromListeners = throwFromListeners;

            _ActivityListener = new ActivityListener
            {
                ShouldListenTo = source =>
                    source.Name == GateKeeperTelemetryNames.ActivitySourceName || source.Name == TestSourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = activity =>
                {
                    if (_ThrowFromListeners && IsMine(activity)) throw new InvalidOperationException("listener failure (start)");
                },
                ActivityStopped = activity =>
                {
                    if (!IsMine(activity)) return;
                    if (_ThrowFromListeners) throw new InvalidOperationException("listener failure (stop)");
                    lock (_Lock) _Activities.Add(activity);
                }
            };
            ActivitySource.AddActivityListener(_ActivityListener);

            _Root = TestSource.StartActivity("test") ?? throw new InvalidOperationException("Unable to start the root test span.");
            _TraceId = _Root.TraceId;

            _MeterListener = new MeterListener();
            _MeterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == GateKeeperTelemetryNames.MeterName) listener.EnableMeasurementEvents(instrument);
            };
            _MeterListener.SetMeasurementEventCallback<long>((instrument, value, tags, state) => OnMeasurement(instrument, value, tags));
            _MeterListener.SetMeasurementEventCallback<double>((instrument, value, tags, state) => OnMeasurement(instrument, value, tags));
            _MeterListener.Start();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Snapshot of captured spans (excluding the root).
        /// </summary>
        /// <returns>Spans.</returns>
        public List<Activity> Spans()
        {
            lock (_Lock) return _Activities.Where(a => a != _Root).ToList();
        }

        /// <summary>
        /// Captured spans with the given name.
        /// </summary>
        /// <param name="name">Span name.</param>
        /// <returns>Spans.</returns>
        public List<Activity> Spans(string name)
        {
            return Spans().Where(a => a.DisplayName == name).ToList();
        }

        /// <summary>
        /// Captured measurements for the given instrument.
        /// </summary>
        /// <param name="name">Instrument name.</param>
        /// <returns>Measurements.</returns>
        public List<CapturedMeasurement> Measurements(string name)
        {
            lock (_Lock) return _Measurements.Where(m => m.Name == name).ToList();
        }

        /// <summary>
        /// Every captured measurement.
        /// </summary>
        /// <returns>Measurements.</returns>
        public List<CapturedMeasurement> AllMeasurements()
        {
            lock (_Lock) return _Measurements.ToList();
        }

        /// <summary>
        /// Trigger observable instruments (gauges) to report.
        /// </summary>
        public void RecordObservableInstruments()
        {
            _MeterListener.RecordObservableInstruments();
        }

        /// <summary>
        /// Poll until the condition holds or the timeout elapses.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="description">Description used in the failure message.</param>
        /// <param name="timeoutMs">Timeout in milliseconds. Default 10000.</param>
        /// <exception cref="TimeoutException">Thrown when the condition is not met in time.</exception>
        public void WaitFor(Func<bool> condition, string description, int timeoutMs = 10000)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (condition()) return;
                Thread.Sleep(10);
            }

            throw new TimeoutException("Timed out waiting for: " + description);
        }

        /// <summary>
        /// Stop the root span and both listeners.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            try { _Root.Dispose(); } catch { }
            _MeterListener.Dispose();
            _ActivityListener.Dispose();
        }

        #endregion

        #region Private-Methods

        private bool IsMine(Activity activity)
        {
            return _Root != null && activity.TraceId == _TraceId;
        }

        private void OnMeasurement<T>(Instrument instrument, T value, ReadOnlySpan<KeyValuePair<string, object?>> tags) where T : struct
        {
            Activity? current = Activity.Current;
            if (current == null || _Root == null || current.TraceId != _TraceId) return;
            if (_ThrowFromListeners) throw new InvalidOperationException("listener failure (measurement)");

            Dictionary<string, object?> copy = new Dictionary<string, object?>();
            foreach (KeyValuePair<string, object?> tag in tags) copy[tag.Key] = tag.Value;

            CapturedMeasurement measurement = new CapturedMeasurement(instrument.Name, Convert.ToDouble(value), copy);
            lock (_Lock) _Measurements.Add(measurement);
        }

        #endregion
    }
}
