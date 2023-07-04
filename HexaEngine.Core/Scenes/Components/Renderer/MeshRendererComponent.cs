namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using Newtonsoft.Json;
    using System.Numerics;
    using System;
    using System.Threading.Tasks;

    [EditorComponent(typeof(MeshRendererComponent), "Mesh Renderer")]
    public class MeshRendererComponent : IRendererComponent
    {
        private string modelPath = string.Empty;

        private GameObject? gameObject;
        private ModelManager modelManager;
        private MaterialManager materialManager;
        private MeshRenderer renderer;
        private Model? model;

        static MeshRendererComponent()
        {
        }

        [EditorProperty("Model", null, "*.mesh")]
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
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public RendererFlags Flags { get; } = RendererFlags.All;

        [JsonIgnore]
        public BoundingBox BoundingBox { get => BoundingBox.Transform(model?.BoundingBox ?? BoundingBox.Empty, gameObject.Transform); }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;

            modelManager = gameObject.GetScene().ModelManager;
            materialManager = gameObject.GetScene().MaterialManager;

            renderer = new(device);

            UpdateModel();
        }

        public void Destory()
        {
            renderer.Dispose();
            model?.Dispose();
        }

        public void Update(IGraphicsContext context)
        {
            if (!gameObject.IsEnabled)
                return;
            renderer.Update(context, gameObject.Transform.Global);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!gameObject.IsEnabled)
                return;
            renderer.DrawDepth(context);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!gameObject.IsEnabled)
                return;
            renderer.DrawShadowMap(context, light, type);
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
        }

        public void Draw(IGraphicsContext context)
        {
            if (!gameObject.IsEnabled)
                return;
            renderer.Draw(context);
        }

        private void UpdateModel()
        {
            renderer?.Uninitialize();
            var tmpModel = model;
            model = null;
            tmpModel?.Dispose();

            Task.Factory.StartNew(async state =>
            {
                if (state is not MeshRendererComponent component)
                {
                    return;
                }

                if (component.gameObject == null)
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
                    component.gameObject.SendUpdateTransformed();
                }
            }, this);
        }
    }
}