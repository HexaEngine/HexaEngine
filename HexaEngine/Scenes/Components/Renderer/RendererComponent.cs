namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Properties;
    using ImGuiNET;
    using System;

    [EditorComponent(typeof(RendererComponent), "Mesh Renderer")]
    public class RendererComponent : IComponent
    {
        private readonly List<Model> models = new();
        private readonly List<ModelInstance> instances = new();
        private GameObject? gameObject;
        private Scene scene;
        private bool initialized;

        static RendererComponent()
        {
            ObjectEditorFactory.RegisterEditor(typeof(RendererComponent), new RendererComponentEditor());
        }

        public List<Model> Meshes => models;

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            scene = gameObject.GetScene();
            scene.MaterialManager.Renamed += MaterialRenamed;

            initialized = true;
            for (int i = 0; i < models.Count; i++)
            {
                var model = models[i];
                scene.InstanceManager.CreateInstanceAsync(model, gameObject).ContinueWith(t =>
                {
                    instances.Add(t.Result);
                });
            }
        }

        private void MaterialRenamed(MaterialDesc material, string oldName, string newName)
        {
            for (int i = 0; i < models.Count; i++)
            {
                var model = models[i];
                if (model.Material == oldName)
                {
                    model.Material = newName;
                }
            }
        }

        public void Destory()
        {
            scene.MaterialManager.Renamed -= MaterialRenamed;
            initialized = false;
            for (int i = 0; i < instances.Count; i++)
            {
                gameObject.GetScene().InstanceManager.DestroyInstance(instances[i]);
            }
            instances.Clear();
        }

        public void AddMesh(Model model)
        {
            models.Add(model);
            if (initialized)
            {
                instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject));
            }
        }

        public void UpdateModel(Model model)
        {
            instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject));
        }

        public void RemoveMesh(Model model)
        {
            models.Remove(model);
            if (initialized)
            {
                instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject));
            }
        }

        private class RendererComponentEditor : IObjectEditor
        {
            private object? instance;

            public RendererComponentEditor()
            {
                Type = typeof(RendererComponent);
                Name = "Mesh Renderer";
            }

            public Type Type { get; }

            public string Name { get; }

            public object? Instance { get => instance; set => instance = value; }

            public bool IsEmpty => false;

            public void Draw()
            {
                if (instance == null) return;
                RendererComponent component = (RendererComponent)instance;
                for (int i = 0; i < component.models.Count; i++)
                {
                    Model model = component.models[i];
                    ImGui.Text($"{model.Mesh}, {model.Material}");
                }
            }
        }
    }
}