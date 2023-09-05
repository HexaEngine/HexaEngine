namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Culling;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Particles;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Renderers;
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

        private ResourceRef<DepthStencil> dsv;

        [JsonIgnore]
        public string DebugName { get; private set; } = nameof(ParticleRenderer);

        [JsonIgnore]
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Transparency;

        [JsonIgnore]
        public BoundingBox BoundingBox { get; }

        [JsonIgnore]
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
            DebugName = gameObject.Name + DebugName;
            this.device = device;
            this.gameObject = gameObject;
            renderer = new(device);

            dsv = SceneRenderer.Current.ResourceBuilder.GetDepthStencilBuffer("#DepthStencil");

            await UpdateTextureAsync();
        }

        public void Destroy()
        {
            renderer.Dispose();
            particleTexture?.Dispose();
        }

        public void Draw(IGraphicsContext context, RenderPath path)
        {
            if (!gameObject.IsEnabled)
                return;
            if (particleTexture == null)
                return;
            renderer.Draw(context, emitter, dsv.Value.SRV, emitter.ParticleTexture.SRV);
        }

        public void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
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

        public void VisibilityTest(CullingContext context)
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