using ComputerysBitStream;

namespace NetCore.BitContext
{
    internal static class Testing
    {
        static readonly Client client = new();
        static readonly Server server = new();
        public static void Main()
        {
            WriteContext context = new(stackalloc ulong[64]);
            context.WriteBool(true);
            client.SendUnreliable(context.ToByte());
        }
    }
}
