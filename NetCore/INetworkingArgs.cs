using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NetCore
{
    /// <summary>
    /// Base interface for both <see cref="IReadOnlyStartupArgs"/> and <see cref="IReadOnlyConnectionArgs"/>
    /// </summary>
    public interface IReadOnlyNetworkingArgs : IDictionary<object, object?> { }

    /// <summary>
    /// Extensions for simpler <see cref="IReadOnlyNetworkingArgs"/> usage.
    /// </summary>
    public static class NetworkingArgsExtensions
    {
        /// <summary>
        /// Attempts to retrieve a specific item from <see cref="IReadOnlyNetworkingArgs"/> under a given <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">Type of the item to expect.</typeparam>
        /// <param name="args">Args to go through.</param>
        /// <param name="key">Key to seek for in <paramref name="args"/>.</param>
        /// <param name="item">Item from provided <paramref name="args"/> or <c>null</c>.</param>
        /// <returns>
        /// <c>true</c> if item was found and it is not <c>null</c>.
        /// <c>false</c> if item was not found or item under the given key is <c>null</c>.
        /// </returns>
        public static bool TryGet<T>(this IReadOnlyNetworkingArgs args, object key, [NotNullWhen(true)] out T? item)
        {
            bool result = args.TryGetValue(key, out object? stored) && stored is not null;
            item = (T?)stored;
            return result;
        }
    }
}
