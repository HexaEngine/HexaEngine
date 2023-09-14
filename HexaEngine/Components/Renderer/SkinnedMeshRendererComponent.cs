namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Culling;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Renderers;
    using HexaEngine.Scenes.Managers;
    using Newtonsoft.Json;
    using System.Numerics;
    using System.Threading.Tasks;

    [EditorComponent(typeof(SkinnedMeshRendererComponent), "Skinned Mesh Renderer")]
    public class SkinnedMeshRendererComponent : BaseRendererComponent
    {
        private string modelPath = string.Empty;

        private ModelManager modelManager;
        private MaterialManager materialManager;
        private SkinnedMeshRenderer renderer;
        private SkinnedModel? model;

        static SkinnedMeshRendererComponent()
        {
        }

        [EditorProperty("Model", null, ".model")]
        public string Model
        {
            get => modelPath;
            set
            {
                modelPath = value;
                UpdateModel();
            }
        }

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(SkinnedMeshRenderer);

        [JsonIgnore]
        public override uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

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

        public override void Load(IGraphicsDevice device)
        {
            modelManager = GameObject.GetScene().ModelManager;
            materialManager = GameObject.GetScene().MaterialManager;

            renderer = new(device);

            UpdateModel().Wait();
        }

        public override void Unload()
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

        private Task UpdateModel()
        {
            loaded = false;
            renderer?.Uninitialize();
            var tmpModel = model;
            model = null;
            tmpModel?.Dispose();

            return Task.Factory.StartNew(async state =>
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

                var path = Paths.CurrentAssetsPath + component.modelPath;
                if (FileSystem.Exists(path))
                {
                    ModelFile source = component.modelManager.Load(path);
                    MaterialLibrary library = component.materialManager.Load(Paths.CurrentMaterialsPath + source.MaterialLibrary + ".matlib");

                    component.model = new(source, library);
                    await component.model.LoadAsync();
                    component.renderer.Initialize(component.model);
                    component.loaded = true;
                    component.GameObject.SendUpdateTransformed();
                }
            }, this);
        }
    }
}