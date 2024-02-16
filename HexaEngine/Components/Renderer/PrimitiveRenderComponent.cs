namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Numerics;

    public abstract class PrimitiveRenderComponent : BaseRendererComponent
    {
        protected AssetRef materialAsset;
        protected PrimitiveRenderer renderer;
        protected IPrimitive primitive;

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(PrimitiveRenderer);

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Clustered | RendererFlags.Deferred | RendererFlags.Forward;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get => BoundingBox.Transform(BoundingBox.Empty, GameObject?.Transform ?? Matrix4x4.Identity); }

        [JsonIgnore]
        public Matrix4x4 Transform => GameObject?.Transform ?? Matrix4x4.Identity;

        [EditorProperty("Material", AssetType.Material)]
        public AssetRef Material
        {
            get => materialAsset;
            set
            {
                materialAsset = value;
                UpdateModel();
            }
        }

        public override void Load(IGraphicsDevice device)
        {
            renderer = new(device);

            UpdateModel();
        }

        public override void Unload()
        {
            renderer.Dispose();
            primitive?.Dispose();
        }

        public override void Update(IGraphicsContext context)
        {
            renderer.Update(context, GameObject.Transform.Global);
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
        {
            if (path == RenderPath.Deferred)
            {
                renderer.DrawDeferred(context);
            }
            else
            {
                renderer.DrawForward(context);
            }
        }

        public override void DrawDepth(IGraphicsContext context)
        {
            renderer.DrawDepth(context);
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            renderer.DrawShadowMap(context, light, type);
        }

        public override void VisibilityTest(CullingContext context)
        {
        }

        public override void Bake(IGraphicsContext context)
        {
            throw new NotSupportedException();
        }

        protected void UpdateModel()
        {
            UpdateModel(Application.GraphicsDevice);
        }

        protected abstract void UpdateModel(IGraphicsDevice device);
    }
}