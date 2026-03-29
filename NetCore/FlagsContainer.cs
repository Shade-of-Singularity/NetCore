namespace NetCore
{
    /// <summary>
    /// Base class for <see cref="FlagsContainer"/> to store it within a <see cref="Flags"/> storage.
    /// </summary>
    public abstract class FlagsContainer
    {
        /// <summary>
        /// Order in which this container was initialized internally.
        /// Value is unreliable for using in networking - it might differ depending on a user.
        /// </summary>
        public abstract int InitOrder { get; }
    }
}
