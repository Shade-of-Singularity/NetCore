namespace NetCore
{
    /// <summary>
    /// Custom message header to apply.
    /// </summary>
    public abstract class CustomHeader<T> where T : CustomHeader<T>, new()
    {
        /// <summary>
        /// Describes how many bits there are in byte - 8.
        /// <para>When offset by this value to the left - multiplies value by 8.</para>
        /// <para>When offset by this value to the right - divides value by 8.</para>
        /// </summary>
        protected const int Bits = 3;
        /// <summary>
        /// Instance of the header. Provided for accessing overridden parameters and methods.
        /// </summary>
        public static readonly T Instance = new();
        /// <summary>
        /// Whether this header contains sensitive data.
        /// Such data will be wiped to prevent use-after-free vulnerabilities within one AppDomain.
        /// </summary>
        public static readonly bool IsSensitive = Instance.Sensitive;
        /// <summary>
        /// Size of the header (in bits).
        /// </summary>
        public static readonly int SizeInBits = Instance.Size;
        /// <summary>
        /// Minimal amount of bytes needed to encode this header.
        /// Useful when creating temporary buffers for <see cref="Header"/> operations.
        /// </summary>
        public static readonly int SizeInBytes = (Instance.Size + 7) >> 3; // This is division by 8 with rounding up.
        /// <summary>
        /// Descriptor, telling <see cref="CustomHeaders"/> how to register this <see cref="CustomHeader{T}"/> instance.
        /// </summary>
        internal static readonly CustomHeaderDescriptor Descriptor = new(SetupParameters, ProvideRegion);
        /// <summary>
        /// Order in which this header was registered. Corresponds to the bit it occupies.
        /// </summary>
        public static int Order { get; private set; }
        /// <summary>
        /// No of the byte which stores a <see cref="RegionFlag"/>.
        /// </summary>
        public static int Region { get; private set; }
        /// <summary>
        /// Flag position in a header, indicating whether this header was registered or not.
        /// </summary>
        public static byte RegionFlag { get; private set; }
        /// <summary>
        /// Position in a content array.
        /// </summary>
        public static int ContentPosition { get; private set; }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Source data used for static identification of the header.
        /// This identifier should not change
        /// </summary>
        //public abstract string StaticID { get; }
        /// <summary>
        /// Size, allocated for the header (in bits).
        /// Header can never use more bits than declared here.
        /// </summary>
        /// <remarks>
        /// The return value will be cached in <see cref="SizeInBits"/> and <see cref="SizeInBytes"/> immediately on registration.
        /// </remarks>
        public abstract int Size { get; }
        /// <summary>
        /// (Default: false)
        /// Whether this header contains any sensitive data that should be wiped.
        /// </summary>
        public virtual bool Sensitive => false;
        /// <summary>
        /// Registers this <see cref="CustomHeader{T}"/> in <see cref="CustomHeaders"/>.
        /// </summary>
        public static void Register() => CustomHeaders.Register<T>();




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static void SetupParameters(int order, int contentPosition)
        {
            Order = order;
            Region = (order + 7) >> 3;
            RegionFlag = (byte)(1 << (order & 0b111));
            ContentPosition = contentPosition;
        }

        private static void ProvideRegion(out int contentPosition, out int sizeInBytes)
        {
            contentPosition = ContentPosition;
            sizeInBytes = SizeInBytes;
        }
    }
}
