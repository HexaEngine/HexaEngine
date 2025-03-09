namespace HexaEngine.Resources
{
    using System.Collections.Generic;

    public static class ResourceTypeRegistry
    {
        private static int currentIndex;
        private static readonly Dictionary<Type, int> typeToUsageTypeId = new();
        private static readonly Dictionary<int, string> typeToUsageTypeName = new();
        private static readonly Lock _lock = new();

        public static string GetName(int id)
        {
            if (typeToUsageTypeName.TryGetValue(id, out var va))
            {
                return va;
            }

            return "<no-name>";
        }

        public static int Register<T>()
        {
            lock (_lock)
            {
                if (typeToUsageTypeId.TryGetValue(typeof(T), out int id))
                {
                    return id;
                }
                id = currentIndex++;
                typeToUsageTypeId.Add(typeof(T), id);
                typeToUsageTypeName.Add(id, typeof(T).Name);
                return id;
            }
        }

        public static bool Unregister<T>()
        {
            lock (_lock)
            {
                if (typeToUsageTypeId.Remove(typeof(T), out int id))
                {
                    return typeToUsageTypeName.Remove(id, out _);
                }
            }
            return false;
        }

        public static int GetId<T>()
        {
            lock (_lock)
            {
                if (typeToUsageTypeId.TryGetValue(typeof(T), out int id))
                {
                    return id;
                }
            }
            return Register<T>();
        }

        public static ResourceGuid GetGuid<T>(Guid asset)
        {
            return new(asset, GetId<T>());
        }

        public static int Register(Type type)
        {
            lock (_lock)
            {
                if (typeToUsageTypeId.TryGetValue(type, out int id))
                {
                    return id;
                }
                id = currentIndex++;
                typeToUsageTypeId.Add(type, id);
                typeToUsageTypeName.Add(id, type.Name);
                return id;
            }
        }

        public static bool Unregister(Type type)
        {
            lock (_lock)
            {
                return typeToUsageTypeId.Remove(type, out _);
            }
        }

        public static int GetId(Type type)
        {
            lock (_lock)
            {
                if (typeToUsageTypeId.TryGetValue(type, out int id))
                {
                    return id;
                }
            }
            return Register(type);
        }

        public static ResourceGuid GetGuid<T>(Guid asset, Type type)
        {
            return new(asset, GetId(type));
        }
    }
}