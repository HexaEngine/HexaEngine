namespace HexaEngine.Core
{
    using System.Collections.Concurrent;

#if TRACELEAK

    using System.Diagnostics;

#endif

    public static class LeakTracer
    {
#if TRACELEAK
        private static readonly ConcurrentDictionary<object, StackTrace> Instances = new();
#endif

        public static void Allocate(object obj)
        {
#if TRACELEAK
            Instances.TryAdd(obj, new(1));
#endif
        }

        public static void Release(object obj)
        {
#if TRACELEAK
            Instances.Remove(obj, out _);
#endif
        }

        public static void ReportLiveInstances()
        {
#if TRACELEAK
            foreach (var pair in Instances)
            {
                Debug.WriteLine($"******LIVE INSTANCE: \n{pair}");
            }
#endif
        }
    }
}