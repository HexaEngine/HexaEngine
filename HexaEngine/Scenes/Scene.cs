namespace HexaEngine.Scenes
{
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Lights;
    using HexaEngine.Queries;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Scenes.Systems;
    using HexaEngine.Weather;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class Scene
    {
        private readonly FlaggedList<SystemFlags, ISystem> systems = new();
        private readonly List<GameObject> nodes = new();
        private readonly List<Camera> cameras = new();
        private readonly List<string> cameraNames = new();
        private readonly ScriptManager scriptManager = new();
        private ModelManager meshManager;
        private LightManager lightManager;
        private RenderManager renderManager;
        private AnimationManager animationManager = new();
        private MaterialManager materialManager = new();
        private WeatherManager weatherManager = new();
        private QueryManager queryManager;

        private readonly SemaphoreSlim semaphore = new(1);
        private string? path;

        private readonly GameObject root;

        public int ActiveCamera;

#pragma warning disable CS8618 // Non-nullable field 'Simulation' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public Scene()
#pragma warning restore CS8618 // Non-nullable field 'Simulation' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
            Name = "Scene";
            root = new SceneRootNode(this);
        }

        public string Name { get; }

        [JsonIgnore]
        public string? Path { get => path; set => path = value; }

        [JsonIgnore]
        public SceneDispatcher Dispatcher { get; } = new();

        [JsonIgnore]
        public List<Camera> Cameras => cameras;

        [JsonIgnore]
        public string[] CameraNames => cameraNames.GetInternalArray();

        [JsonIgnore]
        public LightManager LightManager => lightManager;

        [JsonIgnore]
        public List<GameObject> GameObjects => nodes;

        [JsonIgnore]
        public Camera? CurrentCamera => ActiveCamera >= 0 && ActiveCamera < cameras.Count ? cameras[ActiveCamera] : null;

        [JsonIgnore]
        public ScriptManager Scripts => scriptManager;

        [JsonIgnore]
        public RenderManager RenderManager => renderManager;

        [JsonIgnore]
        public MaterialManager MaterialManager => materialManager;

        [JsonIgnore]
        public WeatherManager WeatherManager => weatherManager;

        [JsonIgnore]
        public FlaggedList<SystemFlags, ISystem> Systems => systems;

        [JsonIgnore]
        public ModelManager ModelManager { get => meshManager; set => meshManager = value; }

        [JsonIgnore]
        public AnimationManager AnimationManager { get => animationManager; set => animationManager = value; }

        [JsonIgnore]
        public QueryManager QueryManager { get => queryManager; set => queryManager = value; }

        [JsonIgnore]
        public SceneProfiler Profiler { get; } = new(10);

        public SceneVariables Variables { get; } = new();

        public event Action<GameObject>? OnGameObjectAdded;

        public event Action<GameObject>? OnGameObjectRemoved;

        public GameObject Root => root;

        public bool IsSimulating;

        public void Initialize(IGraphicsDevice device)
        {
            queryManager = new(this);
            lightManager = new();
            meshManager ??= new();
            lightManager.Initialize(device).Wait();
            systems.Add(new AudioSystem());
            systems.Add(new AnimationSystem(this));
            systems.Add(scriptManager);
            systems.Add(lightManager);
            systems.Add(new PhysicsSystem());
            systems.Add(new TransformSystem());
            systems.Add(renderManager = new(device, lightManager));
            systems.Add(weatherManager);

            semaphore.Wait();

            Time.FixedUpdate += FixedUpdate;
            Time.Initialize();

            root.Initialize(device);
            Validate();
            semaphore.Release();

            var awake = systems[SystemFlags.Awake];
            for (int i = 0; i < awake.Count; i++)
            {
                awake[i].Awake(this);
            }
        }

        public async Task InitializeAsync(IGraphicsDevice device)
        {
            queryManager = new(this);
            lightManager = new();
            meshManager ??= new();
            await lightManager.Initialize(device);
            systems.Add(new AudioSystem());
            systems.Add(new AnimationSystem(this));
            systems.Add(scriptManager);
            systems.Add(lightManager);
            systems.Add(new PhysicsSystem());
            systems.Add(new TransformSystem());
            systems.Add(renderManager = new(device, lightManager));
            systems.Add(weatherManager);

            await semaphore.WaitAsync();

            Time.FixedUpdate += FixedUpdate;
            Time.Initialize();

            root.Initialize(device);
            Validate();
            semaphore.Release();

            var awake = systems[SystemFlags.Awake];
            for (int i = 0; i < awake.Count; i++)
            {
                awake[i].Awake(this);
            }
        }

        public bool Validate()
        {
            foreach (var node in nodes)
            {
                if (nodes.Any(x => node != x && x.Name == x.Name))
                {
                    return false;
                }
            }
            return true;
        }

        public void SaveState()
        {
            root.SaveState();
        }

        public void RestoreState()
        {
            root.RestoreState();
        }

        public void GraphicsUpdate(IGraphicsContext context)
        {
            var render = systems[SystemFlags.GraphicsUpdate];
            for (int i = 0; i < render.Count; i++)
            {
#if PROFILE
                Profiler.Start(render[i]);
#endif
                render[i].Update(Time.Delta);
#if PROFILE
                Profiler.End(render[i]);
#endif
            }
        }

        public void Update()
        {
            Profiler.Clear();
            semaphore.Wait();
            CameraManager.Update();

            Dispatcher.ExecuteInvokes();
            semaphore.Release();

            var early = systems[SystemFlags.Update];

#if PROFILE
            Profiler.Start(systems);
#endif
            for (int i = 0; i < early.Count; i++)
            {
#if PROFILE
                Profiler.Start(early[i]);
#endif
                early[i].Update(Time.Delta);
#if PROFILE
                Profiler.End(early[i]);
#endif
            }

            var late = systems[SystemFlags.LateUpdate];

            for (int i = 0; i < late.Count; i++)
            {
#if PROFILE
                Profiler.Start(late[i]);
#endif
                late[i].Update(Time.Delta);
#if PROFILE
                Profiler.End(late[i]);
#endif
            }
#if PROFILE
            Profiler.End(systems);
#endif
        }

        private void FixedUpdate(object? sender, EventArgs e)
        {
            semaphore.Wait();

            var physics = systems[SystemFlags.PhysicsUpdate];

            for (int i = 0; i < physics.Count; i++)
            {
#if PROFILE
                Profiler.Start(physics[i]);
#endif
                physics[i].Update(Time.FixedDelta);
#if PROFILE
                Profiler.End(physics[i]);
#endif
            }

#if PROFILE
            Profiler.Start(systems);
#endif
            var fixedUpdate = systems[SystemFlags.FixedUpdate];

            for (int i = 0; i < fixedUpdate.Count; i++)
            {
#if PROFILE
                Profiler.Start(fixedUpdate[i]);
#endif
                fixedUpdate[i].Update(Time.Delta);
#if PROFILE
                Profiler.End(fixedUpdate[i]);
#endif
            }
#if PROFILE
            Profiler.End(systems);
#endif
            semaphore.Release();
        }

        /// <summary>
        /// Searches an GameObject by it's Guid.
        /// </summary>
        /// <param name="guid">The unique identifier of the GameObject.</param>
        /// <returns>The found GameObject or null, if the name was not found.</returns>
        public GameObject? FindByGuid(Guid? guid)
        {
            if (guid == null)
            {
                return null;
            }

            semaphore.Wait();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Guid == guid.Value)
                {
                    semaphore.Release();
                    return nodes[i];
                }
            }
            semaphore.Release();
            return null;
        }

        /// <summary>
        /// Searches an GameObject by it's name. NOTE: Names are not unique. Use Guid lookup or FullName lookups to find a specific GameObject.
        /// </summary>
        /// <param name="name">The name of the GameObject.</param>
        /// <returns>The found GameObject or null, if the name was not found.</returns>
        public GameObject? FindByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            semaphore.Wait();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name == name)
                {
                    semaphore.Release();
                    return nodes[i];
                }
            }
            semaphore.Release();
            return null;
        }

        /// <summary>
        /// Searches an GameObject by it's full name.
        /// </summary>
        /// <param name="name">The full name of the GameObject.</param>
        /// <returns>The found GameObject or null, if the name was not found.</returns>
        public GameObject? FindByFullName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            semaphore.Wait();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].FullName == name)
                {
                    semaphore.Release();
                    return nodes[i];
                }
            }
            semaphore.Release();
            return null;
        }

        public GameObject? FindByTag(object? tag)
        {
            if (tag == null)
            {
                return null;
            }

            semaphore.Wait();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Tag == tag)
                {
                    semaphore.Release();
                    return nodes[i];
                }
            }
            semaphore.Release();
            return null;
        }

        public IEnumerable<GameObject> FindAllByName(string? name)
        {
            if (name == null)
            {
                yield break;
            }

            semaphore.Wait();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Name == name)
                {
                    yield return nodes[i];
                }
            }
            semaphore.Release();
        }

        public IEnumerable<GameObject> FindAllByTag(object? tag)
        {
            if (tag == null)
            {
                yield break;
            }

            semaphore.Wait();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Tag == tag)
                {
                    yield return nodes[i];
                }
            }
            semaphore.Release();
        }

        public string GetAvailableName(string name)
        {
            return GetNewName(name);
        }

        private string GetNewName(string name)
        {
            if (FindByName(name) == null)
            {
                return name;
            }

            string result = name;

            int i = 0;
            while (FindByName(result) != null)
            {
                i++;
                result = $"{name} {i}";
            }

            return result;
        }

        internal void RegisterChild(GameObject node)
        {
            nodes.Add(node);
            if (cameras.AddIfIs(node))
            {
                cameraNames.Add(node.Name);
            }

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Register(node);
            }

            OnGameObjectAdded?.Invoke(node);
        }

        internal void UnregisterChild(GameObject node)
        {
            nodes.Remove(node);
            if (cameras.RemoveIfIs(node))
            {
                cameraNames.Remove(node.Name);
            }

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Unregister(node);
            }

            OnGameObjectRemoved?.Invoke(node);
        }

        private class SceneRootNode : GameObject
        {
            private readonly Scene parent;

            public SceneRootNode(Scene parent)
            {
                this.parent = parent;
            }

            public override Scene GetScene()
            {
                return parent;
            }

            public override void GetDepth(ref int depth)
            {
                return;
            }
        }

        public void AddChild(GameObject node)
        {
            semaphore.Wait();
            root.AddChild(node);
            semaphore.Release();
        }

        public void RemoveChild(GameObject node)
        {
            semaphore.Wait();
            root.RemoveChild(node);
            semaphore.Release();
        }

        public void BuildReferences()
        {
            root.BuildReferences();
        }

        public void Uninitialize()
        {
            semaphore.Wait();
            Time.FixedUpdate -= FixedUpdate;
            root.Uninitialize();
            lightManager.Dispose();
            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Destroy();
            }
            systems.Clear();
            semaphore.Release();
        }

        public IEnumerable<GameObject> GetRange(GameObject start, GameObject end)
        {
            bool collect = false;
            for (int i = 0; i < nodes.Count; i++)
            {
                var current = nodes[i];
                if (current == start || current == end)
                {
                    if (!collect)
                    {
                        collect = true;
                    }
                    else
                    {
                        yield return current;
                        break;
                    }
                }
                if (collect && current.IsEditorVisible)
                {
                    yield return current;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetSystem<T>() where T : class, ISystem
        {
            for (int i = 0; i < systems.Count; i++)
            {
                var system = systems[i];
                if (system is T t)
                {
                    return t;
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSystem<T>([NotNullWhen(true)] out T? system) where T : class, ISystem
        {
            system = GetSystem<T>();
            return system != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetRequiredSystem<T>() where T : class, ISystem
        {
            var sys = GetSystem<T>();
            if (sys == null)
                throw new NullReferenceException(nameof(sys));
            return sys;
        }
    }
}