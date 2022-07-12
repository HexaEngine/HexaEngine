﻿namespace HexaEngine.Scenes
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
    using HexaEngine.Physics;
    using HexaEngine.Rendering;
    using Newtonsoft.Json;
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

        [JsonIgnore]
        public readonly Simulation Simulation;

        [JsonIgnore]
        public readonly BufferPool BufferPool;

        [JsonIgnore]
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

        [JsonIgnore]
        public ISceneRenderer Renderer { get => renderer; set => renderer = value; }

        [JsonIgnore]
        public SceneDispatcher Dispatcher { get; } = new();

        [JsonIgnore]
        public IReadOnlyList<Camera> Cameras => cameras;

        [JsonIgnore]
        public IReadOnlyList<Light> Lights => lights;

        [JsonIgnore]
        public IReadOnlyList<Mesh> Meshes => meshes;

        [JsonIgnore]
        public IReadOnlyList<Material> Materials => materials;

        [JsonIgnore]
        public IReadOnlyList<Texture> Textures => textures;

        [JsonIgnore]
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

        public void AddChild(SceneNode node)
        {
            root.AddChild(node);
            cameras.AddIfIs(node);
            lights.AddIfIs(node);
            meshes.AddIfIs(node);
        }

        public void RemoveChild(SceneNode node)
        {
            cameras.RemoveIfIs(node);
            lights.RemoveIfIs(node);
            meshes.RemoveIfIs(node);
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