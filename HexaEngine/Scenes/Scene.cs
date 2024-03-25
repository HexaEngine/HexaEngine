namespace HexaEngine.Scenes
{
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Jobs;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Queries;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Weather;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class Scene
    {
        private readonly FlaggedList<SystemFlags, ISceneSystem> systems = [];
        private readonly List<GameObject> nodes = [];
        private readonly CameraContainer cameraContainer = new();

        private readonly IServiceProvider serviceProvider;
        private readonly IServiceCollection services;

        private readonly SemaphoreSlim semaphore = new(1);

        private readonly GameObject root;
        private readonly SceneInitFlags initFlags;

        private string? path;
        private SceneFlags flags;

        public Scene()
        {
            Name = "Scene";
            root = new SceneRootNode(this);

            services = SceneSystemRegistry.GetServices(this);
            serviceProvider = services.BuildServiceProvider();
        }

        public Scene(SceneInitFlags flags)
        {
            Name = "Scene";
            root = new SceneRootNode(this);

            services = SceneSystemRegistry.GetServices(this);
            serviceProvider = services.BuildServiceProvider();
            initFlags = flags;
        }

        public Scene(IServiceCollection services, SceneInitFlags flags)
        {
            Name = "Scene";
            root = new SceneRootNode(this);

            this.services = services;
            serviceProvider = services.BuildServiceProvider();
            initFlags = flags;
        }

        public string Name { get; }

        [JsonIgnore]
        public string? Path { get => path; set => path = value; }

        [JsonIgnore]
        public bool Initialized
        {
            get => (flags & SceneFlags.Initialized) != 0;
        }

        [JsonIgnore]
        public bool Valid
        {
            get => (flags & SceneFlags.Valid) != 0;
        }

        [JsonIgnore]
        public bool Loaded
        {
            get => (flags & SceneFlags.Loaded) != 0;
        }

        public bool IsSimulating
        {
            get => (flags & SceneFlags.Simulating) != 0;
            set
            {
                if (value)
                {
                    flags |= SceneFlags.Simulating;
                }
                else
                {
                    flags &= ~SceneFlags.Simulating;
                }
            }
        }

        [JsonIgnore]
        public bool UnsavedChanged
        {
            get => (flags & SceneFlags.UnsavedChanges) != 0;
            set
            {
                if (value)
                {
                    flags |= SceneFlags.UnsavedChanges;
                }
                else
                {
                    flags &= ~SceneFlags.UnsavedChanges;
                }
            }
        }

        [JsonIgnore]
        public SceneFlags Flags => flags;

        [JsonIgnore]
        public SceneDispatcher Dispatcher { get; } = new();

        public CameraContainer Cameras { get => cameraContainer; }

        [JsonIgnore]
        public LightManager LightManager => GetRequiredService<LightManager>();

        [JsonIgnore]
        public List<GameObject> GameObjects => nodes;

        [JsonIgnore]
        public Camera? CurrentCamera => cameraContainer.ActiveCamera;

        [JsonIgnore]
        public ScriptManager Scripts => GetRequiredService<ScriptManager>();

        [JsonIgnore]
        public RenderManager RenderManager => GetRequiredService<RenderManager>();

        [JsonIgnore]
        public MaterialManager MaterialManager => GetRequiredService<MaterialManager>();

        [JsonIgnore]
        public WeatherSystem WeatherManager => GetRequiredService<WeatherSystem>();

        [JsonIgnore]
        public FlaggedList<SystemFlags, ISceneSystem> Systems => systems;

        [JsonIgnore]
        public ModelManager ModelManager => GetRequiredService<ModelManager>();

        [JsonIgnore]
        public AnimationManager AnimationManager => GetRequiredService<AnimationManager>();

        [JsonIgnore]
        public QuerySystem QueryManager => GetRequiredService<QuerySystem>();

        [JsonIgnore]
        public SceneProfiler Profiler { get; } = new(10);

        [JsonIgnore]
        public DrawLayerManager DrawLayerManager => serviceProvider.GetRequiredService<DrawLayerManager>();

        public SceneVariables Variables { get; } = [];

        public event Action<GameObject>? OnGameObjectAdded;

        public event Action<GameObject>? OnGameObjectRemoved;

        public GameObject Root => root;

        public void Initialize()
        {
            foreach (var service in serviceProvider.GetAllSystems<ISceneSystem>(services))
            {
                systems.Add(service);
            }

            semaphore.Wait();

            Time.FixedUpdate += FixedUpdate;

            root.Initialize();
            Validate();
            semaphore.Release();

            var awake = systems[SystemFlags.Awake];
            for (int i = 0; i < awake.Count; i++)
            {
                awake[i].Awake(this);
            }

            flags |= SceneFlags.Initialized;
        }

        public async Task InitializeAsync()
        {
            foreach (var service in serviceProvider.GetAllSystems<ISceneSystem>(services))
            {
                systems.Add(service);
            }

            await semaphore.WaitAsync();

            Time.FixedUpdate += FixedUpdate;

            root.Initialize();
            Validate();
            semaphore.Release();

            var awake = systems[SystemFlags.Awake];
            for (int i = 0; i < awake.Count; i++)
            {
                awake[i].Awake(this);
            }

            flags |= SceneFlags.Initialized;
        }

        public void Load(IGraphicsDevice device)
        {
            var load = systems[SystemFlags.Load];
            for (int i = 0; i < load.Count; i++)
            {
                load[i].Load(device);
            }

            Job.WaitAll(JobScheduler.Default.GetAllJobsWithFlag(JobFlags.BlockOnSceneLoad));

            Time.ResetTime();

            flags |= SceneFlags.Loaded;
        }

        public void Unload()
        {
            flags &= ~SceneFlags.Loaded;

            var unload = systems[SystemFlags.Unload];
            for (int i = 0; i < unload.Count; i++)
            {
                unload[i].Unload();
            }
        }

        public void Validate()
        {
            if ((initFlags & SceneInitFlags.SkipValidation) != 0)
            {
                flags |= SceneFlags.Valid;
                return;
            }

            HashSet<GameObject> visited = [];
            foreach (var node in root.Children)
            {
                ValidateUniqueness(node);
                ValidateChildParentRelation(node, visited);
            }

            flags |= SceneFlags.Valid;
        }

        private void ValidateUniqueness(GameObject gameObject)
        {
            foreach (var node in nodes)
            {
                if (node != gameObject && node.Guid == gameObject.Guid)
                {
                    throw new InvalidSceneException($"'{node.FullName}' is not unique.");
                }
            }
        }

        private static void ValidateChildParentRelation(GameObject parent, HashSet<GameObject> visited)
        {
            if (visited.Contains(parent))
            {
                Stack<GameObject> cyclePath = new();
                cyclePath.Push(parent);
                var currentNode = parent.Parent;
                while (currentNode != null)
                {
                    cyclePath.Push(currentNode);
                    currentNode = currentNode.Parent;
                }
                cyclePath.Push(parent);

                StringBuilder cycleStringBuilder = new();
                while (cyclePath.TryPop(out var gameObject))
                {
                    cycleStringBuilder.Append($"{gameObject.FullName} -> ");
                }

                throw new InvalidSceneException($"Cycle detected: {cycleStringBuilder}");
            }

            visited.Add(parent);

            foreach (var child in parent.Children)
            {
                if (child.Parent != parent)
                {
                    throw new InvalidSceneException($"'{parent.FullName}' child parent relation error.");
                }

                ValidateChildParentRelation(child, visited);
            }
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

            Dispatcher.ExecuteInvokes();

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

            semaphore.Release();
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

            var fixedUpdate = systems[SystemFlags.FixedUpdate];

            for (int i = 0; i < fixedUpdate.Count; i++)
            {
#if PROFILE
                Profiler.Start(fixedUpdate[i]);
#endif
                fixedUpdate[i].Update(Time.FixedDelta);
#if PROFILE
                Profiler.End(fixedUpdate[i]);
#endif
            }

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
            cameraContainer.Add(node);

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Register(node);
            }

            OnGameObjectAdded?.Invoke(node);
        }

        internal void UnregisterChild(GameObject node)
        {
            nodes.Remove(node);
            cameraContainer.Remove(node);
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
            flags &= ~SceneFlags.Initialized;
            semaphore.Wait();
            Time.FixedUpdate -= FixedUpdate;
            root.Uninitialize();
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
        public T? GetService<T>() where T : class
        {
            return serviceProvider.GetService<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetService<T>([NotNullWhen(true)] out T? system) where T : class
        {
            system = GetService<T>();
            return system != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetRequiredService<T>() where T : class
        {
            return serviceProvider.GetRequiredService<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject? SelectObject(Ray ray)
        {
            return serviceProvider.GetRequiredService<ObjectPickerManager>().SelectObject(ray);
        }
    }
}