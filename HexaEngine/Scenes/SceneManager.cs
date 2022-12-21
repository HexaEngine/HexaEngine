#nullable disable

namespace HexaEngine.Scenes
{
    using HexaEngine.Core;
    using HexaEngine.Resources;
    using HexaEngine.Windows;
    using Silk.NET.Assimp;
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
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

        /// <summary>
        /// Loads the specified scene and disposes the old Scene automatically.<br/>
        /// Calls <see cref="Scene.Initialize"/> from <paramref name="scene"/><br/>
        /// Calls <see cref="Scene.Dispose"/> if <see cref="Current"/> != <see langword="null"/><br/>
        /// Notifies <see cref="SceneChanged"/><br/>
        /// Forces the GC to Collect.<br/>
        /// </summary>
        /// <param name="scene">The scene.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Load(Scene scene)
        {
            var window = Application.MainWindow as Window;

            window.RenderDispatcher.InvokeBlocking(() =>
            {
                scene.Initialize(window.Device);
                if (Current == null)
                {
                    Current = scene;
                    SceneChanged?.Invoke(null, new(null, scene));
                    return;
                }
                lock (Current)
                {
                    var old = Current;
                    if (scene == null)
                    {
                        Current?.Uninitialize();
                        Current = null;
                        ResourceManager.Release();
                    }
                    else
                    {
                        ResourceManager.BeginPauseCleanup();
                        Current?.Uninitialize();
                        Current = scene;
                        ResourceManager.EndPauseCleanup();
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
            var window = Application.MainWindow as Window;

            window.RenderDispatcher.InvokeBlocking(() =>
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
            var window = Application.MainWindow as Window;

            window.RenderDispatcher.InvokeBlocking(() =>
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
            var window = Application.MainWindow as Window;

            window.RenderDispatcher.InvokeBlocking(() =>
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

        /// <summary>
        /// Asynchronouses loads the scene over <see cref="Load(Scene)"/>
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task AsyncLoad(Scene scene)
        {
            var window = Application.MainWindow as Window;

            await window.RenderDispatcher.InvokeAsync(() =>
            {
                scene.Initialize(window.Device);
                if (Current == null)
                {
                    Current = scene;
                    SceneChanged?.Invoke(null, new(null, scene));
                    return;
                }
                lock (Current)
                {
                    var old = Current;
                    Current?.Uninitialize();
                    Current = scene;
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
        internal static void Unload()
        {
            Current?.Uninitialize();
            Current = null;
        }
    }
}