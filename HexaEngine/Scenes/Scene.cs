namespace HexaEngine.Scenes
{
    using BepuPhysics;
    using BepuUtilities;
    using BepuUtilities.Memory;
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Lights;
    using HexaEngine.Objects;
    using HexaEngine.Physics;
    using HexaEngine.Resources;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public struct NodePtr
    {
        public int Index;
    }

    public class Scene
    {
        private readonly List<GameObject> nodes = new();
        private readonly List<Camera> cameras = new();
        private string[] cameraNames = Array.Empty<string>();
        private readonly List<Light> lights = new();
        private readonly ScriptManager scriptManager = new();
        private InstanceManager instanceManager;
        private MaterialManager materialManager;
        private MeshManager meshManager;
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

        public Scene()
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
        public List<Light> Lights => lights;

        [JsonIgnore]
        public List<GameObject> Nodes => nodes;

        [JsonIgnore]
        public Camera? CurrentCamera => (ActiveCamera >= 0 && ActiveCamera < cameras.Count) ? cameras[ActiveCamera] : null;

        [JsonIgnore]
        public InstanceManager InstanceManager => instanceManager;

        public MaterialManager MaterialManager => materialManager;

        public MeshManager MeshManager => meshManager;

        public GameObject Root => root;

        public bool IsSimulating;

        public void Initialize(IGraphicsDevice device)
        {
            instanceManager = new(device);
            materialManager = new();
            meshManager = new();
            semaphore.Wait();

            BufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new ThreadDispatcher(targetThreadCount);

            NarrowphaseCallbacks callbacks = new();
            callbacks.Characters = new(BufferPool);
            callbacks.Events = new(ThreadDispatcher, BufferPool);
            Simulation = Simulation.Create(BufferPool, callbacks, new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));

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

        private void FixedUpdate(object? sender, EventArgs e)
        {
            if (Designer.InDesignMode) return;
            semaphore.Wait();
            for (int i = 0; i < root.Children.Count; i++)
            {
                root.Children[i].FixedUpdate();
            }
            semaphore.Release();
        }

        internal void Tick()
        {
            semaphore.Wait();
            CameraManager.Update();
            Simulate(Time.Delta);
            Dispatcher.ExecuteInvokes();
            semaphore.Release();
        }

        private const float stepsize = 0.016f;
        private float interpol;

        public void Simulate(float delta)
        {
            if (Designer.InDesignMode) return;
            if (!IsSimulating) return;

            interpol += delta;
            while (interpol > stepsize)
            {
                interpol -= stepsize;
                Simulation.Timestep(stepsize, ThreadDispatcher);
            }

            for (int i = 0; i < root.Children.Count; i++)
            {
                root.Children[i].Update();
            }

            scriptManager.Update();
        }

        public GameObject? Find(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            semaphore.Wait();
            var result = nodes.FirstOrDefault(x => x.Name == name);
            semaphore.Release();
            return result;
        }

        public string GetAvailableName(GameObject node, string name)
        {
            if (!string.IsNullOrEmpty(name)) return node.Name;
            if (Find(name) != null) return node.Name;
            return name;
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

            lights.AddIfIs(node);
            scriptManager.Register(node);
        }

        internal void UnregisterChild(GameObject node)
        {
            nodes.Remove(node);
            if (cameras.RemoveIfIs(node, out int index))
            {
                Array.Copy(cameraNames, index + 1, cameraNames, index, cameras.Count - index);
            }

            lights.RemoveIfIs(node);
            scriptManager.Unregister(node);
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
                if (collect && current.IsVisible)
                    yield return current;
            }
        }
    }
}