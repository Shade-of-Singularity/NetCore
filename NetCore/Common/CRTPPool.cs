using System;
using System.Collections.Generic;

namespace NetCore.Common
{
    /// <summary>
    /// Stores trivia about <see cref="CRTPPool{T}"/>.
    /// </summary>
    public static class CRTPPools
    {
        /// <summary>
        /// Default capacity limit for all <see cref="CRTPPool{T}"/>.
        /// </summary>
        public const int DefaultMaxCapacity = 4;
        /// <summary>
        /// Default max capacity to be used by all <see cref="CRTPPool{T}"/> in instances which does not override it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than zero.</exception>
        public static int MaxCapacity
        {
            get => m_MaxCapacity;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(MaxCapacity)} is less than zero.");

                lock (_lock)
                {
                    if (m_MaxCapacity != value)
                    {
                        m_MaxCapacity = value;
                        OnMaxCapacityChanged?.Invoke(value);
                    }
                }
            }
        }

        private static int m_MaxCapacity;
        private static readonly object _lock = new();

        /// <summary>
        /// Handler for handling when <see cref="MaxCapacity"/> was changed.
        /// </summary>
        /// <param name="maxCapacity"></param>
        public delegate void MaxCapacityChangeHandler(int maxCapacity);
        /// <summary>
        /// Invoked when <see cref="MaxCapacity"/> changes.
        /// </summary>
        public static event MaxCapacityChangeHandler? OnMaxCapacityChanged;
    }

    /// <summary>
    /// Pool for strongly-typed items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class CRTPPool<T> where T : new()
    {
        /// <summary>
        /// Default capacity for a pool.
        /// </summary>
        public static int Capacity => m_Capacity;

        private static int m_Capacity;
        private static bool m_IsModified;
        private static readonly Stack<T> m_Pool = new();

        static CRTPPool() => CRTPPools.OnMaxCapacityChanged += OnMaxCapacityChanged;

        /// <summary>
        /// New capacity to set for a pool.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero.</exception>
        public static void SetCapacity(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), $"{nameof(Capacity)} is less than zero.");

            lock (m_Pool)
            {
                if (!m_IsModified)
                {
                    m_IsModified = true;
                    CRTPPools.OnMaxCapacityChanged -= OnMaxCapacityChanged;
                }

                SetCapacityCoreUnlocked(capacity);
            }
        }

        /// <summary>
        /// Resets capacity to use one from centralized <see cref="CRTPPools.MaxCapacity"/>.
        /// </summary>
        public static void ResetCapacity()
        { 
            lock (m_Pool)
            {
                if (m_IsModified)
                {
                    m_IsModified = false;
                    CRTPPools.OnMaxCapacityChanged += OnMaxCapacityChanged;
                }
            }
        }

        static void OnMaxCapacityChanged(int maxCapacity)
        {
            lock (m_Pool)
            {
                if (m_IsModified)
                    return;

                SetCapacityCoreUnlocked(maxCapacity);
            }
        }

        static void SetCapacityCoreUnlocked(int capacity)
        {
            m_Capacity = capacity;
            while (m_Pool.Count > capacity)
            {
                m_Pool.Pop();
            }
        }

        /// <summary>
        /// Rents item from a pool, or creates a new one.
        /// </summary>
        /// <returns>An item to be used.</returns>
        public static T Rent()
        {
            lock (m_Pool)
            {
                if (m_Pool.TryPop(out T item))
                    return item;
            }

            return new();
        }

        /// <summary>
        /// Returns item to the pool.
        /// Item will be simply de-references for GC to collect it if pool capacity is reached.
        /// </summary>
        public static void Return(T item)
        {
            lock (m_Pool)
            {
                if (m_Pool.Count >= m_Capacity)
                    return;

                m_Pool.Push(item);
            }
        }
    }
}
