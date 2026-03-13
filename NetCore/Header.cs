using System;
using System.Collections.Generic;
using System.Text;

namespace NetCore
{
    /// <summary>
    /// Common class for working with headers, and <see cref="HeaderReader"/> and <see cref="HeaderWriter"/> structs.
    /// </summary>
    public static class Header
    {
        /// <summary>
        /// Unpacks (usually) a first bit in a message, indicating what type of the message it is.
        /// </summary>
        /// <param name="packed">(Usually) a first bit in a datagram, describing a <see cref="MessageType"/></param>
        public static void UnpackType(byte packed, out MessageType type, bool hasCustomHeader)
        {

        }
    }
}
