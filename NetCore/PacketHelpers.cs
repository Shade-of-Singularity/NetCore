using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Helpers for working with network packets in <see cref="NetCore"/>.
    /// </summary>
    public static class DatagramHelpers
    {
        /// <summary>
        /// Encodes the entire message to a <paramref name="buffer"/>, and prepares it for sending.
        /// </summary>
        /// <remarks>
        /// Throw on insufficient buffer size.
        /// </remarks>
        /// <param name="buffer">Buffer to encode a message to.</param>
        /// <param name="flags">
        /// <see cref="HeaderFlags"/> to encode.
        /// <see cref="CustomHeaderUsage"/> will be provided automatically unless manually specified.
        /// </param>
        /// <param name="header">Header to encode.</param>
        /// <param name="datagram">Datagram to encode.</param>
        /// <returns>
        /// Amount of bytes, encoded into a <paramref name="buffer"/>.
        /// </returns>
        public static int Encode(Span<byte> buffer, HeaderFlags flags, in Header header, ReadOnlySpan<byte> datagram)
        {
            int bytes = 0;

            buffer[0] = (byte)flags;
            bytes += 1;
            buffer = buffer[1..]; // Skips bytes, containing flags.

            // TODO: Support header encoding.
            switch (flags.GetCustomHeaderUsage())
            {
                case CustomHeaderUsage.None: break;
                case CustomHeaderUsage.Explicit: break;
                case CustomHeaderUsage.Flags: break;
                case CustomHeaderUsage.FlagGroups: break;
                default: throw new SwitchExpressionException(flags.GetCustomHeaderUsage());
            }
            bytes += 0;
            buffer = buffer[0..]; // Skips bytes, containing header (and flags).

            // Encodes datagram as a raw bytes.
            datagram.CopyTo(buffer);
            return bytes + datagram.Length;
        }

        /// <summary>
        /// Attempts to encode the message into a <paramref name="buffer"/>.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> on success and <see langword="false"/> when <paramref name="buffer"/> is too small.
        /// </returns>
        /// <exception cref="NotImplementedException">This method is pending an implementation, as it is under review.</exception>
        /// <inheritdoc cref="Encode"/>
        public static bool TryEncode(Span<byte> buffer, HeaderFlags flags, in Header header, ReadOnlySpan<byte> datagram, out int bytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decodes a message, previously encoded using <see cref="Encode"/> method.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <param name="flags">Flags, decoded from <paramref name="buffer"/>.</param>
        /// <param name="header">Header, decoded from <paramref name="buffer"/>.</param>
        /// <param name="datagram">Byte content, decoded from <paramref name="buffer"/>.</param>
        public static void Decode(ReadOnlySpan<byte> buffer, out HeaderFlags flags, out Header header, out ReadOnlySpan<byte> datagram)
        {
            flags = (HeaderFlags)buffer[0];

            // TODO: Support header decoding.
            header = flags.GetCustomHeaderUsage() switch
            {
                CustomHeaderUsage.None => default,
                CustomHeaderUsage.Explicit or CustomHeaderUsage.Flags or CustomHeaderUsage.FlagGroups
                    => throw new NotSupportedException("Header encoding/decoding is yet to be supported."),
                _ => throw new SwitchExpressionException(flags.GetCustomHeaderUsage()),
            };

            // Everything else is a datagram.
            datagram = buffer[1..];
        }
    }
}
