namespace GateKeeper.Test.Shared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A single metric measurement captured by <see cref="TelemetryCapture"/>.
    /// </summary>
    public sealed class CapturedMeasurement
    {
        #region Public-Members

        /// <summary>
        /// Instrument name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Measured value, widened to double.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Measurement tags. Never null.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Tags { get; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="name">Instrument name.</param>
        /// <param name="value">Value.</param>
        /// <param name="tags">Tags.</param>
        /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
        public CapturedMeasurement(string name, double value, IReadOnlyDictionary<string, object?> tags)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
            Tags = tags ?? new Dictionary<string, object?>();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return the string form of a tag, or null when absent.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <returns>Tag value as a string, or null.</returns>
        public string? Tag(string key)
        {
            object? value;
            if (Tags.TryGetValue(key, out value)) return value?.ToString();
            return null;
        }

        /// <summary>
        /// Return true when every supplied key/value pair is present on this measurement.
        /// </summary>
        /// <param name="pairs">Alternating key, value strings.</param>
        /// <returns>True on match.</returns>
        public bool Has(params string[] pairs)
        {
            for (int i = 0; i + 1 < pairs.Length; i += 2)
            {
                if (!string.Equals(Tag(pairs[i]), pairs[i + 1], StringComparison.Ordinal)) return false;
            }

            return true;
        }

        #endregion
    }
}
