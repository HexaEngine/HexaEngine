namespace HexaEngine.Components.Renderer
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public abstract class PrimitiveRenderComponent : BaseDrawableComponent
    {
        protected AssetRef materialAsset;
        private static PrimitiveRenderer renderer = null!;
        private static readonly Lock rendererlock = new();
        private BoundingBox boundingBox;
        private PrimitiveModel? model;

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(PrimitiveRenderer);

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Clustered | RendererFlags.Deferred | RendererFlags.Forward;

        [JsonIgnore]
        public override BoundingBox BoundingBox => BoundingBox.Transform(boundingBox, GameObject?.Transform ?? Matrix4x4.Identity);

        [JsonIgnore]
        public Matrix4x4 Transform => GameObject?.Transform ?? Matrix4x4.Identity;

        [EditorProperty("Material", AssetType.Material)]
        public AssetRef Material
        {
            get => materialAsset;
            set => SetAndUpdateModelEquals(ref materialAsset, value, ModelUpdateFlags.Material);
        }

        protected override void LoadCore(IGraphicsDevice device)
        {
            lock (rendererlock)
            {
                if (renderer == null || renderer.IsDisposed)
                {
                    renderer = new();
                    ((IRenderer1)renderer).Initialize(device, CullingManager.Current.Context);
                }
                else
                {
                    renderer.AddRef();
                }
            }

            UpdateModel(ModelUpdateFlags.All);
        }

        protected override void UnloadCore()
        {
            renderer.Dispose();
            model?.Dispose();
        }

        public override void Update(IGraphicsContext context)
        {
            if (model == null)
            {
                return;
            }
            renderer.Update(context, GameObject.Transform.Global, model);
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
        {
            if (model == null)
            {
                return;
            }
            if (path == RenderPath.Deferred)
            {
                renderer.DrawDeferred(context, model);
            }
            else
            {
                renderer.DrawForward(context, model);
            }
        }

        public override void DrawDepth(IGraphicsContext context)
        {
            if (model == null)
            {
                return;
            }
            renderer.DrawDepth(context, model);
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (model == null)
            {
                return;
            }
            renderer.DrawShadowMap(context, model, light, type);
        }

        public override void VisibilityTest(CullingContext context)
        {
            if (model == null)
            {
                return;
            }
            renderer.VisibilityTest(context, model);
        }

        public override void Bake(IGraphicsContext context)
        {
            throw new NotSupportedException();
        }

        protected void UpdateModel(ModelUpdateFlags updateFlags)
        {
            if (model == null)
            {
                model?.Dispose();
                var primitive = CreatePrimitive();
                var material = MaterialData.GetMaterial(materialAsset, LoggerFactory.General);
                boundingBox = GetBoundingBox();
                model = new(primitive, material, boundingBox);
                GameObject.SendUpdateTransformed(); // necessary to inform the light system about an change for updating shadows.
                return;
            }

            if ((updateFlags & ModelUpdateFlags.Mesh) != 0)
            {
                var newPrimitive = CreatePrimitive();
                var oldPrimitive = model.Prim;
                boundingBox = GetBoundingBox();
                model.Prim = newPrimitive;
                model.BoundingBox = boundingBox;
                oldPrimitive.Dispose();
                GameObject.SendUpdateTransformed(); // necessary to inform the light system about an change for updating shadows.
                return;
            }

            if ((updateFlags & ModelUpdateFlags.Material) != 0)
            {
                var newMaterial = MaterialData.GetMaterial(materialAsset, LoggerFactory.General);
                var oldMaterial = model.MaterialData;
                model.MaterialData = newMaterial;
                oldMaterial.Dispose();
                GameObject.SendUpdateTransformed(); // necessary to inform the light system about an change for updating shadows.
                return;
            }
        }

        protected abstract IPrimitive CreatePrimitive();

        protected abstract BoundingBox GetBoundingBox();

        protected virtual void SetAndUpdateModel<T>(ref T target, T value, ModelUpdateFlags updateFlags = ModelUpdateFlags.Mesh, [CallerMemberName] string propertyName = "")
        {
            OnPropertyChanging(propertyName);
            target = value;
            OnPropertyChanged(propertyName);
            if (Loaded) // prevent early update triggers in the deserialization process.
            {
                UpdateModel(updateFlags);
            }
        }

        protected virtual void SetAndUpdateModelEquals<T>(ref T target, T value, ModelUpdateFlags updateFlags = ModelUpdateFlags.Mesh, [CallerMemberName] string propertyName = "") where T : IEquatable<T>
        {
            if (target.Equals(value)) return;
            OnPropertyChanging(propertyName);
            target = value;
            OnPropertyChanged(propertyName);
            if (Loaded) // prevent early update triggers in the deserialization process.
            {
                UpdateModel(updateFlags);
            }
        }

        [Flags]
        protected enum ModelUpdateFlags
        {
            None = 0,
            Mesh = 1,
            Material = 2,
            All = Mesh | Material,
        }
    }
}