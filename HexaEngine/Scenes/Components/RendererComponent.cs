namespace HexaEngine.Scenes.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Objects;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using System;

    public class RendererComponent : IComponent
    {
        private readonly List<Model> models = new();
        private readonly List<ModelInstance> instances = new();
        private GameObject? gameObject;
        private bool initialized;

        static RendererComponent()
        {
            ObjectEditorFactory.RegisterEditor(typeof(RendererComponent), new RendererComponentEditor());
        }

        public List<Model> Meshes => models;

        public void Awake(IGraphicsDevice device, GameObject node)
        {
            gameObject = node;
            initialized = true;
            for (int i = 0; i < models.Count; i++)
            {
                var model = models[i];
                node.GetScene().InstanceManager.CreateInstanceAsync(model, node).ContinueWith(t =>
                {
                    instances.Add(t.Result);
                });
            }
        }

        public void Destory()
        {
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
                Name = "Renderer";
            }

            public Type Type { get; }

            public string Name { get; }
            public object? Instance { get => instance; set => instance = value; }

            public void Draw()
            {
                if (instance == null) return;
                RendererComponent component = (RendererComponent)instance;
                for (int i = 0; i < component.models.Count; i++)
                {
                    Model model = component.models[i];
                    ImGui.Text($"{model.Mesh.Name}, {model.Material.Name}");
                }
            }
        }
    }
}