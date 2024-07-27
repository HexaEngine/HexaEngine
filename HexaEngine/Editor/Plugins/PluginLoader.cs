namespace HexaEngine.Editor.Plugins
{
    using HexaEngine.Core.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;

    /// <summary>
    /// Represents a utility for loading and managing plugins from assemblies.
    /// </summary>
    public class PluginLoader
    {
        private readonly string path;
        private AssemblyLoadContext context;
        private readonly List<Assembly> assemblies = new();
        private readonly Dictionary<Type, IEnumerable<Type>> _cache = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoader"/> class with the specified path.
        /// </summary>
        /// <param name="path">The directory path where plugin assemblies are located.</param>
        public PluginLoader(string path)
        {
            this.path = path;
            context = new AssemblyLoadContext(nameof(PluginLoader), true);
        }

        /// <summary>
        /// Loads plugins from the specified directory path.
        /// </summary>
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

        /// <summary>
        /// Loads an assembly from the specified file path.
        /// </summary>
        /// <param name="path">The file path of the assembly to load.</param>
        /// <returns>The loaded assembly, or <c>null</c> if loading fails.</returns>
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
                PluginManager.Logger.Log(ex);
                MessageBox.Show("Failed to load plugin", ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Unloads loaded assemblies and resets the type cache.
        /// </summary>
        public void Unload()
        {
            _cache.Clear();
            assemblies.Clear();
            context.Unload();
            context = new(nameof(PluginLoader), true);
            Unloaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event that occurs when an assembly is loaded.
        /// </summary>
        public static event EventHandler<Assembly>? Loaded;

        /// <summary>
        /// Event that occurs when the plugin loader is unloaded.
        /// </summary>
        public static event EventHandler? Unloaded;

        /// <summary>
        /// Gets types in loaded assemblies that are assignable to the specified generic type parameter.
        /// </summary>
        /// <typeparam name="T">The generic type parameter.</typeparam>
        /// <returns>An IEnumerable of types that are assignable to the specified generic type parameter.</returns>
        public IEnumerable<Type> GetAssignableTypes<T>()
        {
            if (!_cache.TryGetValue(typeof(T), out var result))
            {
                result = assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(typeof(T))));
                _cache.Add(typeof(T), result);
            }
            return result;
        }

        /// <summary>
        /// Gets types in loaded assemblies that are assignable to the specified type.
        /// </summary>
        /// <param name="type">The type to check assignability against.</param>
        /// <returns>An IEnumerable of types that are assignable to the specified type.</returns>
        public IEnumerable<Type> GetAssignableTypes(Type type)
        {
            if (!_cache.TryGetValue(type, out var result))
            {
                result = assemblies.SelectMany(assembly => assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(type)));
                _cache.Add(type, result);
            }
            return result;
        }

        /// <summary>
        /// Gets types in the specified assembly that are assignable to the specified generic type parameter.
        /// </summary>
        /// <typeparam name="T">The generic type parameter.</typeparam>
        /// <param name="assembly">The assembly to search for types.</param>
        /// <returns>An IEnumerable of types that are assignable to the specified generic type parameter.</returns>
        public static IEnumerable<Type> GetAssignableTypes<T>(Assembly assembly)
        {
            return assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(typeof(T)));
        }

        /// <summary>
        /// Gets types in the specified assembly that are assignable to the specified type.
        /// </summary>
        /// <param name="assembly">The assembly to search for types.</param>
        /// <param name="type">The type to check assignability against.</param>
        /// <returns>An IEnumerable of types that are assignable to the specified type.</returns>
        public static IEnumerable<Type> GetAssignableTypes(Assembly assembly, Type type)
        {
            return assembly.GetTypes().AsParallel().Where(x => x.IsAssignableTo(type));
        }
    }
}