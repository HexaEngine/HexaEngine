namespace HexaEngine.Scenes
{
    using Hexa.NET.Logging;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Windows;
    using HexaEngine.Editor;
    using HexaEngine.Lights;
    using HexaEngine.Resources;
    using HexaEngine.Scenes.Serialization;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class SceneManager
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(SceneManager));
        private static readonly object _lock = new();
        private static readonly SemaphoreSlim semaphore = new(1);

        public static Scene? Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

        public static object SyncObject => _lock;

        public static event EventHandler<SceneChangedEventArgs>? SceneChanged;

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
                if (Current.IsPrefabScene)
                {
                    SavePrefab(Current, new(Current), Current.Path);
                }
                else
                {
                    SaveScene(Current, Current.Path);
                }
            }
        }

        private static void SavePrefab(Scene scene, Prefab prefab, string path)
        {
            if (PrefabSerializer.TrySerialize(prefab, path, out var ex))
            {
                scene.UnsavedChanged = false;
                SourceAssetsDatabase.Update(path, false);
                return;
            }
            else if (Application.InEditorMode)
            {
                MessageBox.Show("Failed to save prefab", ex.Message);
                Logger.Log(ex);
            }
            else
            {
                Logger.Throw(ex);
            }
        }

        private static void SaveScene(Scene scene, string path)
        {
            if (SceneSerializer.TrySerialize(scene, path, out var ex))
            {
                scene.UnsavedChanged = false;
                SourceAssetsDatabase.Update(path, false);
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

        public static void Load(string path, SceneInitFlags initFlags)
        {
            lock (_lock)
            {
                if (SceneSerializer.TryDeserialize(path, out var scene, out var ex))
                {
                    Load(scene, initFlags);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Load(Scene? scene, SceneInitFlags initFlags)
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
                (ICoreWindow window, Scene? scene, SceneInitFlags flags) = state;
                IScene? iScene = scene;

                semaphore.Wait();

                if (Current == null && iScene != null)
                {
                    iScene.Initialize(flags);
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

                    iScene.Initialize(flags);

                    lock (_lock)
                    {
                        Current = scene;
                    }

                    semaphore.Release();

                    OnSceneChanged(SceneChangeType.Load, oldScene, scene);
                }

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }, (window, scene, initFlags));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Reload(SceneInitFlags initFlags)
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
                    if (Current == null || Current.Path == null)
                    {
                        return;
                    }

                    if (!SceneSerializer.TryDeserialize(Current.Path, out var scene, out var ex))
                    {
                        if (Application.InEditorMode)
                        {
                            MessageBox.Show("Failed to load scene", ex.Message);
                            Logger.Log(ex);
                        }
                        else
                        {
                            Logger.Throw(ex);
                        }
                    }
                    else
                    {
                        Current = scene;
                    }
                }

                semaphore.Wait();

                (ICoreWindow window, SceneInitFlags initFlags) = state;
                ResourceManager.Shared.BeginNoGCRegion();

                IScene iScene = Current;

                iScene.Unload();
                iScene.Uninitialize();

                iScene.Initialize(initFlags);
                iScene.Load(window.GraphicsDevice);

                OnSceneChanged(SceneChangeType.Reload, Current, Current);

                semaphore.Release();

                ResourceManager.Shared.EndNoGCRegion();

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }, (window, initFlags));

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
        public static void EndReload(SceneInitFlags initFlags)
        {
            var window = Application.MainWindow;

            window.Dispatcher.InvokeBlocking(state =>
            {
                lock (_lock)
                {
                    if (Current == null || Current.Path == null)
                    {
                        return;
                    }

                    if (!SceneSerializer.TryDeserialize(Current.Path, out var scene, out var ex))
                    {
                        if (Application.InEditorMode)
                        {
                            MessageBox.Show("Failed to load scene", ex.Message);
                            Logger.Log(ex);
                        }
                        else
                        {
                            Logger.Throw(ex);
                        }
                    }
                    else
                    {
                        Current = scene;
                    }
                }

                (ICoreWindow window, SceneInitFlags flags) = state;

                semaphore.Wait();

                IScene iScene = Current;

                iScene.Initialize(flags);
                iScene.Load(window.GraphicsDevice);

                ResourceManager.Shared.EndNoGCRegion();
                OnSceneChanged(SceneChangeType.Reload, Current, Current);

                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                semaphore.Release();
            }, (window, initFlags));
        }

        public static Task AsyncLoadPrefab(string path, SceneInitFlags initFlags)
        {
            if (PrefabSerializer.TryDeserialize(path, out var prefab, out var ex))
            {
                return AsyncLoad(prefab.ToScene(), initFlags);
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

        public static Task AsyncLoad(string path, SceneInitFlags initFlags)
        {
            if (SceneSerializer.TryDeserialize(path, out var scene, out var ex))
            {
                return AsyncLoad(scene, initFlags);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task AsyncLoad(Scene? scene, SceneInitFlags initFlags)
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
                    (ICoreWindow window, Scene? scene, SceneInitFlags flags) = state;
                    IScene? iScene = scene;

                    await semaphore.WaitAsync();

                    if (Current == null && iScene != null)
                    {
                        await iScene.InitializeAsync(flags);
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

                        await iScene.InitializeAsync(flags);
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
                }, (window, scene, initFlags));
            });
        }

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

        internal static void Shutdown()
        {
            LightManager.Shutdown();
        }
    }
}