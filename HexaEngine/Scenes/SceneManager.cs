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
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(SceneManager));
        private static readonly object _lock = new();
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
        /// Occurs when <see cref="Current"/> changed.
        /// </summary>
        public static event EventHandler<SceneChangedEventArgs>? SceneChanged;

        /// <summary>
        /// Occurs when <see cref="Current"/> changing.
        /// </summary>
        public static event EventHandler<SceneChangingEventArgs>? SceneChanging;

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
                    Current.UnsavedChanged = false;
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
        /// Calls <see cref="IScene.Initialize"/> from <paramref name="scene"/><br/>
        /// Notifies <see cref="SceneChanged"/><br/>
        /// Forces the GC to Collect.<br/>
        /// </summary>
        /// <param name="scene">The scene.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Load(Scene? scene)
        {
            // Early exit nothing to do.
            if (Current == null && scene == null)
            {
                return;
            }

            if (OnSceneChanging(scene == null ? SceneChangeType.Unload : SceneChangeType.Load, scene))
            {
                return;
            }

            SelectionCollection.Global.ClearSelection();
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(state =>
            {
                (ICoreWindow window, Scene? scene) = state;
                IScene? iScene = scene;

                semaphore.Wait();

                if (Current == null && iScene != null)
                {
                    iScene.Initialize();
                    iScene.Load(window.GraphicsDevice);
                    lock (_lock)
                    {
                        Current = scene;
                    }
                    OnSceneChanged(SceneChangeType.Load, null, scene);
                    semaphore.Release();
                    return;
                }

                var oldScene = Current;
                IScene? iOldScene = oldScene;

                lock (_lock)
                {
                    Current = null;
                }

                if (iScene == null)
                {
                    if (iOldScene != null)
                    {
                        iOldScene.Unload();
                        iOldScene.Uninitialize();
                    }

                    ResourceManager.Shared.Release();

                    lock (_lock)
                    {
                        Current = null;
                    }

                    semaphore.Release();

                    OnSceneChanged(SceneChangeType.Unload, oldScene, scene);
                }
                else
                {
                    if (iOldScene != null)
                    {
                        iOldScene.Unload();
                        iOldScene.Uninitialize();
                    }

                    ResourceManager.Shared.Release();

                    iScene.Initialize();

                    lock (_lock)
                    {
                        Current = scene;
                    }

                    semaphore.Release();

                    OnSceneChanged(SceneChangeType.Load, oldScene, scene);
                }

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }, (window, scene));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Reload()
        {
            if (OnSceneChanging(SceneChangeType.Reload, Current))
            {
                return false;
            }

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

                ICoreWindow window = state;
                ResourceManager.Shared.BeginNoGCRegion();

                IScene scene = Current;

                scene.Unload();
                scene.Uninitialize();

                scene.Initialize();
                scene.Load(window.GraphicsDevice);

                OnSceneChanged(SceneChangeType.Reload, Current, Current);

                semaphore.Release();

                ResourceManager.Shared.EndNoGCRegion();

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }, window);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BeginReload()
        {
            if (OnSceneChanging(SceneChangeType.Reload, Current))
            {
                return false;
            }

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

                IScene scene = Current;

                scene.Unload();
                scene.Uninitialize();

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                semaphore.Release();
            });

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndReload()
        {
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(window =>
            {
                lock (_lock)
                {
                    if (Current == null)
                    {
                        return;
                    }
                }

                semaphore.Wait();

                IScene scene = Current;

                scene.Initialize();
                scene.Load(window.GraphicsDevice);

                ResourceManager.Shared.EndNoGCRegion();
                OnSceneChanged(SceneChangeType.Reload, Current, Current);

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
        /// <param name="scene">The scene.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task AsyncLoad(Scene? scene)
        {
            // Early exit nothing to do.
            if (Current == null && scene == null)
            {
                return Task.CompletedTask;
            }

            if (OnSceneChanging(scene == null ? SceneChangeType.Unload : SceneChangeType.Load, scene))
            {
                return Task.CompletedTask;
            }

            SelectionCollection.Global.ClearSelection(); // clear selection to prevent memory leaks since it holds references.
            var window = Application.MainWindow;

            return Task.Factory.StartNew(async () =>
            {
                await window.Dispatcher.InvokeAsync(async state =>
                {
                    (ICoreWindow window, Scene? scene) = state;
                    IScene? iScene = scene;

                    await semaphore.WaitAsync();

                    if (Current == null && iScene != null)
                    {
                        await iScene.InitializeAsync();
                        iScene.Load(window.GraphicsDevice);

                        lock (_lock)
                        {
                            Current = scene;
                        }

                        OnSceneChanged(SceneChangeType.Load, null, scene);
                        semaphore.Release();
                        return;
                    }

                    var oldScene = Current;
                    IScene? iOldScene = oldScene;

                    lock (_lock)
                    {
                        Current = null;
                    }

                    if (iScene == null)
                    {
                        if (iOldScene != null)
                        {
                            iOldScene.Unload();
                            iOldScene.Uninitialize();
                        }

                        ResourceManager.Shared.Release();

                        lock (_lock)
                        {
                            Current = null;
                        }

                        semaphore.Release();

                        OnSceneChanged(SceneChangeType.Unload, oldScene, scene);
                    }
                    else
                    {
                        if (iOldScene != null)
                        {
                            iOldScene.Unload();
                            iOldScene.Uninitialize();
                        }

                        ResourceManager.Shared.Release();

                        await iScene.InitializeAsync();
                        iScene.Load(window.GraphicsDevice);

                        lock (_lock)
                        {
                            Current = scene;
                        }

                        semaphore.Release();

                        OnSceneChanged(SceneChangeType.Load, oldScene, scene);
                    }

                    GC.WaitForPendingFinalizers();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                }, (window, scene));
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

            IScene? scene = Current;

            if (scene == null)
            {
                semaphore.Release();
                return;
            }

            scene.Unload();
            scene.Uninitialize();

            lock (_lock)
            {
                Current = null;
            }

            semaphore.Release();
        }

        private static bool OnSceneChanging(SceneChangeType changeType, Scene? newScene)
        {
            SceneChangingEventArgs sceneChangingEventArgs = new(changeType, Current, newScene);
            SceneChanging?.Invoke(null, sceneChangingEventArgs);
            return sceneChangingEventArgs.Handled;
        }

        private static void OnSceneChanged(SceneChangeType changeType, Scene? oldScene, Scene? newScene)
        {
            SceneChangedEventArgs sceneChangedEventArgs = new(changeType, oldScene, newScene);
            SceneChanged?.Invoke(null, sceneChangedEventArgs);
        }
    }
}