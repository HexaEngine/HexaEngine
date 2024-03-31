namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Jobs;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using HexaEngine.Scenes.Managers;
    using Newtonsoft.Json;
    using System.Numerics;

    [EditorCategory("Renderer")]
    [EditorComponent(typeof(SkinnedMeshRendererComponent), "Skinned Mesh Renderer")]
    public class SkinnedMeshRendererComponent : BaseRendererComponent
    {
        private AssetRef modelAsset;

        private ModelManager modelManager;
        private MaterialManager materialManager;
        private SkinnedMeshRenderer renderer;
        private SkinnedModel? model;
        private readonly MaterialAssetMappingCollection materials = new();

        static SkinnedMeshRendererComponent()
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

        [EditorCategory("Materials")]
        [EditorProperty("")]
        public MaterialAssetMappingCollection Materials => materials;

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(SkinnedMeshRenderer);

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Clustered | RendererFlags.Deferred | RendererFlags.Forward;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get => BoundingBox.Transform(model?.BoundingBox ?? BoundingBox.Empty, GameObject.Transform); }

        public void SetLocal(Matrix4x4 local, uint nodeId)
        {
            model?.SetLocal(local, nodeId);
            GameObject.SendUpdateTransformed();
        }

        public Matrix4x4 GetLocal(uint nodeId)
        {
            if (model == null)
                return Matrix4x4.Identity;
            return model.Locals[nodeId];
        }

        public void SetBoneLocal(Matrix4x4 local, uint boneId)
        {
            model?.SetBoneLocal(local, boneId);
            GameObject.SendUpdateTransformed();
        }

        public Matrix4x4 GetBoneLocal(uint boneId)
        {
            if (model == null)
                return Matrix4x4.Identity;
            return model.BoneLocals[boneId];
        }

        public int GetNodeIdByName(string name)
        {
            if (model == null)
                return -1;
            return model.GetNodeIdByName(name);
        }

        public int GetBoneIdByName(string name)
        {
            if (model == null)
                return -1;
            return model.GetBoneIdByName(name);
        }

        public Node[]? GetNodes()
        {
            return model?.Nodes;
        }

        public PlainNode[]? GetPlainNodes()
        {
            return model?.PlainNodes;
        }

        public PlainNode[]? GetPlainBones()
        {
            return model?.Bones;
        }

        protected override void LoadCore(IGraphicsDevice device)
        {
            modelManager = GameObject.GetScene().ModelManager;
            materialManager = GameObject.GetScene().MaterialManager;

            renderer = new(device);

            UpdateModel();
        }

        protected override void UnloadCore()
        {
            renderer.Dispose();
            model?.Dispose();
        }

        public override void Update(IGraphicsContext context)
        {
            renderer.Update(context, GameObject.Transform.Global);
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

        public override void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        private void OnChanged(MaterialAssetMapping mapping)
        {
            UpdateModel();
        }

        private Job UpdateModel()
        {
            Loaded = false;
            renderer?.Uninitialize();
            var tmpModel = model;
            model = null;
            tmpModel?.Dispose();

            return Job.Run("Skinned Model Load", this, state =>
            {
                if (state is not SkinnedMeshRendererComponent component)
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

                var stream = modelAsset.OpenRead();
                if (stream != null)
                {
                    component.Materials.OnChanged -= component.OnChanged;
                    component.model = new(stream, component.materials);
                    component.Materials.OnChanged += component.OnChanged;
                    component.model.LoadAsync().Wait();

                    var flags = component.model.ShaderFlags;
                    component.QueueIndex = (uint)RenderQueueIndex.Geometry;

                    if (flags.HasFlag(ModelMaterialShaderFlags.AlphaTest))
                    {
                        component.QueueIndex = (uint)RenderQueueIndex.AlphaTest;
                    }

                    if (flags.HasFlag(ModelMaterialShaderFlags.Transparent))
                    {
                        component.QueueIndex = (uint)RenderQueueIndex.Transparency;
                    }

                    component.renderer.Initialize(component.model);
                    component.Loaded = true;
                    component.GameObject.SendUpdateTransformed();
                }
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }
    }
}