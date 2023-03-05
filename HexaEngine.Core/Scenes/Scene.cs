namespace HexaEngine.Core.Scenes
{
    using BepuPhysics;
    using BepuUtilities;
    using BepuUtilities.Memory;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Physics;
    using HexaEngine.Core.Physics.Characters;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Scenes.Systems;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class Scene
    {
        private readonly List<ISystem> systems = new();
        private readonly List<GameObject> nodes = new();
        private readonly List<Camera> cameras = new();
        private string[] cameraNames = Array.Empty<string>();
        private readonly ScriptManager scriptManager = new();
        private InstanceManager instanceManager;
        private MaterialManager materialManager;
        private MeshManager meshManager;
        private LightManager lightManager;
        private RenderManager renderManager;
        private AnimationManager animationManager = new();

        private readonly SemaphoreSlim semaphore = new(1);
        private string? path;

        [JsonIgnore]
        public Simulation Simulation;

        [JsonIgnore]
        public BufferPool BufferPool;

        [JsonIgnore]
        public ThreadDispatcher ThreadDispatcher;

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
        public string[] CameraNames => cameraNames;

        [JsonIgnore]
        public LightManager Lights => lightManager;

        [JsonIgnore]
        public List<GameObject> Nodes => nodes;

        [JsonIgnore]
        public Camera? CurrentCamera => ActiveCamera >= 0 && ActiveCamera < cameras.Count ? cameras[ActiveCamera] : null;

        [JsonIgnore]
        public InstanceManager InstanceManager => instanceManager;

        [JsonIgnore]
        public MaterialManager MaterialManager { get => materialManager; set => materialManager = value; }

        [JsonIgnore]
        public ScriptManager Scripts => scriptManager;

        [JsonIgnore]
        public RenderManager RenderManager => renderManager;

        [JsonIgnore]
        public List<ISystem> Systems => systems;

        [JsonIgnore]
        public MeshManager MeshManager { get => meshManager; set => meshManager = value; }

        [JsonIgnore]
        public AnimationManager AnimationManager { get => animationManager; set => animationManager = value; }

        [JsonIgnore]
        public SceneProfiler Profiler { get; } = new(10);

        [JsonIgnore]
        public CharacterControllers CharacterControllers => characterControllers;

        [JsonIgnore]
        public ContactEvents ContactEvents => contactEvents;

        public SceneVariables Variables { get; } = new();

        public GameObject Root => root;

        public bool IsSimulating;

        public void Initialize(IGraphicsDevice device)
        {
            instanceManager = new();
            lightManager = new(device, instanceManager);
            materialManager ??= new();
            meshManager ??= new();

            systems.Add(new AudioSystem());
            systems.Add(new PhysicsSystem());
            systems.Add(new AnimationSystem(this));
            systems.Add(scriptManager);
            systems.Add(lightManager);
            systems.Add(new TransformSystem());
            systems.Add(renderManager = new(device, instanceManager));

            semaphore.Wait();

            BufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new ThreadDispatcher(targetThreadCount);

            characterControllers = new(BufferPool);
            contactEvents = new(ThreadDispatcher, BufferPool);

            Simulation = Simulation.Create(BufferPool, new NarrowphaseCallbacks(characterControllers, contactEvents), new PoseIntegratorCallbacks(new Vector3(0, -9.81f, 0)), new SolveDescription(8, 1));

            Time.FixedUpdate += FixedUpdate;
            Time.Initialize();

            root.Initialize(device);
            Validate();
            semaphore.Release();
        }

        public bool Validate()
        {
            foreach (var node in nodes)
            {
                if (nodes.Any(x => node != x && x.Name == x.Name))
                    return false;
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

        public void Tick()
        {
            Profiler.Clear();
            semaphore.Wait();
            CameraManager.Update();
            Simulate(Time.Delta);
            Dispatcher.ExecuteInvokes();
            semaphore.Release();

#if PROFILE
            Profiler.Start(systems);
#endif
            for (int i = 0; i < systems.Count; i++)
            {
#if PROFILE
                Profiler.Start(systems[i]);
#endif
                systems[i].Update(ThreadDispatcher);
#if PROFILE
                Profiler.End(systems[i]);
#endif
            }
#if PROFILE
            Profiler.End(systems);
#endif
        }

        private void FixedUpdate(object? sender, EventArgs e)
        {
            if (Application.InDesignMode)
            {
#if PROFILE
                Profiler.Set(sceneUpdateCallbacks, 0);
#endif
                return;
            }

            semaphore.Wait();

#if PROFILE
            Profiler.Start(systems);
#endif
            for (int i = 0; i < systems.Count; i++)
            {
#if PROFILE
                Profiler.Start(systems[i]);
#endif
                systems[i].Update(ThreadDispatcher);
#if PROFILE
                Profiler.End(systems[i]);
#endif
            }
#if PROFILE
            Profiler.End(systems);
#endif
            semaphore.Release();
        }

        private const float stepsize = 0.016f;
        private float interpol;
        private CharacterControllers characterControllers;
        private ContactEvents contactEvents;

        public void Simulate(float delta)
        {
            if (Application.InDesignMode) return;
            if (!IsSimulating) return;

            interpol += delta;
            while (interpol > stepsize)
            {
                interpol -= stepsize;
                Simulation.Timestep(stepsize, ThreadDispatcher);
            }
        }

        public GameObject? Find(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
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

        public string GetAvailableName(string name)
        {
            return GetNewName(name);
        }

        private string GetNewName(string name)
        {
            if (Find(name) == null)
                return name;

            string result = name;

            int i = 0;
            while (Find(result) != null)
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
                if (cameraNames.Length != cameras.Capacity)
                {
                    var old = cameraNames;
                    cameraNames = new string[cameras.Capacity];
                    Array.Copy(old, cameraNames, old.Length);
                }
                cameraNames[cameras.Count - 1] = node.Name;
            }

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Register(node);
            }
        }

        internal void UnregisterChild(GameObject node)
        {
            nodes.Remove(node);
            if (cameras.RemoveIfIs(node, out int index))
            {
                Array.Copy(cameraNames, index + 1, cameraNames, index, cameras.Count - index);
            }

            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Unregister(node);
            }
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

        public void Merge(GameObject node)
        {
            semaphore.Wait();
            root.Merge(node);
            semaphore.Release();
        }

        public void BuildTree()
        {
            root.BuildTree();
        }

        public void Uninitialize()
        {
            semaphore.Wait();
            Time.FixedUpdate -= FixedUpdate;
            root.Uninitialize();
            Simulation.Dispose();
            ThreadDispatcher.Dispose();
            lightManager.Dispose();
            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Destroy(ThreadDispatcher);
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
                    yield return current;
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
    }
}