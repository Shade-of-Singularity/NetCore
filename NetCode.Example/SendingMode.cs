using System.Runtime.CompilerServices;

namespace NetCore.Examples
{
    internal enum SendingFlags : byte
    {
        Reliable = 0b01,
        Ordered = 0b10,
    }

    public static class Testing
    {
        internal enum SendingMode : byte
        {
            Unreliable = 0b00, // -> 0
            Reliable = 0b01, // -> 1
            Notify = 0b10, // -> 2
            Resilient = 0b11, // -> 3
        }

        bool IsReliable(SendingMode mode)
            => ((int)mode & 0b01) != 0;

        bool IsReliable(SendingMode mode)
            => ((int)mode & 0b01) != 0;
    }

    internal enum ReliableMode : byte
    {
        Unreliable = 0b00,
        Reliable = 0b01,
    }

    internal enum OrderedMode : byte
    {
        Unordered = 0b00,
        Ordered = 0b10,
    }

    internal static class Client
    {
        public static void Send(string data, SendingFlags flags)
        {

        }

        public static void Send(string data, ReliableMode reliable, OrderedMode ordered)
        {

        }
    }

    internal static class UsageBlock
    {
        public static void SeparateUsage()
        {
            Client.Send("data", ReliableMode.Reliable, OrderedMode.Ordered);
        }

        public static void PackedUsage()
        {
            Client.Send("data", SendingFlags.Reliable | SendingFlags.Ordered);
        }
    }

    internal static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReliable(this SendingFlags flags) => ((int)flags & (int)SendingFlags.Reliable) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOrdered(this SendingFlags flags) => ((int)flags & (int)SendingFlags.Ordered) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReliable(this SendingMode mode) => ((int)mode & (int)SendingFlags.Reliable) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOrdered(this SendingMode mode) => ((int)mode & (int)SendingFlags.Ordered) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnreliable(this SendingFlags flags) => ((int)flags & (int)SendingFlags.Reliable) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnordered(this SendingFlags flags) => ((int)flags & (int)SendingFlags.Ordered) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnreliable(this SendingMode mode) => ((int)mode & (int)SendingFlags.Reliable) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnordered(this SendingMode mode) => ((int)mode & (int)SendingFlags.Ordered) == 0;

        /// <summary>
        /// Explicit casting method from <see cref="SendingMode"/> to <see cref="SendingFlags"/>.
        /// </summary>
        /// <remarks>
        /// Does not (and will never) differ from a direct cast.
        /// In the future, especially in AOT, such methods might be inlined completely.
        /// </remarks>
        /// <param name="mode"><see cref="SendingMode"/> to cast</param>
        /// <returns><see cref="SendingMode"/> from <see cref="SendingFlags"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SendingFlags ToFlags(this SendingMode mode) => (SendingFlags)mode;

        /// <summary>
        /// Explicit casting method from <see cref="SendingFlags"/> to <see cref="SendingMode"/>.
        /// </summary>
        /// <remarks>
        /// Does not (and will never) differ from a direct cast.
        /// In the future, especially in AOT, such methods might be inlined completely.
        /// </remarks>
        /// <param name="flags"><see cref="SendingFlags"/> to cast.</param>
        /// <returns><see cref="SendingMode"/> from <see cref="SendingFlags"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SendingMode ToMode(this SendingFlags flags) => (SendingMode)flags;
    }
}