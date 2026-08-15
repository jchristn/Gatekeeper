namespace GateKeeper.Test.Shared
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Minimal assertion helpers used by shared Touchstone test cases.
    /// Tests throw on failure; these helpers wrap the common comparisons so the
    /// same assertions run identically under the console, xUnit, and NUnit hosts.
    /// </summary>
    public static class TestAssert
    {
        #region Public-Methods

        /// <summary>
        /// Throws if the condition is false.
        /// </summary>
        /// <param name="condition">Condition that must be true.</param>
        /// <param name="message">Failure message when condition is false.</param>
        public static void True(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Assertion failed: " + message);
        }

        /// <summary>
        /// Throws if the condition is true.
        /// </summary>
        /// <param name="condition">Condition that must be false.</param>
        /// <param name="message">Failure message when condition is true.</param>
        public static void False(bool condition, string message)
        {
            if (condition) throw new InvalidOperationException("Assertion failed: " + message);
        }

        /// <summary>
        /// Throws if the value is null.
        /// </summary>
        /// <param name="value">Value that must not be null.</param>
        /// <param name="message">Failure message when value is null.</param>
        public static void NotNull(object? value, string message)
        {
            if (value == null) throw new InvalidOperationException("Assertion failed: " + message + ". Expected non-null.");
        }

        /// <summary>
        /// Throws if the value is not null.
        /// </summary>
        /// <param name="value">Value that must be null.</param>
        /// <param name="message">Failure message when value is not null.</param>
        public static void Null(object? value, string message)
        {
            if (value != null) throw new InvalidOperationException("Assertion failed: " + message + ". Expected null, Actual=" + value + ".");
        }

        /// <summary>
        /// Throws if expected and actual are not equal.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="expected">Expected value.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="message">Failure message.</param>
        public static void Equal<T>(T expected, T actual, string message)
        {
            if (!object.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    $"Assertion failed: {message}. Expected={expected}, Actual={actual}");
            }
        }

        /// <summary>
        /// Executes the action and verifies it throws an exception assignable to TException.
        /// </summary>
        /// <typeparam name="TException">Expected exception type.</typeparam>
        /// <param name="action">Action expected to throw.</param>
        /// <param name="message">Failure message if no exception or wrong type.</param>
        public static void Throws<TException>(Action action, string message) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Assertion failed: {message}. Expected {typeof(TException).Name}, got {ex.GetType().Name}: {ex.Message}");
            }

            throw new InvalidOperationException(
                $"Assertion failed: {message}. Expected {typeof(TException).Name} but no exception was thrown.");
        }

        /// <summary>
        /// Executes the async function and verifies it throws an exception assignable to TException.
        /// </summary>
        /// <typeparam name="TException">Expected exception type.</typeparam>
        /// <param name="func">Async function expected to throw.</param>
        /// <param name="message">Failure message if no exception or wrong type.</param>
        /// <returns>Task that completes once the assertion is checked.</returns>
        public static async Task ThrowsAsync<TException>(Func<Task> func, string message) where TException : Exception
        {
            try
            {
                await func().ConfigureAwait(false);
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Assertion failed: {message}. Expected {typeof(TException).Name}, got {ex.GetType().Name}: {ex.Message}");
            }

            throw new InvalidOperationException(
                $"Assertion failed: {message}. Expected {typeof(TException).Name} but no exception was thrown.");
        }

        #endregion
    }
}
