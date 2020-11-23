namespace Annium.Core.DependencyInjection
{
    public interface IIndex<in TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        /// <summary>
        /// Get the value associated with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The value to retrieve.</param>
        /// <returns>The associated value.</returns>
        TValue this[TKey key] { get; }

        /// <summary>
        /// Get the value associated with <paramref name="key"/> if any is available.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>True if a value associated with the key exists.</returns>
        bool TryGetValue(TKey key, out TValue value);
    }
}