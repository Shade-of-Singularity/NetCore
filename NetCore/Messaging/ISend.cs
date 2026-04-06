namespace NetCore.Messaging
{
    /// <summary>
    /// Declares all sending methods for all <see cref="SendingMode"/>s.
    /// </summary>
    public interface ISend : ISendUnreliable, ISendReliable, ISendSequential, ISendResilient { }
}
