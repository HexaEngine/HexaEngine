namespace HexaEngine.Components.Renderer
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Jobs;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes.Managers;
    using Newtonsoft.Json;
    using System.Numerics;

    [EditorCategory("Renderer")]
    [EditorComponent(typeof(MeshRendererComponent), "Mesh Renderer", Icon = "\xf158")]
    public class MeshRendererComponent : BaseDrawableComponent, ILODRendererComponent, ISelectableRayTest
    {
        private ModelManager modelManager;
        private MaterialManager materialManager;
        private Model? model;
        private AssetRef modelAsset;
        private int maxLODLevel;
        private int minLODLevel;
        private int currentLODLevel;

        private static MeshRenderer meshRenderer1;
        private static int instances;

        static MeshRendererComponent()
        {
        }

        [EditorProperty("Model", AssetType.Model)]
        public AssetRef Model
        {
            get => modelAsset;
            set
            {
                modelAsset = value;
                UpdateModel();
            }
        }

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(MeshRenderer);

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Clustered | RendererFlags.Deferred | RendererFlags.Forward;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get => BoundingBox.Transform(model?.BoundingBox ?? BoundingBox.Empty, GameObject?.Transform ?? Matrix4x4.Identity); }

        [JsonIgnore]
        public Matrix4x4 Transform => GameObject?.Transform ?? Matrix4x4.Identity;

        [JsonIgnore]
        public Model? ModelInstance => model;

        [EditorCategory("Materials")]
        [EditorProperty("")]
        public MaterialAssetMapper Materials { get; } = new();

        [JsonIgnore]
        public int LODLevel => currentLODLevel;

        [EditorCategory("LOD")]
        [EditorProperty("Max Level")]
        public int MaxLODLevel
        {
            get => maxLODLevel;
            set
            {
                if (model != null)
                {
                    value = MathUtil.Clamp(value, 0, model.LODLevels - 1);
                }

                maxLODLevel = value;
            }
        }

        [EditorCategory("LOD")]
        [EditorProperty("Min Level")]
        public int MinLODLevel
        {
            get => minLODLevel;
            set
            {
                if (model != null)
                {
                    value = MathUtil.Clamp(value, 0, model.LODLevels - 1);
                }

                minLODLevel = value;
            }
        }

        protected override void LoadCore(IGraphicsDevice device)
        {
            modelManager = GameObject.GetScene().ModelManager;
            materialManager = GameObject.GetScene().MaterialManager;

            if (Interlocked.Increment(ref instances) == 1)
            {
                meshRenderer1 = new();
                ((IRenderer1)meshRenderer1).Initialize(device, CullingManager.Current.Context);
            }

            UpdateModel();
        }

        protected override void UnloadCore()
        {
            if (Interlocked.Decrement(ref instances) == 0)
            {
                meshRenderer1.Dispose();
            }

            model?.Dispose();
        }

        public override void Update(IGraphicsContext context)
        {
            if (model == null)
            {
                return;
            }

            meshRenderer1.Update(context, GameObject.Transform.Global, model);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer cam)
        {
            if (model == null)
            {
                return;
            }

            meshRenderer1.DrawDepth(context, model, cam);
        }

        public override void DrawDepth(IGraphicsContext context)
        {
            if (model == null)
            {
                return;
            }

            meshRenderer1.DrawDepth(context, model);
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (model == null)
            {
                return;
            }

            meshRenderer1.DrawShadowMap(context, model, light, type);
        }

        public override void VisibilityTest(CullingContext context)
        {
            if (model == null)
            {
                return;
            }

            meshRenderer1.VisibilityTest(context, model);
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
        {
            if (model == null)
            {
                return;
            }

            if (path == RenderPath.Deferred)
            {
                meshRenderer1.DrawDeferred(context, model);
            }
            else
            {
                meshRenderer1.DrawForward(context, model);
            }
        }

        public override void Bake(IGraphicsContext context)
        {
            if (model == null)
            {
                return;
            }

            meshRenderer1.Bake(context, model);
        }

        public bool SelectRayTest(Ray ray, ref float depth)
        {
            if (model == null)
            {
                return false;
            }

            if (!BoundingBox.Intersects(ray).HasValue)
            {
                return false;
            }

            Ray objectSpaceRay = Ray.Transform(ray, GameObject.Transform.GlobalInverse);

            for (int i = 0; i < model.Meshes.Length; i++)
            {
                var mesh = model.Meshes[i];
                var result = ((MeshLODData)mesh.LODData).Intersect(objectSpaceRay);
                if (result != null)
                {
                    depth = result.Value;
                    return true;
                }
            }

            return false;
        }

        private void OnChanged(MaterialAssetMapping mapping)
        {
            UpdateMaterial(mapping);
        }

        public Job UpdateModel()
        {
            Loaded = false;
            var tmpModel = model;
            model = null;
            tmpModel?.Dispose();

            return Job.Run("Model Load Job", this, state =>
            {
                if (state is not MeshRendererComponent component)
                {
                    return;
                }

                if (component.GameObject == null)
                {
                    return;
                }

                if (component.modelManager == null)
                {
                    return;
                }

                if (component.modelAsset.Guid == Guid.Empty)
                {
                    return;
                }

                var stream = modelAsset.OpenReadReusable();
                if (stream != null)
                {
                    component.Materials.OnChanged -= component.OnChanged;
                    component.model = new(stream, component.Materials, LoggerFactory.General);
                    component.maxLODLevel = MathUtil.Clamp(component.maxLODLevel, 0, component.model.LODLevels - 1);
                    component.minLODLevel = MathUtil.Clamp(component.minLODLevel, 0, component.model.LODLevels - 1);
                    component.Materials.OnChanged += component.OnChanged;
                    component.model.LoadAsync().Wait();

                    var flags = component.model.ShaderFlags;
                    component.QueueIndex = (uint)RenderQueueIndex.Geometry;

                    if (flags.HasFlag(ModelMaterialShaderFlags.AlphaTest))
                    {
                        component.QueueIndex = (uint)RenderQueueIndex.AlphaTest;
                    }

                    if (flags.HasFlag(ModelMaterialShaderFlags.DepthAlways))
                    {
                        component.QueueIndex = (uint)RenderQueueIndex.GeometryLast;
                    }

                    if (flags.HasFlag(ModelMaterialShaderFlags.Transparent))
                    {
                        component.QueueIndex = (uint)RenderQueueIndex.Transparency;
                    }

                    component.Loaded = true;
                    component.GameObject.SendUpdateTransformed();
                }
                else
                {
                    LoggerFactory.General.Error($"Couldn't load model {modelAsset}");
                }
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }

        public Job UpdateMaterial(MaterialAssetMapping mapping)
        {
            return Job.Run("Model Material Load Job", (this, mapping), state =>
            {
                if (state is not (MeshRendererComponent component, MaterialAssetMapping mapping))
                {
                    return;
                }

                if (component.GameObject == null)
                {
                    return;
                }

                if (component.model == null)
                {
                    return;
                }

                component.model.ReloadMaterial(mapping);

                var flags = component.model.ShaderFlags;
                component.QueueIndex = (uint)RenderQueueIndex.Geometry;

                if (flags.HasFlag(ModelMaterialShaderFlags.AlphaTest))
                {
                    component.QueueIndex = (uint)RenderQueueIndex.AlphaTest;
                }

                if (flags.HasFlag(ModelMaterialShaderFlags.DepthAlways))
                {
                    component.QueueIndex = (uint)RenderQueueIndex.GeometryLast;
                }

                if (flags.HasFlag(ModelMaterialShaderFlags.Transparent))
                {
                    component.QueueIndex = (uint)RenderQueueIndex.Transparency;
                }
                component.GameObject.SendUpdateTransformed();
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }

        public Job UpdateMaterials()
        {
            return Job.Run("Model Material Load Job", this, state =>
            {
                if (state is not MeshRendererComponent component)
                {
                    return;
                }

                if (component.GameObject == null)
                {
                    return;
                }

                if (component.model == null)
                {
                    return;
                }

                component.model.ReloadMaterials();

                var flags = component.model.ShaderFlags;
                component.QueueIndex = (uint)RenderQueueIndex.Geometry;

                if (flags.HasFlag(ModelMaterialShaderFlags.AlphaTest))
                {
                    component.QueueIndex = (uint)RenderQueueIndex.AlphaTest;
                }

                if (flags.HasFlag(ModelMaterialShaderFlags.DepthAlways))
                {
                    component.QueueIndex = (uint)RenderQueueIndex.GeometryLast;
                }

                if (flags.HasFlag(ModelMaterialShaderFlags.Transparent))
                {
                    component.QueueIndex = (uint)RenderQueueIndex.Transparency;
                }
                component.GameObject.SendUpdateTransformed();
                component.Invalidate();
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }

        public float Distance(Vector3 position)
        {
            return Vector3.Distance(position, GameObject.Transform.GlobalPosition);
        }

        public void SetLODLevel(int level)
        {
            currentLODLevel = level;
            model?.SetLOD(level);
        }
    }
}