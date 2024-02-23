namespace HexaEngine.Resources
{
    using System.Collections.Concurrent;

    public static class ResourceTypeRegistry
    {
        private static int currentIndex;
        private static readonly ConcurrentDictionary<Type, int> typeToUsageTypeId = new();
        private static readonly ConcurrentDictionary<int, string> typeToUsageTypeName = new();

        public static string GetName(int id)
        {
            if (typeToUsageTypeName.TryGetValue(id, out var va))
                return va;
            return "<no-name>";
        }

        public static int Register<T>()
        {
            var id = Interlocked.Increment(ref currentIndex);
            typeToUsageTypeId.TryAdd(typeof(T), id);
            typeToUsageTypeName.TryAdd(id, nameof(T));
            return id;
        }

        public static bool Unregister<T>()
        {
            if (typeToUsageTypeId.TryRemove(typeof(T), out int id))
            {
                return typeToUsageTypeName.TryRemove(id, out _);
            }
            return false;
        }

        public static int GetId<T>()
        {
            if (typeToUsageTypeId.TryGetValue(typeof(T), out int id))
            {
                return id;
            }
            return Register<T>();
        }

        public static ResourceGuid GetGuid<T>(Guid asset)
        {
            return new(asset, GetId<T>());
        }

        public static int Register(Type type)
        {
            var id = Interlocked.Increment(ref currentIndex);
            typeToUsageTypeId.TryAdd(type, id);
            typeToUsageTypeName.TryAdd(id, type.Name);
            return id;
        }

        public static bool Unregister(Type type)
        {
            return typeToUsageTypeId.TryRemove(type, out _);
        }

        public static int GetId(Type type)
        {
            if (typeToUsageTypeId.TryGetValue(type, out int id))
            {
                return id;
            }
            return Register(type);
        }

        public static ResourceGuid GetGuid<T>(Guid asset, Type type)
        {
            return new(asset, GetId(type));
        }
    }
}