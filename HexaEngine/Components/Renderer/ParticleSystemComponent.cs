namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Particles;
    using HexaEngine.Scenes.Managers;
    using System;
    using System.Numerics;

    [EditorCategory("Particles")]
    [EditorComponent<ParticleSystemComponent>("GPU Particle System", false, false)]
    public class ParticleSystemComponent : BaseRendererComponent, ISelectableRayTest
    {
        private GPUParticleSystem renderer;
        private readonly ParticleEmitter emitter = new();

        private AssetRef particleTexturePath;
        private Texture2D? particleTexture;

        private ResourceRef<DepthStencil> dsv;

        public ParticleSystemComponent()
        {
            QueueIndex = (uint)RenderQueueIndex.Transparency;
        }

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(GPUParticleSystem);

        [JsonIgnore]
        public override BoundingBox BoundingBox { get; }

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.Draw | RendererFlags.Update | RendererFlags.NoDepthTest;

        [EditorProperty("Texture", Core.Assets.AssetType.Texture2D)]
        public AssetRef ParticleTexturePath
        {
            get => particleTexturePath;
            set
            {
                particleTexturePath = value;
                UpdateTextureAsync();
            }
        }

        [EditorProperty("Velocity")]
        public Vector4 Velocity { get => emitter.Velocity; set => emitter.Velocity = value; }

        [EditorProperty("Position Variance")]
        public Vector4 PositionVariance { get => emitter.PositionVariance; set => emitter.PositionVariance = value; }

        [EditorProperty("Particle Lifespan")]
        public float ParticleLifespan { get => emitter.ParticleLifespan; set => emitter.ParticleLifespan = value; }

        [EditorProperty("Start Size")]
        public float StartSize { get => emitter.StartSize; set => emitter.StartSize = value; }

        [EditorProperty("End Size")]
        public float EndSize { get => emitter.EndSize; set => emitter.EndSize = value; }

        [EditorProperty("Mass")]
        public float Mass { get => emitter.Mass; set => emitter.Mass = value; }

        [EditorProperty("Velocity Variance")]
        public float VelocityVariance { get => emitter.VelocityVariance; set => emitter.VelocityVariance = value; }

        [EditorProperty("Particles Per Second")]
        public float ParticlesPerSecond { get => emitter.ParticlesPerSecond; set => emitter.ParticlesPerSecond = value; }

        [EditorProperty("Collisions Enabled")]
        public bool CollisionsEnabled { get => emitter.CollisionsEnabled; set => emitter.CollisionsEnabled = value; }

        [EditorProperty("Collision Thickness")]
        public int CollisionThickness { get => emitter.CollisionThickness; set => emitter.CollisionThickness = value; }

        [EditorProperty("Alpha Blended")]
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

        protected override void LoadCore(IGraphicsDevice device)
        {
            renderer = new(device, emitter);

            dsv = SceneRenderer.Current.ResourceBuilder.GetDepthStencilBuffer("#DepthStencil");

            UpdateTextureAsync();
        }

        protected override void UnloadCore()
        {
            renderer.Dispose();
            particleTexture?.Dispose();
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
        {
            if (particleTexture == null || Application.InEditMode)
            {
                return;
            }

            renderer.Draw(context, dsv.Value.SRV);
        }

        public override void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public override void DrawDepth(IGraphicsContext context)
        {
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotSupportedException();
        }

        public override void Update(IGraphicsContext context)
        {
            if (particleTexture == null)
            {
                return;
            }

            emitter.Position = new(GameObject.Transform.GlobalPosition, 0);
            renderer.Update();
        }

        public override void VisibilityTest(CullingContext context)
        {
            throw new NotSupportedException();
        }

        private Task UpdateTextureAsync()
        {
            Loaded = false;
            emitter.ParticleTexture = null;
            var tmpTexture = particleTexture;
            particleTexture = null;
            tmpTexture?.Dispose();

            var state = new Tuple<IGraphicsDevice, ParticleSystemComponent>(Application.GraphicsDevice, this);
            return Task.Factory.StartNew((state) =>
            {
                var p = (Tuple<IGraphicsDevice, ParticleSystemComponent>)state;
                var device = p.Item1;
                var component = p.Item2;
                var path = component.particleTexturePath.GetPath();

                if (path == null)
                    return;

                if (component.GameObject == null)
                {
                    return;
                }

                if (FileSystem.Exists(path))
                {
                    component.particleTexture = new(new TextureFileDescription(path));
                    component.emitter.ParticleTexture = component.particleTexture;
                    component.emitter.ResetEmitter = true;
                    component.Loaded = true;
                }
            }, state);
        }

        public bool SelectRayTest(Ray ray, ref float depth)
        {
            BoundingSphere sphere = new(GameObject.Transform.GlobalPosition, 0.5f);
            var result = sphere.Intersects(ray);
            if (result == null)
            {
                return false;
            }
            depth = result.Value;
            return true;
        }
    }
}