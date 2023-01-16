#nullable disable

namespace HexaEngine.Scenes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Windows;
    using HexaEngine.Resources;
    using HexaEngine.Scenes.Serialization;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains the <see cref="Current"/> scene and informs about a change (<see cref="SceneChanged"/>) of <see cref="Current"/>
    /// </summary>
    public static class SceneManager
    {
        /// <summary>
        /// Gets the current scene.
        /// </summary>
        /// <value>
        /// The current scene.
        /// </value>
        public static Scene Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

        /// <summary>
        /// Occurs when [scene changed].
        /// </summary>
        public static event EventHandler<SceneChangedEventArgs> SceneChanged;

        public static void Save()
        {
            if (Current == null) return;
            if (Current.Path == null) return;
            SceneSerializer.Serialize(Current, Current.Path);
        }

        public static void Load(string path)
        {
            Load(SceneSerializer.Deserialize(path));
        }

        public static void Load(IRenderWindow window, string path)
        {
            Load(window, SceneSerializer.Deserialize(path));
        }

        /// <summary>
        /// Loads the specified scene and disposes the old Scene automatically.<br/>
        /// Calls <see cref="Scene.Initialize"/> from <paramref dbgName="scene"/><br/>
        /// Calls <see cref="Scene.Dispose"/> if <see cref="Current"/> != <see langword="null"/><br/>
        /// Notifies <see cref="SceneChanged"/><br/>
        /// Forces the GC to Collect.<br/>
        /// </summary>
        /// <param dbgName="scene">The scene.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Load(Scene scene)
        {
            GameObject.Selected.ClearSelection();
            var window = Application.MainWindow as IRenderWindow;

            window.Dispatcher.InvokeBlocking(() =>
            {
                if (Current == null)
                {
                    scene.Initialize(window.Device);
                    Current = scene;
                    SceneChanged?.Invoke(null, new(null, scene));
                    return;
                }
                lock (Current)
                {
                    var old = Current;
                    Current = null;
                    if (scene == null)
                    {
                        old?.Uninitialize();
                        ResourceManager.Release();
                        Current = null;
                    }
                    else
                    {
                        old?.Uninitialize();
                        ResourceManager.Release();
                        scene.Initialize(window.Device);
                        Current = scene;
                    }

                    SceneChanged?.Invoke(null, new(old, scene));
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        /// <summary>
        /// Loads the specified scene and disposes the old Scene automatically.<br/>
        /// Calls <see cref="Scene.Initialize"/> from <paramref dbgName="scene"/><br/>
        /// Calls <see cref="Scene.Dispose"/> if <see cref="Current"/> != <see langword="null"/><br/>
        /// Notifies <see cref="SceneChanged"/><br/>
        /// Forces the GC to Collect.<br/>
        /// </summary>
        /// <param dbgName="scene">The scene.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Load(IRenderWindow window, Scene scene)
        {
            GameObject.Selected.ClearSelection();
            window.Dispatcher.InvokeBlocking(() =>
            {
                if (Current == null)
                {
                    scene.Initialize(window.Device);
                    Current = scene;
                    SceneChanged?.Invoke(null, new(null, scene));
                    return;
                }
                lock (Current)
                {
                    var old = Current;
                    Current = null;
                    if (scene == null)
                    {
                        old?.Uninitialize();
                        ResourceManager.Release();
                        Current = null;
                    }
                    else
                    {
                        old?.Uninitialize();
                        ResourceManager.Release();
                        scene.Initialize(window.Device);
                        Current = scene;
                    }

                    SceneChanged?.Invoke(null, new(old, scene));
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reload()
        {
            GameObject.Selected.ClearSelection();
            var window = Application.MainWindow as IRenderWindow;

            window.Dispatcher.InvokeBlocking(() =>
            {
                lock (Current)
                {
                    Current?.Uninitialize();
                    Current.Initialize(window.Device);
                    SceneChanged?.Invoke(null, new(Current, Current));
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeginReload()
        {
            GameObject.Selected.ClearSelection();
            var window = Application.MainWindow as IRenderWindow;

            window.Dispatcher.InvokeBlocking(() =>
            {
                lock (Current)
                {
                    Current?.Uninitialize();
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndReload()
        {
            var window = Application.MainWindow as IRenderWindow;

            window.Dispatcher.InvokeBlocking(() =>
            {
                lock (Current)
                {
                    Current.Initialize(window.Device);
                    SceneChanged?.Invoke(null, new(Current, Current));
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        public static async Task AsyncLoad(string path)
        {
            await AsyncLoad(SceneSerializer.Deserialize(path));
        }

        public static async Task AsyncLoad(IRenderWindow window, string path)
        {
            await AsyncLoad(window, SceneSerializer.Deserialize(path));
        }

        /// <summary>
        /// Asynchronouses loads the scene over <see cref="Load(Scene)"/>
        /// </summary>
        /// <param dbgName="scene">The scene.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task AsyncLoad(Scene scene)
        {
            GameObject.Selected.ClearSelection();
            var window = Application.MainWindow as IRenderWindow;

            await window.Dispatcher.InvokeAsync(() =>
            {
                if (Current == null)
                {
                    scene.Initialize(window.Device);
                    Current = scene;
                    SceneChanged?.Invoke(null, new(null, scene));
                    return;
                }
                lock (Current)
                {
                    var old = Current;
                    Current = null;
                    if (scene == null)
                    {
                        old?.Uninitialize();
                        ResourceManager.Release();
                        Current = null;
                    }
                    else
                    {
                        old?.Uninitialize();
                        ResourceManager.Release();
                        scene.Initialize(window.Device);
                        Current = scene;
                    }

                    SceneChanged?.Invoke(null, new(old, scene));
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        /// <summary>
        /// Asynchronouses loads the scene over <see cref="Load(Scene)"/>
        /// </summary>
        /// <param dbgName="scene">The scene.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task AsyncLoad(IRenderWindow window, Scene scene)
        {
            GameObject.Selected.ClearSelection();
            await window.Dispatcher.InvokeAsync(() =>
            {
                if (Current == null)
                {
                    scene.Initialize(window.Device);
                    Current = scene;
                    SceneChanged?.Invoke(null, new(null, scene));
                    return;
                }
                lock (Current)
                {
                    var old = Current;
                    Current = null;
                    if (scene == null)
                    {
                        old?.Uninitialize();
                        ResourceManager.Release();
                        Current = null;
                    }
                    else
                    {
                        old?.Uninitialize();
                        ResourceManager.Release();
                        scene.Initialize(window.Device);
                        Current = scene;
                    }

                    SceneChanged?.Invoke(null, new(old, scene));
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        /// <summary>
        /// Unloads <see cref="Current"/> if != <see langword="null"/><br/>
        /// Sets <see cref="Current"/> <see langword="null"/><br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unload()
        {
            Current?.Uninitialize();
            Current = null;
        }
    }
}