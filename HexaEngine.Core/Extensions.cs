namespace HexaEngine.Core
{
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ts, Action<T> action)
        {
            foreach (T item in ts)
            {
                action(item);
            }
        }

        public static void ForEach<T, T1>(this IEnumerable<T> ts, Action<T1> action)
        {
            foreach (T item in ts)
            {
                if (item is T1 t1)
                {
                    action(t1);
                }
            }
        }
    }
}