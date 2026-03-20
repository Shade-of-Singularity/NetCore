using NetCore.Transports;

namespace NetCore
{
    /// <summary>
    /// <inheritdoc cref="SendingMode"/>
    /// </summary>
    /// <remarks>
    /// Uses CRTP. Inherit to specify a custom mode.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public abstract class SendingMode<T> where T : SendingMode<T>
    {

    }

    /// <summary>
    /// Message sending mode (think: Reliable, Unreliable, Ordered, etc.)
    /// Sending mode is not synchronized when <see cref="Settings.SynchronizeHeaders"/> is enabled.
    /// It's only used to see which <see cref="ITransport"/>s support a specific sending mode.
    /// </summary>
    public abstract class SendingMode
    {
    }
}
