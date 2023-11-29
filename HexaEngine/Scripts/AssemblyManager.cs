namespace HexaEngine.Scripts
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Loader;

    public static class AssemblyManager
    {
        private static AssemblyLoadContext assemblyLoadContext;
        private static readonly List<Assembly> assemblies = new();
        private static readonly Dictionary<Type, IList<Type>> _typeCache = new();
        private static readonly Dictionary<Type, string[]> _typeNameCache = new();

        private static readonly Dictionary<Type, Array> _enumCache = new();
        private static readonly Dictionary<Type, string[]> _enumNameCache = new();

        static AssemblyManager()
        {
            assemblyLoadContext = new AssemblyLoadContext(nameof(AssemblyManager), true);
        }

        public static IReadOnlyList<Assembly> Assemblies => assemblies;

        public static event EventHandler<Assembly>? AssemblyLoaded;

        public static event EventHandler<EventArgs?>? AssembliesUnloaded;

        public static Assembly? Load(string path)
        {
            string? folder = Path.GetDirectoryName(path);
            if (folder == null)
            {
                return null;
            }

            string filename = Path.GetFileName(path);
            string pdb = Path.Combine(folder, Path.GetFileNameWithoutExtension(filename) + ".pdb");
            Assembly assembly;
            if (File.Exists(pdb))
            {
                using MemoryStream ms = new(File.ReadAllBytes(path));
                using MemoryStream ms2 = new(File.ReadAllBytes(pdb));
                assembly = assemblyLoadContext.LoadFromStream(ms, ms2);
            }
            else
            {
                using MemoryStream ms = new(File.ReadAllBytes(path));
                assembly = assemblyLoadContext.LoadFromStream(ms);
            }

            assemblies.Add(assembly);
            AssemblyLoaded?.Invoke(null, assembly);

            _typeCache.Clear();
            _typeNameCache.Clear();

            return assembly;
        }

        public static async Task<Assembly?> LoadAsync(string path)
        {
            string? folder = Path.GetDirectoryName(path);
            if (folder == null)
            {
                return null;
            }

            string filename = Path.GetFileName(path);
            string pdb = Path.Combine(folder, Path.GetFileNameWithoutExtension(filename) + ".pdb");
            Assembly assembly;
            if (File.Exists(pdb))
            {
                using MemoryStream ms = new(await File.ReadAllBytesAsync(path));
                using MemoryStream ms2 = new(await File.ReadAllBytesAsync(pdb));
                assembly = assemblyLoadContext.LoadFromStream(ms, ms2);
            }
            else
            {
                using MemoryStream ms = new(await File.ReadAllBytesAsync(path));
                assembly = assemblyLoadContext.LoadFromStream(ms);
            }

            assemblies.Add(assembly);
            AssemblyLoaded?.Invoke(null, assembly);

            _typeCache.Clear();
            _typeNameCache.Clear();

            return assembly;
        }

        public static IList<Type> GetAssignableTypes<T>()
        {
            if (!_typeCache.TryGetValue(typeof(T), out var result))
            {
                try
                {
                    result = Assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(typeof(T)))).ToList();
                    _typeCache.Add(typeof(T), result);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get assignable types");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to get assignable types", ex.Message);
                    return Array.Empty<Type>();
                }
            }
            return result;
        }

        public static Type? GetAssignableType<T>()
        {
            var types = GetAssignableTypes<T>();
            return types.FirstOrDefault();
        }

        public static IList<Type> GetAssignableTypes(Type type)
        {
            if (Assemblies.Count == 0)
                return Array.Empty<Type>();

            if (!_typeCache.TryGetValue(type, out var result))
            {
                try
                {
                    result = Assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(type))).ToList();
                    _typeCache.Add(type, result);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get assignable types");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to get assignable types", ex.Message);
                    return Array.Empty<Type>();
                }
            }
            return result;
        }

        public static string[] GetAssignableTypeNames(Type type)
        {
            if (Assemblies.Count == 0)
                return [];

            if (!_typeNameCache.TryGetValue(type, out var result))
            {
                try
                {
                    result = GetAssignableTypes(type).Select(x => x.Name).ToArray();
                    _typeNameCache.Add(type, result);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get assignable type names");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to get assignable type names", ex.Message);
                    return [];
                }
            }
            return result;
        }

        public static Array GetEnumValues(Type type)
        {
            if (Assemblies.Count == 0)
                return Array.Empty<Type>();

            if (!_enumCache.TryGetValue(type, out var result))
            {
                try
                {
                    result = Enum.GetValues(type);
                    _enumCache.Add(type, result);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get enum values");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to get enum values", ex.Message);
                    return Array.Empty<Type>();
                }
            }
            return result;
        }

        public static Array GetEnumValues<T>()
        {
            return GetEnumValues(typeof(T));
        }

        public static string[] GetEnumNames(Type type)
        {
            if (Assemblies.Count == 0)
                return [];

            if (!_enumNameCache.TryGetValue(type, out var result))
            {
                try
                {
                    result = Enum.GetNames(type);
                    _enumNameCache.Add(type, result);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get enum value names");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to get enum value names", ex.Message);
                    return [];
                }
            }
            return result;
        }

        public static string[] GetEnumNames<T>()
        {
            return GetEnumNames(typeof(T));
        }

        public static IEnumerable<Type> GetAssignableTypes<T>(Assembly assembly)
        {
            if (Assemblies.Count == 0)
                return [];

            try
            {
                return assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(typeof(T)));
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to get assignable types");
                Logger.Log(ex);
                MessageBox.Show("Failed to get assignable types", ex.Message);
                return [];
            }
        }

        public static IEnumerable<Type> GetAssignableTypes(Assembly assembly, Type type)
        {
            if (Assemblies.Count == 0)
                return [];

            try
            {
                return assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(type));
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to get assignable types");
                Logger.Log(ex);
                MessageBox.Show("Failed to get assignable types", ex.Message);
                return [];
            }
        }

        public static Type? GetType(string name)
        {
            if (Assemblies.Count == 0)
                return null;

            foreach (Assembly assembly in Assemblies)
            {
                try
                {
                    Type? type = assembly.GetType(name, false, false);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get type");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to get type", ex.Message);
                    return null;
                }
            }
            return null;
        }

        public static void Unload()
        {
            _typeCache.Clear();
            _typeNameCache.Clear();
            _enumCache.Clear();
            _enumNameCache.Clear();
            assemblies.Clear();
            assemblyLoadContext.Unload();
            assemblyLoadContext = new(nameof(AssemblyManager), true);
            AssembliesUnloaded?.Invoke(null, null);
        }
    }
}