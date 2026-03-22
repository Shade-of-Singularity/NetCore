using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Extensions useful when working with <see cref="SendingMode"/>.
    /// </summary>
    public static class SendingModeExtensions
    {
        /// <summary>
        /// Whether this <paramref name="mode"/> (<see cref="SendingMode"/>) is reliable.
        /// </summary>
        /// <param name="mode"><see cref="SendingMode"/> to analyze.</param>
        /// <returns>
        /// <c>true</c> - <paramref name="mode"/> is reliable.
        /// <c>false</c> - <paramref name="mode"/> is not reliable.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReliable(this SendingMode mode) => ((int)mode & 0b01) != 0;
        /// <summary>
        /// Whether this <paramref name="mode"/> (<see cref="SendingMode"/>) is reliable.
        /// </summary>
        /// <remarks>
        /// Native inverse of <see cref="IsReliable"/>.
        /// </remarks>
        /// <param name="mode"><see cref="SendingMode"/> to analyze.</param>
        /// <returns>
        /// <c>true</c> - <paramref name="mode"/> is not reliable.
        /// <c>false</c> - <paramref name="mode"/> is reliable.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnreliable(this SendingMode mode) => ((int)mode & 0b01) == 0;
        /// <summary>
        /// Whether this <paramref name="mode"/> (<see cref="SendingMode"/>) is ordered.
        /// </summary>
        /// <param name="mode"><see cref="SendingMode"/> to analyze.</param>
        /// <returns>
        /// <c>true</c> - <paramref name="mode"/> is ordered.
        /// <c>false</c> - <paramref name="mode"/> is not ordered.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOrdered(SendingMode mode) => ((int)mode & 0b10) != 0;
        /// <summary>
        /// Whether this <paramref name="mode"/> (<see cref="SendingMode"/>) is ordered.
        /// </summary>
        /// <remarks>
        /// Native inverse of <see cref="IsOrdered(SendingMode)"/>.
        /// </remarks>
        /// <param name="mode"><see cref="SendingMode"/> to analyze.</param>
        /// <returns>
        /// <c>true</c> - <paramref name="mode"/> is not ordered.
        /// <c>false</c> - <paramref name="mode"/> is ordered.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnordered(SendingMode mode) => ((int)mode & 0b10) == 0;
    }
}
