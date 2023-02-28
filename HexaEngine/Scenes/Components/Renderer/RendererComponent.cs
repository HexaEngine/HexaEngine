namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Properties;
    using ImGuiNET;
    using System;

    [EditorComponent(typeof(RendererComponent), "Mesh Renderer")]
    public class RendererComponent : IComponent
    {
        private readonly List<string> models = new();
        private readonly List<ModelInstance> instances = new();
        private GameObject? gameObject;
#pragma warning disable CS8618 // Non-nullable field 'scene' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private Scene scene;
#pragma warning restore CS8618 // Non-nullable field 'scene' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private bool initialized;

        static RendererComponent()
        {
            ObjectEditorFactory.RegisterEditor(typeof(RendererComponent), new RendererComponentEditor());
        }

        public List<string> Meshes => models;

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            scene = gameObject.GetScene();

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

        public void Destory()
        {
            initialized = false;
            for (int i = 0; i < instances.Count; i++)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                gameObject.GetScene().InstanceManager.DestroyInstance(instances[i]);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            instances.Clear();
        }

        public void AddMesh(string model)
        {
            models.Add(model);
            if (initialized)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        public void UpdateModel(string model)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public void RemoveMesh(string model)
        {
            models.Remove(model);
            if (initialized)
            {
                throw new NotImplementedException();
                // instances.Add(gameObject.GetScene().InstanceManager.CreateInstance(model, gameObject));
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
                    string model = component.models[i];
                    ImGui.Text($"{model}, {model}");
                }
            }
        }
    }
}