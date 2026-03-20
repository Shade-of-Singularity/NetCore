namespace NetCore.Transports
{
    /// <summary>
    /// <see cref="System.IO.Stream"/>-based transport type.
    /// Depending on the type, usually (Ordered and Reliable) transportation method.
    /// Natively supported by <see cref="TCP.TCPTransport"/> (WIP).
    /// </summary>
    public interface IStreamTransport : ITransport
    {
        // TODO: Add stream writing methods.
        // TODO: Also write a header first.
    }
}
