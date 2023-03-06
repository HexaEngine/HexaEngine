namespace HexaEngine.Core.Renderers.Components
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using System.Collections.Generic;

    [EditorComponent(typeof(ModelRendererComponent), "Model Renderer")]
    public class ModelRendererComponent : IComponent
    {
        private string model = string.Empty;
        private readonly List<IInstance> instances = new();
        private GameObject? gameObject;
        private ModelManager modelManager;
        private ModelRenderer renderer;

        static ModelRendererComponent()
        {
        }

        [EditorProperty("Model", null, "*.mesh")]
        public string Model
        {
            get => model;
            set
            {
                model = value;
                UpdateModel();
            }
        }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            if (!gameObject.GetScene().TryGetSystem<RenderManager>(out var manager))
                return;
            modelManager = gameObject.GetScene().ModelManager;
            renderer = manager.GetRenderer<ModelRenderer>();
            UpdateModel();
        }

        public void Destory()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                renderer.DestroyInstance(instances[i]);
            }
            instances.Clear();
        }

        private void UpdateModel()
        {
            Task.Factory.StartNew(async state =>
            {
                if (state is not ModelRendererComponent component)
                    return;
                if (component.gameObject == null)
                    return;
                lock (component.instances)
                {
                    for (int i = 0; i < component.instances.Count; i++)
                    {
                        component.renderer.DestroyInstance(component.instances[i]);
                    }
                }
                component.instances.Clear();
                var path = Paths.CurrentAssetsPath + component.model;
                if (FileSystem.Exists(path))
                {
                    ModelSource source = component.modelManager.Load(path);

                    for (ulong i = 0; i < source.Header.MeshCount; i++)
                    {
                        var mesh = source.GetMesh(i);
                        var instance = await component.renderer.CreateInstanceAsync(mesh, component.gameObject);
                        lock (component.instances)
                        {
                            component.instances.Add(instance);
                        }
                    }
                }
            }, this);
        }
    }
}