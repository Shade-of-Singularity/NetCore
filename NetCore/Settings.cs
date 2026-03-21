using NetCore.Transports;
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
        /// <summary>
        /// <para>(WIP)(Default: false)</para>
        /// If enabled - during a handshake, will check which <see cref="CustomHeader{T}"/> are defined client-side and server-side.
        /// If they are different - systems will synchronize header identifiers, and exclude from writing unused ones.
        /// <para>
        /// If disabled - <see cref="NetCore"/> will assume that headers are the same.
        /// This is typical for games which update constantly.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Only enable this feature if you communicate globally and cannot ensure that everyone will use the same core system.
        /// </remarks>
        public static bool SynchronizeHeaders
        {
            get => false;
            set => throw new NotImplementedException();
        }
        /// <summary>
        /// <para>(Default: true)</para>
        /// Enables protections and delays, useful when <see cref="NetworkMember"/> and <see cref="ITransport"/>s
        /// are accessed from multiple threads, or very frequently within one thread.
        /// </summary>
        /// <remarks>
        /// When protections are enabled - system will use <see cref="Cysharp.Threading.Tasks.UniTask.Yield"/>
        /// on some calls which return tasks to await (e.g. <see cref="NetworkMember.Start(StartupArgsProvider?, bool)"/>).
        /// If another thread accesses the same <see cref="NetworkMember"/>
        /// - small delay the yield produces make system register only the latter method.
        /// <para>
        /// If protections are disabled - there will be no small delays when you call those methods.
        /// However, it will result in higher resource usage and potential instability if accessed from multiple threads,
        /// of from a single thread but extremely frequently.
        /// </para>
        /// Delays yield methods introduce are very small (usually at max ~0.017s),
        /// so it is recommended to always keep this option enabled.
        /// </remarks>
        public static bool UseConcurrentProtections
        {
            get => m_ConcurrentProtections;
            set => m_ConcurrentProtections = false;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        static volatile int m_MaxUnreliablePacketSize = 1024 * 16; // 16 KB.
        static volatile int m_MaxReliablePacketSize = 1024 * 1024; // 1 MB
        static volatile int m_BufferSizeIncrement = 2048; // 2 KB.
        static volatile bool m_ConcurrentProtections = true;
        //static bool m_SynchronizeHeaders = false;
    }
}
