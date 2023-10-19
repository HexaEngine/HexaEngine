namespace HexaEngine.Editor.Plugins
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;

    public class PluginLoader
    {
        private readonly string path;
        private AssemblyLoadContext context;
        private readonly List<Assembly> assemblies = new();
        private readonly Dictionary<Type, IEnumerable<Type>> _cache = new();

        public PluginLoader(string path)
        {
            this.path = path;
            context = new AssemblyLoadContext(nameof(PluginLoader), true);
        }

        public void Load()
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(path, "*.dll"))
            {
                LoadAssembly(file);
            }
        }

        public Assembly? LoadAssembly(string path)
        {
            try
            {
                string? folder = Path.GetDirectoryName(path);
                string filename = Path.GetFileName(path);
                if (string.IsNullOrEmpty(folder))
                {
                    return null;
                }

                string pdb = Path.Combine(folder, Path.GetFileNameWithoutExtension(filename) + ".pdb");
                Assembly assembly;
                if (File.Exists(pdb))
                {
                    using MemoryStream ms = new(File.ReadAllBytes(path));
                    using MemoryStream ms2 = new(File.ReadAllBytes(pdb));
                    assembly = context.LoadFromStream(ms, ms2);
                }
                else
                {
                    using MemoryStream ms = new(File.ReadAllBytes(path));
                    assembly = context.LoadFromStream(ms);
                }

                assemblies.Add(assembly);
                Loaded?.Invoke(this, assembly);

                return assembly;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                MessageBox.Show("Failed to load plugin", ex.Message);
            }

            return null;
        }

        public void Unload()
        {
            _cache.Clear();
            assemblies.Clear();
            context.Unload();
            context = new(nameof(PluginLoader), true);
            Unloaded?.Invoke(this, EventArgs.Empty);
        }

        public static event EventHandler<Assembly>? Loaded;

        public static event EventHandler? Unloaded;

        public IEnumerable<Type> GetAssignableTypes<T>()
        {
            if (!_cache.TryGetValue(typeof(T), out var result))
            {
                result = assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(typeof(T))));
                _cache.Add(typeof(T), result);
            }
            return result;
        }

        public IEnumerable<Type> GetAssignableTypes(Type type)
        {
            if (!_cache.TryGetValue(type, out var result))
            {
                result = assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(type)));
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
    }
}