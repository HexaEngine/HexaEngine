namespace HexaEngine.Editor.Projects
{
    using HexaEngine.Scenes;
    using HexaEngine.Scripts;
    using HexaEngine.Windows;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    public static class ComponentRegistry
    {
        private static readonly List<Type> _components = new();

        static ComponentRegistry()
        {
            Scripts = new();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Assembly assembly = Assembly.GetAssembly(typeof(Window));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _components = new(assembly.GetTypes().AsParallel().Where(x => x.IsClass && !x.IsGenericType && x.GetInterface(nameof(IComponent)) != null));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Components = new((IEnumerable<Type>)_components);
            ScriptAssemblyManager.AssemblyLoaded += AssemblyLoaded;
            ScriptAssemblyManager.AssembliesUnloaded += AssembliesUnloaded;
        }

        private static void AssembliesUnloaded(object? sender, EventArgs? e)
        {
            Components.Clear();
            Scripts.Clear();
        }

        private static void AssemblyLoaded(object? sender, Assembly e)
        {
            Assembly assembly = e;
            foreach (var type in assembly.GetTypes().AsParallel().Where(x => x.IsClass && !x.IsGenericType && x.GetInterface(nameof(IComponent)) != null))
            {
                Components.Add(type);
            }

            foreach (var type in ScriptAssemblyManager.GetAssignableTypes<IScriptBehaviour>(assembly))
            {
                Scripts.Add(type);
            }
        }

        public static ObservableCollection<Type> Components { get; private set; }

        public static ObservableCollection<Type> Scripts { get; private set; }

        public static void RegisterAssembly(string path)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Assembly assembly = ScriptAssemblyManager.Load(path);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var type in assembly.GetTypes().AsParallel().Where(x => x.IsClass && !x.IsGenericType && x.GetInterface(nameof(IComponent)) != null))
            {
                Components.Add(type);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            foreach (var type in ScriptAssemblyManager.GetAssignableTypes<IScriptBehaviour>(assembly))
            {
                Scripts.Add(type);
            }
        }

        public static IEnumerable<Type> GetAssignableTypes<T>()
        {
            return ScriptAssemblyManager.GetAssignableTypes<T>();
        }
    }
}