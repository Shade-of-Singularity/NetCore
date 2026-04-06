namespace NetCore.Packets
{
    internal sealed class TestPacket : FlagPacket<TestPacket> { }
    internal static class Testing
    {
        static readonly Client Client = new();

        internal static void TestMethod()
        {
            TestPacket packet = new();
            Client.Send(SendingMode.Reliable, packet);
        }
    }
}
