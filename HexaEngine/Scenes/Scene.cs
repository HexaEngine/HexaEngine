namespace HexaEngine.Scenes
{
    using BepuPhysics;
    using BepuUtilities;
    using BepuUtilities.Memory;
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using HexaEngine.Lights;
    using HexaEngine.Objects;
    using HexaEngine.Physics;
    using HexaEngine.Scripting;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public struct NodePtr
    {
        public int Index;
    }

    public class Scene
    {
#nullable disable
        private IGraphicsDevice device;
#nullable enable
        private readonly List<SceneNode> nodes = new();
        private readonly List<Camera> cameras = new();
        private readonly List<Light> lights = new();
        private readonly List<Mesh> meshes = new();
        private readonly List<Material> materials = new();
        private readonly List<IScript> scripts = new();
        private readonly SemaphoreSlim semaphore = new(1);

        public readonly ConcurrentQueue<SceneCommand> CommandQueue = new();

        public Simulation Simulation;

        public BufferPool BufferPool;

        public ThreadDispatcher ThreadDispatcher;

        private readonly SceneNode root;
        public int ActiveCamera;

        public Scene()
        {
            Name = "Scene";
            root = new SceneRootNode(this);
        }

        public string Name { get; }

        public SceneDispatcher Dispatcher { get; } = new();

        public IReadOnlyList<Camera> Cameras => cameras;

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Mesh> Meshes => meshes;

        public IReadOnlyList<Material> Materials => materials;

        public IReadOnlyList<IScript> Scripts => scripts;

        public IReadOnlyList<SceneNode> Nodes => nodes;

        public Camera? CurrentCamera => (ActiveCamera >= 0 && ActiveCamera < cameras.Count) ? cameras[ActiveCamera] : null;

        public SceneNode Root => root;

        public bool IsSimulating;

        public void Initialize(IGraphicsDevice device, SdlWindow window)
        {
            semaphore.Wait();

            BufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new ThreadDispatcher(targetThreadCount);

            NarrowphaseCallbacks callbacks = new();
            callbacks.Characters = new(BufferPool);
            callbacks.Events = new(ThreadDispatcher, BufferPool);
            Simulation = Simulation.Create(BufferPool, callbacks, new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));

            this.device = device;
            Time.FixedUpdate += FixedUpdate;
            Time.Initialize();
            materials.ForEach(x => { x.Initialize(this); CommandQueue.Enqueue(new() { Sender = x, Type = CommandType.Load }); });
            meshes.ForEach(x => CommandQueue.Enqueue(new(CommandType.Load, x)));
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

        public void AddMaterial(Material material)
        {
            semaphore.Wait();
            if (root.Initialized)
            {
                CommandQueue.Enqueue(new(CommandType.Load, material));
                material.Initialize(this);
            }
            materials.Add(material);
            semaphore.Release();
        }

        public void RemoveMaterial(Material material)
        {
            semaphore.Wait();
            if (root.Initialized)
                CommandQueue.Enqueue(new(CommandType.Unload, material));
            materials.Remove(material);
            semaphore.Release();
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
            Mouse.Clear();
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

            for (int i = 0; i < scripts.Count; i++)
            {
                scripts[i].Update();
            }
        }

        public SceneNode? Find(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            semaphore.Wait();
            var result = nodes.FirstOrDefault(x => x.Name == name);
            semaphore.Release();
            return result;
        }

        public string GetAvailableName(SceneNode node, string name)
        {
            if (!string.IsNullOrEmpty(name)) return node.Name;
            if (Find(name) != null) return node.Name;
            return name;
        }

        internal void RegisterChild(SceneNode node)
        {
            nodes.Add(node);
            cameras.AddIfIs(node);
            lights.AddIfIs(node);
            scripts.AddComponentIfIs(node);
        }

        internal void UnregisterChild(SceneNode node)
        {
            nodes.Remove(node);
            cameras.RemoveIfIs(node);
            lights.RemoveIfIs(node);
            scripts.RemoveComponentIfIs(node);
        }

        private class SceneRootNode : SceneNode
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
        }

        public void AddMesh(Mesh mesh)
        {
            semaphore.Wait();
            if (root.Initialized)
                CommandQueue.Enqueue(new(CommandType.Load, mesh));
            meshes.Add(mesh);
            semaphore.Release();
        }

        public void AddChild(SceneNode node)
        {
            semaphore.Wait();
            root.AddChild(node);
            semaphore.Release();
        }

        public void RemoveChild(SceneNode node)
        {
            semaphore.Wait();
            root.RemoveChild(node);
            semaphore.Release();
        }

        public void Merge(SceneNode node)
        {
            semaphore.Wait();
            root.Merge(node);
            semaphore.Release();
        }

        public void Uninitialize()
        {
            semaphore.Wait();
            root.Uninitialize();
            Simulation.Dispose();
            ThreadDispatcher.Dispose();
            semaphore.Release();
        }
    }
}