using System;

namespace NetCore
{
    /// <summary>
    /// Stores settings, common for all internal infrastructure and plugins.
    /// </summary>
    /// <remarks>
    /// Most of the settings here apply on a startup of an system.
    /// And there is no guarantee that plugins will update their values when you change them at runtime.
    /// So please modify them before starting any other systems.
    /// </remarks>
    public static class Settings
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Minimal value for <see cref="MaxUnreliablePacketSize"/>.
        /// </summary>
        public const int MinUnreliablePacketSize = 1024; // 1KB;
        /// <summary>
        /// Minimal value for <see cref="MaxUnreliablePacketSize"/>.
        /// </summary>
        public const int MinReliablePacketSize = 1024; // 1KB;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Static Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// <para>(Default: 1024*16 -> 16KB)</para>
        /// Max size of an unreliable package (in bytes), including a message header.
        /// Plugins working with unreliable messages can use this value to limit their internal buffer sizes.
        /// </summary>
        public static int MaxUnreliablePacketSize
        {
            get => m_MaxUnreliablePacketSize;
            set => m_MaxUnreliablePacketSize = Math.Max(MinUnreliablePacketSize, value);
        }

        /// <summary>
        /// <para>(Default: 1024*1024 -> 1MB)</para>
        /// Max size of an reliable package (in bytes), including 
        /// </summary>
        public static int MaxReliablePacketSize
        {
            get => m_MaxReliablePacketSize;
            set => m_MaxReliablePacketSize = Math.Max(MinReliablePacketSize, value);
        }

        /// <summary>
        /// <para>[Advanced] (Default: 1024*2 -> 2KB)</para>
        /// Size increment (in bytes) to use on internal buffer resize.
        /// On resize, size will "snap" to this value (inclusively).
        /// </summary>
        /// <remarks>
        /// For example, when <see cref="ITransport"/> needs to resize buffer from 2KB to 2.1KB
        /// - with increment set to 1KB, it will snap a size to 3KB. With 8KB increment:
        /// (8KB -> 9KB) -> 16KB.
        /// (24KB -> 30KB) -> 32KB.
        /// (1MB -> 1MB+4KB) -> 1MB+8KB.
        /// </remarks>
        public static int BufferSizeIncrement
        {
            get => m_BufferSizeIncrement;
            set => m_BufferSizeIncrement = Math.Max(0, value);
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        static int m_MaxUnreliablePacketSize = 1024 * 16; // 16 KB.
        static int m_MaxReliablePacketSize = 1024 * 1024; // 1 MB
        static int m_BufferSizeIncrement = 2048; // 2 KB.
    }
}
