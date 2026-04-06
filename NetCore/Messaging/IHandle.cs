namespace NetCore.Messaging
{
    /// <summary>
    /// Declares all message handling methods for all <see cref="SendingMode"/>s.
    /// </summary>
    public interface IHandle : IHandleUnreliable, IHandleReliable, IHandleSequential, IHandleResilient { }
}
