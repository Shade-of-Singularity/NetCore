using System;

namespace NetCore
{
    /// <summary>
    /// Helper methods for <see cref="ITransport"/>s.
    /// </summary>
    public static class TransportHelpers
    {
        /// <summary>
        /// Resolves type of an input <see cref="NetworkMember"/> in <see cref="ITransport.Initialize(NetworkMember)"/>.
        /// </summary>
        /// <param name="initializer"><see cref="NetworkMember"/> which initializes <see cref="ITransport"/>.</param>
        /// <param name="server"><see cref="NetworkMember"/> casted to <see cref="Server"/>.</param>
        /// <param name="client"><see cref="NetworkMember"/> caster to <see cref="Client"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="initializer"/> is a server.
        /// <c>false</c> if <paramref name="initializer"/> is a client.
        /// </returns>
        public static bool ResolveInitializer(this NetworkMember initializer, out Server? server, out Client? client)
        {
            if (initializer is Server s)
            {
                server = s;
                client = default;
                return true;
            }
            else if (initializer is Client c)
            {
                client = c;
                server = default;
                return false;
            }

            throw new Exception($"Cannot resolve {nameof(initializer)} type - it's not a {nameof(Server)} nor a {nameof(Client)}.");
        }
    }
}
