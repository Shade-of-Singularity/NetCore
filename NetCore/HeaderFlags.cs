using System;

namespace NetCore
{
    /// <summary>
    /// Encodes special data which <see cref="Header"/> can have.
    /// </summary>
    /// <remarks>
    /// Base value for those flags can change.
    /// Do not assume it will always be <see cref="byte"/>.
    /// Better use <see cref="Header"/> natively provided by the library:
    /// <para><see cref="NetworkMember.GetHeader()"/></para>
    /// For encoding, use <see cref="HeaderFlagsHelpers"/> instead of manually writing your own methods.
    /// You can retrieve a size of this value by using <see cref="HeaderFlagsHelpers.HeaderBits"/>.
    /// </remarks>
    public enum HeaderFlags : byte { } // Merely a carrier for other types.

    /// <summary>
    /// Type of the header - what it holds, etc.
    /// </summary>
    [Obsolete("I feel like we can simply use one type instead of two.")]
    public enum HeaderType : byte
    {
        /// <summary>
        /// Defines headers, which are system-defined.
        /// Those are used for welcoming messages, internal communication, etc.
        /// They only include build-in headers, allowing for optimizations due to predictable initialization order.
        /// </summary>
        System = 0,
        /// <summary>
        /// Defines all headers.
        /// </summary>
        Custom = 1,
    }

    /// <summary>
    /// Encodes how <see cref="CustomHeaders"/> are defined in a given header instance.
    /// </summary>
    public enum CustomHeaderUsage : byte
    {
        /// <summary>
        /// Custom headers are not defined.
        /// </summary>
        None = 0,
        /// <summary>
        /// Headers are explicitly defined in the message, in full encoding without packing.
        /// Usually happens when system haven't communicated header types yet, or will never do, like with connection-less systems.
        /// </summary>
        Explicit = 0b01,
        /// <summary>
        /// Headers are defined via flags in a beginning of the message.
        /// If header mappings was not communicated before this message arrives - such messages will be discarded.
        /// </summary>
        Flags = 0b10,
        /// <summary>
        /// Headers are defined via flags, grouped together for better packing.
        /// If header mappings was not communicated before this message arrives - such messages will be discarded.
        /// </summary>
        /// <remarks>
        /// Groups, depending on the implementation, might use 8bit packing, or implement a very optimized quad map.
        /// 8bit packing will simply pack all headers in groups of 8,
        ///  and if any header from a given group was used - it will declare it in the header.
        /// Quad-map will pack all the headers in groups of 4, and then group those groups in groups of 4 as well.
        ///  By traversing the map layer-by-layer, you can retrieve all the identifiers used to encode the header.
        /// Which algorithm is better is unknown at the moment, but both have their advantages:
        /// - 8bit groups will excel if you use headers sparingly, and scale incrementally (8, 16, 24, 32, ...)
        /// - quad-maps will excel if you headers cannot be categorized, but might take more bits, since each layer takes 4 bit per region to encode.
        /// With Quad-map encoding, first byte encodes the amount of layers. 0 - represents 1 layer, and 15 - 16 layers.
        /// 8 layers allow to encode 262,144 unique headers.
        /// This is probably an overkill already - synchronizing that many headers would be slow anyway.
        /// But this way we will occupy 4 bits, and 4 other bits in a byte can be used to encode a top level of the quad map.
        /// </remarks>
        FlagGroups = 0b11,
    }

    /// <summary>
    /// Helpers for working with <see cref="HeaderFlags"/>
    /// </summary>
    /// <remarks>
    /// Most of the flags are not stored at their real positions in a header to make flags usable in switch cases.
    /// This is why you need helper methods.
    /// </remarks>
    public static class HeaderFlagsHelpers
    {
        /// <summary>
        /// How many bits are used to encode <see cref="HeaderFlags"/>.
        /// </summary>
        public const int HeaderBits = 8;

        /// <see cref="CustomHeaderUsage"/>
        internal const uint CustomHeaderUsageMask = 0b11;
        internal const int CustomHeaderUsageShift = 0;

        /// <summary>
        /// Decodes <see cref="CustomHeaderUsage"/> from a given <see cref="HeaderFlags"/> value.
        /// </summary>
        public static CustomHeaderUsage GetCustomHeaderUsage(this HeaderFlags flags)
        {
            return (CustomHeaderUsage)(((uint)flags >> CustomHeaderUsageShift) & CustomHeaderUsageMask);
        }

        /// <summary>
        /// Encodes <see cref="CustomHeaderUsage"/> in a given <see cref="HeaderFlags"/> value.
        /// </summary>
        public static HeaderFlags SetCustomHeaderUsage(this HeaderFlags flags, CustomHeaderUsage usage)
        {
            return (HeaderFlags)(((uint)flags & ~(CustomHeaderUsageMask << CustomHeaderUsageShift)) | ((uint)usage << CustomHeaderUsageShift));
        }
    }
}