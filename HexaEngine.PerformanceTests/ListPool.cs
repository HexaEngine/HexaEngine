#define MoveAndReplace

namespace HexaEngine.PerformanceTests
{
    using System.Collections.Concurrent;

    public class ListPool<T>
    {
        private readonly ConcurrentStack<List<T>> cache = new();

        public static readonly ListPool<T> Shared = new();

        public List<T> Rent()
        {
            if (cache.TryPop(out var result))
            {
                return result;
            }
            return [];
        }

        public void Return(List<T> list, bool clear)
        {
            if (clear || !typeof(T).IsValueType) // prevent memory leaks
            {
                list.Clear();
            }

            if (cache.Count < 1024)
            {
                cache.Push(list);
            }
        }
    }
}