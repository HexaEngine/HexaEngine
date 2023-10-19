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

    [EditorComponent(typeof(MeshRendererComponent), "Mesh Renderer")]
    public class MeshRendererComponent : BaseRendererComponent
    {
        private string modelPath = string.Empty;

        private ModelManager modelManager;
        private MaterialManager materialManager;
        private MeshRenderer renderer;
        private Model? model;

        static MeshRendererComponent()
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
        public override string DebugName { get; protected set; } = nameof(MeshRenderer);

        [JsonIgnore]
        public override uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Clustered | RendererFlags.Deferred | RendererFlags.Forward;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get => BoundingBox.Transform(model?.BoundingBox ?? BoundingBox.Empty, GameObject?.Transform ?? Matrix4x4.Identity); }

        [JsonIgnore]
        public Matrix4x4 Transform => GameObject?.Transform ?? Matrix4x4.Identity;

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

        public void DrawDepth(IGraphicsContext context, IBuffer cam)
        {
            renderer.DrawDepth(context, cam);
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

                var path = Paths.CurrentAssetsPath + component.modelPath;
                if (FileSystem.Exists(path))
                {
                    ModelFile source = component.modelManager.Load(path);
                    MaterialLibrary library = component.materialManager.Load(Paths.CurrentMaterialsPath + source.MaterialLibrary);

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