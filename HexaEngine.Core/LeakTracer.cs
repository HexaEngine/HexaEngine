﻿namespace HexaEngine.Core
{
#if TRACELEAK

    using System.Diagnostics;

#endif

    public static class LeakTracer
    {
#if TRACELEAK
        private static readonly Dictionary<object, StackTrace> Instances = new();
#endif

        public static void Allocate(object obj)
        {
#if TRACELEAK
            Instances.Add(obj, new(1));
#endif
        }

        public static void Release(object obj)
        {
#if TRACELEAK
            Instances.Remove(obj);
#endif
        }

        public static void ReportLiveInstances()
        {
#if TRACELEAK
            foreach (var pair in Instances)
            {
                Trace.WriteLine($"******LIVE INSTANCE: \n{pair}");
            }
            if (Instances.Count != 0)
                throw new Exception("memory leak detected!");
#endif
        }
    }
}