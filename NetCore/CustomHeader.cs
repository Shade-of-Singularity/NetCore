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
        public static byte RegionFlag { get; private set; }
        /// <summary>
        /// No of the byte which stores a <see cref="RegionFlag"/>.
        /// </summary>
        public static int TargetRegion { get; private set; }
        /// <summary>
        /// Order of this header relative to the other ones in a custom header data region of a message.
        /// </summary>
        public static int HeaderOrder { get; private set; }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Size of the header (in bits).
        /// </summary>
        public abstract int Size { get; }
        /// <summary>
        /// Registers this <see cref="CustomHeader{T}"/> in <see cref="CustomHeaders"/>.
        /// </summary>
        public static void Register()
        {
            CustomHeaders.Register<T>(Reset, out int index);
            TargetRegion = index >> 3;
            RegionFlag = (byte)(index & 0b111);
        }

        private static void Reset()
        {
            TargetRegion = 0;
            RegionFlag = 0;
        }
    }
}
