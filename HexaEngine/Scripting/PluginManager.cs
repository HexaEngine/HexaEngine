namespace HexaEngine.Core.Plugins
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Loader;

    /*public static class PluginManager
    {
        private static AssemblyLoadContext assemblyLoadContext;
        private static readonly List<Assembly> assemblies = new();
        private static readonly Dictionary<Type, IEnumerable<Type>> _cache = new();

        static PluginManager()
        {
            assemblyLoadContext = new AssemblyLoadContext(nameof(PluginManager), true);
        }

        public static IReadOnlyList<Assembly> Assemblies => assemblies;

        public static event EventHandler<Assembly>? AssemblyLoaded;

        public static event EventHandler<EventArgs?>? AssembliesUnloaded;

        public static Assembly? Register(string path)
        {
            string? folder = Path.GetDirectoryName(path);
            if (folder == null) return null;
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

            return assembly;
        }

        public static IEnumerable<Type> GetAssignableTypes<T>()
        {
            if (!_cache.TryGetValue(typeof(T), out var result))
            {
                result = Assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(typeof(T))));
                _cache.Add(typeof(T), result);
            }
            return result;
        }

        public static IEnumerable<Type> GetAssignableTypes(Type type)
        {
            if (!_cache.TryGetValue(type, out var result))
            {
                result = Assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(type)));
                _cache.Add(type, result);
            }
            return result;
        }

        public static IEnumerable<Type> GetAssignableTypes<T>(Assembly assembly)
        {
            return assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(typeof(T)));
        }

        public static IEnumerable<Type> GetAssignableTypes(Assembly assembly, Type type)
        {
            return assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(type));
        }

        public static Type? GetType(string name)
        {
            foreach (Assembly assembly in Assemblies)
            {
                Type? type = assembly.GetTypes().FirstOrDefault(x => x.AssemblyQualifiedName == name);
                if (type != null)
                    return type;
            }
            return null;
        }

        public static void Clear()
        {
            _cache.Clear();
            assemblies.Clear();
            assemblyLoadContext.Unload();
            assemblyLoadContext = new(nameof(PluginManager), true);
            AssembliesUnloaded?.Invoke(null, null);
        }
    }*/
}