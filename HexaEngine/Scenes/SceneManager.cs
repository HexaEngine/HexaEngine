namespace HexaEngine.Scenes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Windows;
    using HexaEngine.Editor;
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
        /// Read lock
        /// </summary>
        private static readonly object _lock = new();

        /// <summary>
        /// Write lock
        /// </summary>
        private static readonly SemaphoreSlim semaphore = new(1);

        /// <summary>
        /// Gets the current scene.
        /// </summary>
        /// <value>
        /// The current scene.
        /// </value>
        public static Scene? Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

        public static object SyncObject => _lock;

        /// <summary>
        /// Occurs when [scene changed].
        /// </summary>
        public static event EventHandler<SceneChangedEventArgs>? SceneChanged;

        public static void Lock()
        {
            semaphore.Wait();
        }

        public static void Unlock()
        {
            semaphore.Release();
        }

        public static void Save()
        {
            if (Current == null)
            {
                return;
            }

            if (Current.Path == null)
            {
                return;
            }

            lock (_lock)
            {
                if (SceneSerializer.TrySerialize(Current, Current.Path, out var ex))
                {
                    return;
                }
                else if (Application.InEditorMode)
                {
                    MessageBox.Show("Failed to save scene", ex.Message);
                    Logger.Log(ex);
                }
                else
                {
                    Logger.Throw(ex);
                }
            }
        }

        public static void Load(string path)
        {
            lock (_lock)
            {
                if (SceneSerializer.TryDeserialize(path, out var scene, out var ex))
                {
                    Load(scene);
                }
                else if (Application.InEditorMode)
                {
                    MessageBox.Show("Failed to load scene", ex.Message);
                    Logger.Log(ex);
                }
                else
                {
                    Logger.Throw(ex);
                }
            }
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
            SelectionCollection.Global.ClearSelection();
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(state =>
            {
                var values = (Tuple<ICoreWindow, Scene>)state;
                var window = values.Item1;
                var scene = values.Item2;

                semaphore.Wait();

                if (Current == null)
                {
                    scene.Initialize();
                    scene.Load(window.GraphicsDevice);
                    lock (_lock)
                    {
                        Current = scene;
                    }
                    SceneChanged?.Invoke(null, new(null, scene));
                    semaphore.Release();
                    return;
                }

                var old = Current;

                lock (_lock)
                {
                    Current = null;
                }

                if (scene == null)
                {
                    old?.Unload();
                    old?.Uninitialize();

                    ResourceManager.Shared.Release();

                    lock (_lock)
                    {
                        Current = null;
                    }
                }
                else
                {
                    old?.Unload();
                    old?.Uninitialize();

                    ResourceManager.Shared.Release();

                    scene.Initialize();

                    lock (_lock)
                    {
                        Current = scene;
                    }
                }

                semaphore.Release();

                SceneChanged?.Invoke(null, new(old, scene));

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }, new Tuple<ICoreWindow, Scene>(window, scene));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reload()
        {
            SelectionCollection.Global.ClearSelection();
            var window = Application.MainWindow;
            window.Dispatcher.InvokeBlocking(state =>
            {
                lock (_lock)
                {
                    if (Current == null)
                    {
                        return;
                    }
                }

                semaphore.Wait();

                var window = (ICoreWindow)state;
                ResourceManager.Shared.BeginNoGCRegion();

                Current.Unload();
                Current.Uninitialize();

                Current.Initialize();
                Current.Load(window.GraphicsDevice);

                SceneChanged?.Invoke(null, new(Current, Current));

                semaphore.Release();

                ResourceManager.Shared.EndNoGCRegion();

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }, window);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeginReload()
        {
            SelectionCollection.Global.ClearSelection();
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(() =>
            {
                lock (_lock)
                {
                    if (Current == null)
                    {
                        return;
                    }
                }

                semaphore.Wait();

                ResourceManager.Shared.BeginNoGCRegion();

                Current.Unload();
                Current.Uninitialize();

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                semaphore.Release();
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndReload()
        {
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(state =>
            {
                lock (_lock)
                {
                    if (Current == null)
                    {
                        return;
                    }
                }

                var window = (ICoreWindow)state;
                semaphore.Wait();

                Current.Initialize();
                Current.Load(window.GraphicsDevice);

                ResourceManager.Shared.EndNoGCRegion();
                SceneChanged?.Invoke(null, new(Current, Current));

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                semaphore.Release();
            }, window);
        }

        public static Task AsyncLoad(string path)
        {
            if (SceneSerializer.TryDeserialize(path, out var scene, out var ex))
            {
                return AsyncLoad(scene);
            }
            else if (Application.InEditorMode)
            {
                MessageBox.Show("Failed to load scene", ex.Message);
                Logger.Log(ex);
            }
            else
            {
                Logger.Throw(ex);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Asynchronous loads the scene over <see cref="Load(Scene)"/>
        /// </summary>
        /// <param dbgName="scene">The scene.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task AsyncLoad(Scene scene)
        {
            SelectionCollection.Global.ClearSelection();
            var window = Application.MainWindow;

            return Task.Factory.StartNew(async () =>
            {
                await window.Dispatcher.InvokeAsync(async state =>
                {
                    var values = (Tuple<ICoreWindow, Scene>)state;
                    var window = values.Item1;
                    var scene = values.Item2;

                    await semaphore.WaitAsync();

                    if (Current == null)
                    {
                        await scene.InitializeAsync();
                        scene.Load(window.GraphicsDevice);

                        lock (_lock)
                        {
                            Current = scene;
                        }

                        SceneChanged?.Invoke(null, new(null, scene));
                        semaphore.Release();
                        return;
                    }

                    var old = Current;

                    lock (_lock)
                    {
                        Current = null;
                    }

                    if (scene == null)
                    {
                        old?.Unload();
                        old?.Uninitialize();

                        ResourceManager.Shared.Release();
                        lock (_lock)
                        {
                            Current = null;
                        }
                    }
                    else
                    {
                        old?.Unload();
                        old?.Uninitialize();

                        ResourceManager.Shared.Release();

                        await scene.InitializeAsync();
                        scene.Load(window.GraphicsDevice);

                        lock (_lock)
                        {
                            Current = scene;
                        }
                    }

                    semaphore.Release();

                    SceneChanged?.Invoke(null, new(old, scene));

                    GC.WaitForPendingFinalizers();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                }, new Tuple<ICoreWindow, Scene>(window, scene));
            });
        }

        /// <summary>
        /// Unloads <see cref="Current"/> if != <see langword="null"/><br/>
        /// Sets <see cref="Current"/> <see langword="null"/><br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unload()
        {
            semaphore.Wait();

            Current?.Unload();
            Current?.Uninitialize();

            lock (_lock)
            {
                Current = null;
            }

            semaphore.Release();
        }
    }
}