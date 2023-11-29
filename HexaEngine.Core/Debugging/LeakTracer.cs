namespace HexaEngine.Core.Debugging
{
    using System.Runtime.CompilerServices;

#if TRACELEAK

    using HexaEngine.Core.Graphics;
    using System.Collections.Concurrent;
    using System.Diagnostics;

#endif

    /// <summary>
    /// Provides a mechanism for tracing memory leaks in the engine.
    /// </summary>
    public static class LeakTracer
    {
#if TRACELEAK
        private static readonly ConcurrentDictionary<object, StackTrace> Instances = new();
#endif

        /// <summary>
        /// Allocates an object for tracking memory leaks.
        /// </summary>
        /// <param name="obj">The object to allocate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Allocate(object obj)
        {
#if TRACELEAK
            Instances.TryAdd(obj, new(1));
#endif
        }

        /// <summary>
        /// Releases an object from memory leak tracking.
        /// </summary>
        /// <param name="obj">The object to release.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release(object obj)
        {
#if TRACELEAK
            Instances.Remove(obj, out _);
#endif
        }

        /// <summary>
        /// Reports the live instances of tracked objects.
        /// </summary>
        public static void ReportLiveInstances()
        {
#if TRACELEAK
            foreach (var pair in Instances)
            {
                if (pair.Key is IDeviceChild child)
                {
                    Debug.WriteLine($"******LIVE INSTANCE: {child.DebugName} \n{pair}");
                }
                else
                {
                    Debug.WriteLine($"******LIVE INSTANCE: \n{pair}");
                }
            }
#endif
        }
    }
}