using System;

namespace NetCore
{
    internal static class Testing
    {
        sealed class UIDHeader : CustomHeader<UIDHeader>
        {
            public override int Size => sizeof(ulong) << Bits;
        }

        sealed class AlignmentHeader : CustomHeader<AlignmentHeader>
        {
            public override int Size => sizeof(byte) << Bits;
        }

        // Note: This is a good idea actually.
        //  We can provide component system for fast UI sharing this way...
        //  I think :D
        //  We will have to deal with duplicates somehow, and introduce header contexts.
        //  Encoding/Decoding will have to be as fast or faster than simply reading UTF-8 header.
        enum Alignment : byte
        {
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left,
            TopLeft,
        }

        static readonly Client client = new();
        static readonly Server server = new();
        public static void Main()
        {
            Header header;
            using (header = Header.GetLocked())
            {
                header.Set<UIDHeader>(325623253uL);
                header.Set<AlignmentHeader, Alignment>(Alignment.BottomLeft);
                Console.WriteLine($"UID (client): {header.GetULong<UIDHeader>()}");
                // Note: we can probably generate a custom header at runtime for enums.
                //  Size can be defined procedurally as well.
                //  However, this will only be possible to use with enum-only headers.
                Console.WriteLine($"Alignment (client): {header.GetEnum<AlignmentHeader, Alignment>()}");
                client.SendReliable(ref header, default);
            }

            using (header = Header.GetLocked())
            {
                header.Lock();
                header.Set<UIDHeader>(54634563463546uL);
                header.Set<AlignmentHeader, Alignment>(Alignment.TopRight);
                Console.WriteLine($"UID (server): {header.GetULong<UIDHeader>()}");
                Console.WriteLine($"Alignment (server): {header.GetEnum<AlignmentHeader, Alignment>()}");
                server.SendReliable(ref header, default);
            }

            client.SendReliable(stackalloc byte[8], Construct);
            static void Construct(ref Header header)
            {
                header.Set<UIDHeader>(981231234uL);
                header.Set<AlignmentHeader, Alignment>(Alignment.Bottom);
            }

            client.ForEachReliableTransport(static (c, t) => c.RemoveReliableTransport(t));
            client.ForEachUnreliableTransport(static (c, t) => c.RemoveUnreliableTransport(t));
        }
    }
}
