using System;

namespace NetCore
{
    /// <summary>
    /// Timeout for sending a message.
    /// </summary>
    public readonly struct Timeout(int ticks) : IContentFlags
    {
        /// <summary>Timeout of 1 minute.</summary>
        public static readonly Timeout Min1 = new(ticks: 60_000 * 1);
        /// <summary>Timeout of 3 minutes.</summary>
        public static readonly Timeout Min3 = new(ticks: 60_000 * 3);
        /// <summary>Timeout of 5 minutes.</summary>
        public static readonly Timeout Min5 = new(ticks: 60_000 * 5);
        /// <summary>Timeout of 10 minutes.</summary>
        public static readonly Timeout Min10 = new(ticks: 60_000 * 10);
        /// <summary>
        /// Timeout in <see cref="Environment.TickCount"/>, a.k.a. in milliseconds.
        /// </summary>
        public readonly int TimeMs = ticks;
        /// <summary>
        /// Rents a <see cref="ContentContainer{TValue}"/>, assigns a given <paramref name="value"/> to it, and returns an instance of that container.
        /// </summary>
        public static implicit operator ContentContainer<Timeout>(Timeout value) => ContentContainer<Timeout>.Rent().Set(value);
        /// <summary>
        /// DOES NOT releases the container, but returns a value of it.
        /// </summary>
        public static implicit operator Timeout(ContentContainer<Timeout> container) => container.Value;
    }
}
