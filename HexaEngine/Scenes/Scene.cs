namespace HexaEngine.Scenes
{
    using BepuPhysics;
    using BepuUtilities;
    using BepuUtilities.Memory;
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Objects.Components;
    using HexaEngine.Physics;
    using HexaEngine.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Scene
    {
        private IGraphicsDevice device;
        private ISceneRenderer renderer;
        private readonly List<Camera> cameras = new();
        private readonly List<Light> lights = new();
        private readonly List<Mesh> meshes = new();
        private readonly List<Material> materials = new();
        private readonly List<Texture> textures = new();
        private readonly List<IDeferredRendererComponent> deferredRenderers = new();
        private readonly List<IForwardRendererComponent> forwardRenderers = new();
        private readonly List<IDepthRendererComponent> depthRenderers = new();
        public readonly Simulation Simulation;

        public readonly BufferPool BufferPool;
        public readonly ThreadDispatcher ThreadDispatcher;
        private readonly SceneNode root;
        public int ActiveCamera;

        public Scene()
        {
            Name = "Scene";
            Renderer = new DeferredRenderer();
            root = new SceneRootNode(this);
            BufferPool = new BufferPool();
            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new ThreadDispatcher(targetThreadCount);

            NarrowphaseCallbacks callbacks = new();
            callbacks.Characters = new(BufferPool);
            callbacks.Events = new(ThreadDispatcher, BufferPool);
            Simulation = Simulation.Create(BufferPool, callbacks, new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));
        }

        public string Name { get; }

        public ISceneRenderer Renderer { get => renderer; set => renderer = value; }

        public SceneDispatcher Dispatcher { get; } = new();

        public IReadOnlyList<Camera> Cameras => cameras;

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Mesh> Meshes => meshes;

        public IReadOnlyList<Material> Materials => materials;

        public IReadOnlyList<Texture> Textures => textures;

        public IReadOnlyList<IDeferredRendererComponent> DeferredRenderers => deferredRenderers;

        public IReadOnlyList<IForwardRendererComponent> ForwardRenderers => forwardRenderers;

        public IReadOnlyList<IDepthRendererComponent> DepthRenderers => depthRenderers;

        public Camera CurrentCamera => cameras[ActiveCamera];

        public SceneNode Root => root;

        public void Initialize(IGraphicsDevice device, SdlWindow window)
        {
            this.device = device;
            Time.FixedUpdate += FixedUpdate;
            Time.Initialize();
            Time.FrameUpdate();
            renderer.Initialize(device, window);
            materials.ForEach(x => x.Initialize(device));
            root.Initialize(device);
        }

        public bool TryGetMaterial(string name, out Material material)
        {
            material = materials.FirstOrDefault(x => x.Name == name);
            return material != null;
        }

        public void AddMaterial(Material material)
        {
            if (root.Initialized)
                material.Initialize(device);
            materials.Add(material);
        }

        public void RemoveMaterial(Material material)
        {
            if (root.Initialized)
                material.Dispose();
            materials.Remove(material);
        }

        private void FixedUpdate(object sender, EventArgs e)
        {
            root.Children.ForEach(x => x.FixedUpdate());
        }

        internal void Render(IGraphicsContext context, SdlWindow window, Viewport viewport)
        {
            CameraManager.Update();
            Simulate(Time.Delta);
            Dispatcher.ExecuteInvokes();
            root.Children.ForEach(x => x.Update());
            renderer.Render(context, window, viewport, this, CameraManager.Current);
            Mouse.Clear();
        }

        private const float stepsize = 0.016f;
        private float interpol;

        public void Simulate(float delta)
        {
            interpol += delta;
            while (interpol > stepsize)
            {
                interpol -= stepsize;
                Simulation.Timestep(stepsize, ThreadDispatcher);
            }
        }

        private class SceneRootNode : SceneNode
        {
            private Scene parent;

            public SceneRootNode(Scene parent)
            {
                this.parent = parent;
            }

            public override Scene GetScene()
            {
                return parent;
            }
        }

        public void AddChild(SceneNode node)
        {
            root.AddChild(node);
            forwardRenderers.AddRange(node.GetComponents<IForwardRendererComponent>());
            deferredRenderers.AddRange(node.GetComponents<IDeferredRendererComponent>());
            depthRenderers.AddRange(node.GetComponents<IDepthRendererComponent>());
            cameras.AddIfIs(node);
            lights.AddIfIs(node);
            meshes.AddIfIs(node);
        }

        public void RemoveChild(SceneNode node)
        {
            cameras.RemoveIfIs(node);
            lights.RemoveIfIs(node);
            meshes.RemoveIfIs(node);
            forwardRenderers.RemoveAll(x => node.GetComponents<IForwardRendererComponent>().Any(y => x == y));
            deferredRenderers.RemoveAll(x => node.GetComponents<IDeferredRendererComponent>().Any(y => x == y));
            depthRenderers.RemoveAll(x => node.GetComponents<IDepthRendererComponent>().Any(y => x == y));
            root.RemoveChild(node);
        }

        public void Uninitialize()
        {
            root.Uninitialize();
            materials.ForEach(x => x.Dispose());
            Renderer.Dispose();
        }
    }

    [Flags]
    public enum SceneUpdateFlags
    {
        ImmediateData = 1,
        Resources = 2,
        All = ImmediateData | Resources,
    }
}