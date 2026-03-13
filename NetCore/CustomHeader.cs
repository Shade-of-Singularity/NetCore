namespace NetCore
{
    /// <summary>
    /// Custom message header to apply.
    /// </summary>
    public abstract class CustomHeader<T> where T : CustomHeader<T>, new()
    {
        /// <summary>
        /// Instance of the header. Provided for accessing overridden parameters and methods.
        /// </summary>
        public static readonly T Instance = new();
        /// <summary>
        /// Size of the header (in bits).
        /// </summary>
        public static readonly int SizeInBits = Instance.Size;
        /// <summary>
        /// Flag position in a header, indicating whether this header was registered or not.
        /// </summary>
        public static byte ByteFlag { get; private set; }
        /// <summary>
        /// No of the byte which stores a <see cref="ByteFlag"/>.
        /// </summary>
        public static uint TargetByte { get; private set; }
        /// <summary>
        /// Size of the header (in bits).
        /// </summary>
        public abstract int Size { get; }
        /// <summary>
        /// Registers this <see cref="CustomHeader{T}"/> in <see cref="CustomHeaders"/>.
        /// </summary>
        public static void Register()
        {
            // Intentionally empty - triggers a static constructor.
            ByteFlag = 0; // TODO: Register properly.
            TargetByte = ByteFlag / 7u; // Due to ZigZag encoding, each 8th bit is used to encode whether a next region exist.
        }
    }
}
