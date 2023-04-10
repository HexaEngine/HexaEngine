#nullable disable

namespace HexaEngine.Core.Scenes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes.Serialization;
    using HexaEngine.Core.Windows;
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
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(state =>
            {
                var values = (Tuple<IRenderWindow, Scene>)state;
                var window = values.Item1;
                var scene = values.Item2;
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
            }, new Tuple<IRenderWindow, Scene>(window, scene));
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
            window.Dispatcher.InvokeBlocking(state =>
            {
                var values = (Tuple<IRenderWindow, Scene>)state;
                var window = values.Item1;
                var scene = values.Item2;
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
            }, new Tuple<IRenderWindow, Scene>(window, scene));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reload()
        {
            GameObject.Selected.ClearSelection();
            var window = Application.MainWindow;
            window.Dispatcher.InvokeBlocking(state =>
            {
                lock (Current)
                {
                    var window = (IRenderWindow)state;
                    ResourceManager.BeginPauseCleanup();
                    Current?.Uninitialize();
                    Current.Initialize(window.Device);
                    SceneChanged?.Invoke(null, new(Current, Current));
                    ResourceManager.EndPauseCleanup();
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }, window);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeginReload()
        {
            GameObject.Selected.ClearSelection();
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(() =>
            {
                lock (Current)
                {
                    ResourceManager.BeginPauseCleanup();
                    Current?.Uninitialize();
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndReload()
        {
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(state =>
            {
                var window = (IRenderWindow)state;
                lock (Current)
                {
                    Current.Initialize(window.Device);
                    ResourceManager.EndPauseCleanup();
                    SceneChanged?.Invoke(null, new(Current, Current));
                }
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }, window);
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
            var window = Application.MainWindow;

            await window.Dispatcher.InvokeAsync(async state =>
            {
                var values = (Tuple<IRenderWindow, Scene>)state;
                var window = values.Item1;
                var scene = values.Item2;
                if (Current == null)
                {
                    await scene.InitializeAsync(window.Device);
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
            }, new Tuple<IRenderWindow, Scene>(window, scene));
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
            await window.Dispatcher.InvokeAsync(async state =>
            {
                var values = (Tuple<IRenderWindow, Scene>)state;
                var window = values.Item1;
                var scene = values.Item2;
                if (Current == null)
                {
                    await scene.InitializeAsync(window.Device);
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
            }, new Tuple<IRenderWindow, Scene>(window, scene));
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