using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCore
{
    public partial class ConnectionArgs
    {
        /// <summary>
        /// Stack-space lock used for unlocking the args.
        /// </summary>
        /// <param name="args">Args to unlock on disposal.</param>
        public readonly ref struct ReturnHandle(ConnectionArgs args)
		{
			/// <summary>
			/// Releases one lock from a parent <see cref="ConnectionArgs"/>.
			/// </summary>
			public void Dispose() => args.Unlock();
        }



        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private volatile int m_ActiveLocks = 0;
        private volatile bool m_Disposed = false;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Locks this instance from being returned to the pool.
        /// </summary>
        /// <returns>The instance itself for inlining.</returns>
        public ReturnHandle Lock()
        {
            Interlocked.Increment(ref m_ActiveLocks);
            return new(this);
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Unlocks one lock.
        /// If number of active locks is at 0 - returns instance to the pool using <see cref="Return(ConnectionArgs)"/>.
        /// </summary>
        void Unlock()
		{
			if (m_Disposed)
				throw new ObjectDisposedException($"{nameof(ConnectionArgs)} is already returned to the pool.");

			if (Interlocked.Decrement(ref m_ActiveLocks) > 0)
			{
				return;
			}

			m_Disposed = true;
			Return(this);
		}




		/// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
		/// .
		/// .                                              Static Properties
		/// .
		/// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
		/// <summary>
		/// How many <see cref="ConnectionArgs"/> instances can internal pool store, until the excess is released for GC to collect.
		/// </summary>
		public static int PoolCapacity
        {
            get
            {
                lock (m_ArgsPool) return m_PoolCapacity;
            }
            set
            {
                lock (m_ArgsPool)
                {
                    const int MinimumCapacity = 4;
                    value = Math.Max(value, MinimumCapacity);
                    if (m_PoolCapacity != value)
                    {
                        m_PoolCapacity = value;
                        while (m_ArgsPool.Count > value)
                        {
                            // Removes excess instances.
                            m_ArgsPool.Pop();
                        }
                    }
                }
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static readonly Stack<ConnectionArgs> m_ArgsPool = new();
        private static volatile int m_PoolCapacity = 16;




		/// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
		/// .
		/// .                                               Public Methods
		/// .
		/// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
		/// <summary>
		/// Rents an *unlocked* <see cref="ConnectionArgs"/> instance from the internal pool.
		/// </summary>
		/// <remarks>
		/// All <see cref="Client"/> and <see cref="Server"/> sending methods
		/// will automatically <see cref="Return(ConnectionArgs)"/> the args after usage.
		/// If you want to reuse the args instance multiple times - please, lock it first:
		/// <c>
		/// <para><![CDATA[var args = ConnectionArgs.Rent();]]></para>
		/// <para><![CDATA[using (args.Lock()) {...}]]></para>
		/// </c>
		/// Or inlined:
		/// <c>
		/// <para><![CDATA[using (ConnectionArgs.Rent(out args).Lock()) {...}]]></para>
		/// </c>
		/// Or alternatively:
		/// <c>
		/// <para><![CDATA[using (ConnectionArgs.RentLocked(out args)) {...}]]></para>
		/// </c>
		/// </remarks>
		/// <returns>
		/// New instance of <see cref="ConnectionArgs"/> or the one pulled from the internal pool.
		/// </returns>
		public static ConnectionArgs Rent()
        {
            lock (m_ArgsPool)
            {
                if (m_ArgsPool.Count == 0)
                {
                    return [];
                }
                else
                {
                    ConnectionArgs args = m_ArgsPool.Pop();
                    args.m_ActiveLocks = 0;
                    args.m_Disposed = false;
                    return args;
                }
            }
        }
        /// <param name="args">
        /// The same instance of <see cref="ConnectionArgs"/> that is being returned.
        /// Provided twice for inlining.
        /// </param>
        /// <inheritdoc cref="Rent()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConnectionArgs Rent(out ConnectionArgs args) => args = Rent();
		/// <summary>
		/// Rents and instantly locks an <see cref="ConnectionArgs"/> instance from the internal pool.
		/// Return <see cref="ReturnHandle"/> is recommended to be immediately used in an <c>using (...) {}</c> block.
		/// </summary>
		/// <param name="args">Usable instance of <see cref="ConnectionArgs"/>, either new or retrieved from the pool.</param>
		/// <returns>Lock to be used in an <c>using (...) {}</c> block.</returns>
		public static ReturnHandle RentLocked(out ConnectionArgs args) => (args = Rent()).Lock();
        /// <summary>
        /// Returns <see cref="ConnectionArgs"/> to the 
        /// </summary>
        /// <param name="args"></param>
        public static void Return(ConnectionArgs args)
        {
            if (args.m_ActiveLocks > 0)
            {
                throw new Exception("Cannot return locked instance to the pool.");
            }

            lock (m_ArgsPool)
            {
                if (m_ArgsPool.Count < m_PoolCapacity)
                {
                    args.Clear();
                    m_ArgsPool.Push(args);
                }
            }
        }
    }
}
