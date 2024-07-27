namespace HexaEngine.Scripts
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.UI;
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Loader;

    public static class ScriptAssemblyManager
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ScriptAssemblyManager));
        private static AssemblyLoadContext assemblyLoadContext;
        private static readonly List<Assembly> assemblies = new();
        private static readonly Dictionary<Guid, Type> _typeCache = new();
        private static readonly Dictionary<Type, IList<Type>> _typeToTypeCache = new();
        private static readonly Dictionary<Type, string[]> _typeNameCache = new();

        private static readonly Dictionary<Type, Array> _enumCache = new();
        private static readonly Dictionary<Type, string[]> _enumNameCache = new();

        private static readonly ManualResetEventSlim loadLock = new(false);
        private static volatile bool isInvalid;

        static ScriptAssemblyManager()
        {
            assemblyLoadContext = new AssemblyLoadContext(nameof(ScriptAssemblyManager), true);
        }

        public static IReadOnlyList<Assembly> Assemblies => assemblies;

        public static bool IsInvalid => isInvalid;

        public static event EventHandler<Assembly>? AssemblyLoaded;

        public static event EventHandler<EventArgs?>? AssembliesUnloaded;

        public static void SetInvalid(bool isInvalid)
        {
            ScriptAssemblyManager.isInvalid = isInvalid;
            loadLock.Set();
        }

        public static Assembly? Load(string path)
        {
            loadLock.Reset();
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
                FileStream fs = File.OpenRead(path);
                FileStream fsPdb = File.OpenRead(pdb);

                try
                {
                    assembly = assemblyLoadContext.LoadFromStream(fs, fsPdb);
                }
                finally
                {
                    fs.Close();
                    fsPdb?.Close();
                }
            }
            else
            {
                FileStream fs = File.OpenRead(path);

                try
                {
                    assembly = assemblyLoadContext.LoadFromStream(fs);
                }
                finally
                {
                    fs.Close();
                }
            }

            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                _typeCache.Add(type.GUID, type);
            }

            _typeToTypeCache.Clear();
            _typeNameCache.Clear();

            assemblies.Add(assembly);

            loadLock.Set();

            AssemblyLoaded?.Invoke(null, assembly);

            return assembly;
        }

        public static Task<Assembly?> LoadAsync(string path)
        {
            return Task.Run(() => Load(path));
        }

        public static IList<Type> GetAssignableTypes<T>()
        {
            loadLock.Wait();

            if (isInvalid)
            {
                return [];
            }

            if (!_typeToTypeCache.TryGetValue(typeof(T), out var result))
            {
                try
                {
                    result = Assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(typeof(T)))).ToList();
                    _typeToTypeCache.Add(typeof(T), result);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to get assignable types");
                    Logger.Log(ex);
                    MessageBox.Show("Failed to get assignable types", ex.Message);
                    return [];
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
            loadLock.Wait();

            if (isInvalid)
            {
                return [];
            }

            if (Assemblies.Count == 0)
            {
                return [];
            }

            if (!_typeToTypeCache.TryGetValue(type, out var result))
            {
                try
                {
                    result = Assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(type))).ToList();
                    _typeToTypeCache.Add(type, result);
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
            loadLock.Wait();

            if (isInvalid)
            {
                return [];
            }

            if (Assemblies.Count == 0)
            {
                return [];
            }

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
            loadLock.Wait();

            if (isInvalid)
            {
                return Array.Empty<Type>();
            }

            if (Assemblies.Count == 0)
            {
                return Array.Empty<Type>();
            }

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
            loadLock.Wait();

            if (isInvalid)
            {
                return [];
            }

            if (Assemblies.Count == 0)
            {
                return [];
            }

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
            loadLock.Wait();

            if (isInvalid)
            {
                return [];
            }

            if (Assemblies.Count == 0)
            {
                return [];
            }

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
            loadLock.Wait();

            if (isInvalid)
            {
                return [];
            }

            if (Assemblies.Count == 0)
            {
                return [];
            }

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
            loadLock.Wait(); // where the dead lock happens when the script assembly had compiler errors.

            if (isInvalid)
            {
                return null;
            }

            if (Assemblies.Count == 0)
            {
                return null;
            }

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

        public static Type? GetTypeByGUID(Guid guid)
        {
            loadLock.Wait();

            if (isInvalid)
            {
                return null;
            }

            if (_typeCache.TryGetValue(guid, out var type))
            {
                return type;
            }

            return null;
        }

        public static void Unload()
        {
            loadLock.Reset();
            isInvalid = false;
            _typeCache.Clear();
            _typeToTypeCache.Clear();
            _typeNameCache.Clear();
            _enumCache.Clear();
            _enumNameCache.Clear();
            assemblies.Clear();
            assemblyLoadContext.Unload();
            assemblyLoadContext = new(nameof(ScriptAssemblyManager), true);
            AssembliesUnloaded?.Invoke(null, null);
        }
    }
}