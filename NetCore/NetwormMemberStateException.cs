using System;

namespace NetCore
{
    /// <summary>
    /// State exception of <see cref="NetworkMember"/>.
    /// Usually thrown if you attempt to start already started <see cref="NetworkMember"/>, etc.
    /// </summary>
    /// <param name="message"><inheritdoc/></param>
    public sealed class NetworkMemberStateException(string message) : Exception(message) { }
}
