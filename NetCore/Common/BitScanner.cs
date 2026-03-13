namespace NetCore.Common
{
    /// <summary>
    /// Allows scanning for the most significant and the least significant bits.
    /// </summary>
    /// See source: https://stackoverflow.com/questions/31374628/fast-way-of-finding-most-and-least-significant-bit-set-in-a-64-bit-integer
    internal static class BitScanner
    {
        private const ulong Magic = 0x37E84A99DAE458F;
        private static readonly int[] MagicTable =
        [
            0, 1, 17, 2, 18, 50, 3, 57,
            47, 19, 22, 51, 29, 4, 33, 58,
            15, 48, 20, 27, 25, 23, 52, 41,
            54, 30, 38, 5, 43, 34, 59, 8,
            63, 16, 49, 56, 46, 21, 28, 32,
            14, 26, 24, 40, 53, 37, 42, 7,
            62, 55, 45, 31, 13, 39, 36, 6,
            61, 44, 12, 35, 60, 11, 10, 9,
        ];

        /// <summary>
        /// Scans for the most significant bit.
        /// </summary>
        /// <returns>Index of the most significant bit.</returns>
        public static int BitScanForward(ulong b)
        {
            return MagicTable[((ulong)((long)b & -(long)b) * Magic) >> 58];
        }

        /// <summary>
        /// Scans for the least significant bit.
        /// </summary>
        /// <returns>Index of the least significant bit.</returns>
        public static int BitScanReverse(ulong b)
        {
            b |= b >> 1;
            b |= b >> 2;
            b |= b >> 4;
            b |= b >> 8;
            b |= b >> 16;
            b |= b >> 32;
            b &= ~(b >> 1);
            return MagicTable[(b * Magic) >> 58];
        }
    }
}
