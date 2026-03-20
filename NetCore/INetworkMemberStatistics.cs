namespace NetCore
{
    /// <summary>
    /// Interface for <see cref="NetworkMember"/> to handle networking statistics.
    /// </summary>
    public interface INetworkMemberStatistics
    {
        /// <summary>
        /// Increments the amount of transports with established connection by 1.
        /// </summary>
        void IncrementConnectedTransports();
        /// <summary>
        /// Decrements the amount of transports with established connection by 1.
        /// </summary>
        void DecrementConnectedTransports();
    }
}
