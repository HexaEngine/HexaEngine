﻿namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using System;
    using System.Numerics;

    [EditorComponent<ParticleSystemComponent>("Particle System", false, false)]
    public class ParticleSystemComponent : IRendererComponent
    {
        private IGraphicsDevice device;
        private GameObject gameObject;
        private ParticleRenderer renderer;
        private readonly ParticleEmitter emitter = new();

        private string particleTexturePath = string.Empty;
        private Texture2D? particleTexture;

        private ResourceRef<IShaderResourceView> dsv;

        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Transparency;

        public BoundingBox BoundingBox { get; }

        public RendererFlags Flags { get; } = RendererFlags.Draw | RendererFlags.Update;

        [EditorProperty("Texture", EditorPropertyMode.Filepicker)]
        public string ParticleTexturePath
        {
            get => particleTexturePath;
            set
            {
                particleTexturePath = value;
                if (device == null)
                {
                    return;
                }

                UpdateTextureAsync();
            }
        }

        [EditorProperty("Velocity")]
        public Vector4 Velocity { get => emitter.Velocity; set => emitter.Velocity = value; }

        [EditorProperty("PositionVariance")]
        public Vector4 PositionVariance { get => emitter.PositionVariance; set => emitter.PositionVariance = value; }

        [EditorProperty("ParticleLifespan")]
        public float ParticleLifespan { get => emitter.ParticleLifespan; set => emitter.ParticleLifespan = value; }

        [EditorProperty("StartSize")]
        public float StartSize { get => emitter.StartSize; set => emitter.StartSize = value; }

        [EditorProperty("EndSize")]
        public float EndSize { get => emitter.EndSize; set => emitter.EndSize = value; }

        [EditorProperty("Mass")]
        public float Mass { get => emitter.Mass; set => emitter.Mass = value; }

        [EditorProperty("VelocityVariance")]
        public float VelocityVariance { get => emitter.VelocityVariance; set => emitter.VelocityVariance = value; }

        [EditorProperty("ParticlesPerSecond")]
        public float ParticlesPerSecond { get => emitter.ParticlesPerSecond; set => emitter.ParticlesPerSecond = value; }

        [EditorProperty("CollisionsEnabled")]
        public bool CollisionsEnabled { get => emitter.CollisionsEnabled; set => emitter.CollisionsEnabled = value; }

        [EditorProperty("CollisionThickness")]
        public int CollisionThickness { get => emitter.CollisionThickness; set => emitter.CollisionThickness = value; }

        [EditorProperty("AlphaBlended")]
        public bool AlphaBlended { get => emitter.AlphaBlended; set => emitter.AlphaBlended = value; }

        [EditorProperty("Pause")]
        public bool Pause { get => emitter.Pause; set => emitter.Pause = value; }

        [EditorProperty("Sort")]
        public bool Sort { get => emitter.Sort; set => emitter.Sort = value; }

        [EditorButton("Reset Emitter")]
        public void ResetEmitter()
        {
            emitter.ResetEmitter = true;
        }

        public async void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.device = device;
            this.gameObject = gameObject;
            renderer = new(device);

            dsv = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.DepthCopy");

            await UpdateTextureAsync();
        }

        public void Destory()
        {
            renderer.Dispose();
            particleTexture?.Dispose();
        }

        public void Draw(IGraphicsContext context)
        {
            if (!gameObject.IsEnabled)
                return;
            if (particleTexture == null)
                return;
            renderer.Draw(context, emitter, dsv.Value, emitter.ParticleTexture.SRV);
        }

        public void DrawDepth(IGraphicsContext context)
        {
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotSupportedException();
        }

        public void Update(IGraphicsContext context)
        {
            if (!gameObject.IsEnabled)
                return;
            if (particleTexture == null)
                return;
            emitter.Position = new(gameObject.Transform.GlobalPosition, 0);
            renderer.Update(emitter);
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
            throw new NotSupportedException();
        }

        private Task UpdateTextureAsync()
        {
            emitter.ParticleTexture = null;
            var tmpTexture = particleTexture;
            particleTexture = null;
            tmpTexture?.Dispose();

            var state = new Tuple<IGraphicsDevice, ParticleSystemComponent>(device, this);
            return Task.Factory.StartNew((state) =>
            {
                var p = (Tuple<IGraphicsDevice, ParticleSystemComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = Paths.CurrentAssetsPath + component.particleTexturePath;

                if (FileSystem.Exists(path))
                {
                    component.particleTexture = new(component.device, new TextureFileDescription(path));
                    component.emitter.ParticleTexture = component.particleTexture;
                    component.emitter.ResetEmitter = true;
                }
            }, state);
        }
    }
}