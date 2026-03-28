using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Extension methods and helpers for working with <see cref="Flags"/>.
    /// </summary>
    public static class FlagsHelpers
    {
        /// <summary>
        /// Increments amount of locks on a given <paramref name="header"/> instance.
        /// </summary>
        /// <returns>
        /// The <paramref name="header"/> instance itself for easier inlining.
        /// </returns>
        public static ref Flags Lock(this ref Flags header)
        {
            header.IncrementLocks();
            return ref header;
        }

        /// <inheritdoc cref="Flags.SetFlag{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Flags SetFlag<T>(this ref Flags flags) where T : INoContentFlag
        {
            flags.SetFlag<T>();
            return ref flags;
        }

        /// <inheritdoc cref="Flags.ResetFlags{T}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Flags ResetFlags<T>(this ref Flags flags) where T : INoContentFlag
        {
            flags.ResetFlags<T>();
            return ref flags;
        }
    }
}
