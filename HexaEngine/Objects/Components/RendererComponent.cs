namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;

    public class RendererComponent : IComponent
    {
        private readonly List<Model> models = new();
        private readonly List<ModelInstance> instances = new();
        private GameObject? gameObject;
        private bool initialized;

        public RendererComponent()
        {
            Editor = new RendererComponentEditor(this);
        }

        public IPropertyEditor? Editor { get; }

        public IReadOnlyList<Model> Meshes => models;

        public void Awake(IGraphicsDevice device, GameObject node)
        {
            gameObject = node;
            initialized = true;
            for (int i = 0; i < models.Count; i++)
            {
                var model = models[i];
                if (model.Material != null)
                {
                    node.GetScene().InstanceManager.CreateInstanceAsync(model, node.Transform).ContinueWith(t =>
                    {
                        instances.Add(t.Result);
                    });
                }
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
                if (model.Material != null)
                {
                    instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject.Transform));
                }
            }
        }

        public void UpdateModel(Model model)
        {
            if (model.Material != null)
            {
                instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject.Transform));
            }
        }

        public void RemoveMesh(Model model)
        {
            models.Remove(model);
            if (initialized)
            {
                if (model.Material != null)
                {
                    instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject.Transform));
                }
            }
        }

        private class RendererComponentEditor : IPropertyEditor
        {
            private readonly RendererComponent component;
            private int currentMesh;

            public RendererComponentEditor(RendererComponent component)
            {
                Type = typeof(RendererComponent);
                Name = "Renderer";
                this.component = component;
            }

            public Type Type { get; }

            public string Name { get; }

            public void Draw()
            {
                var scene = SceneManager.Current;
                for (int i = 0; i < component.models.Count; i++)
                {
                    Model model = component.models[i];
                    ImGui.Text($"{model.Mesh.Name}, {model.Material.Name}");
                }
            }
        }
    }
}