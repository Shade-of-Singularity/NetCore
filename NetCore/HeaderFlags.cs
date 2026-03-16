using System;

namespace NetCore
{
    /// <summary>
    /// Encodes special data which <see cref="HeaderReader"/> or <see cref="HeaderWriter"/> can have.
    /// </summary>
    /// <remarks>
    /// Base value for those flags can change. Do not assume it will always be <see cref="byte"/>.
    /// Better use <see cref="HeaderReader(ReadOnlySpan{byte})"/> or <see cref="HeaderWriter.TryRead(ReadOnlySpan{byte}, out HeaderReader)"/> instead.
    /// For encoding, use <see cref="HeaderFlagsHelpers"/> instead of manually writing your own methods.
    /// </remarks>
    public enum HeaderFlags : byte { } // Merely a carrier for other types.

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
    public static class HeaderFlagsHelpers
    {
        /// <summary>
        /// Covers all the bits in <see cref="HeaderFlags"/>, covering values for <see cref="CustomHeaderUsage"/>.
        /// </summary>
        public const HeaderFlags CustomHeaderUsageMask = (HeaderFlags)0b11;
        /// <summary>
        /// Decodes <see cref="CustomHeaderUsage"/> from a given <see cref="HeaderFlags"/> value.
        /// </summary>
        public static CustomHeaderUsage GetCustomHeaderUsage(this HeaderFlags flags) => (CustomHeaderUsage)(flags & CustomHeaderUsageMask);
    }
}