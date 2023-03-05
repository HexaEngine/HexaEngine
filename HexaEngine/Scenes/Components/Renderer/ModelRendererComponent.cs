namespace HexaEngine.Core.Renderers.Components
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Editor.Properties;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Editor.Properties;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;

    [EditorComponent(typeof(ModelRendererComponent), "Model Renderer")]
    public class ModelRendererComponent : IComponent
    {
        private string model = string.Empty;
        private readonly List<IInstance> instances = new();
        private GameObject? gameObject;
        private ModelRenderer renderer;

        static ModelRendererComponent()
        {
            ObjectEditorFactory.RegisterEditor(typeof(ModelRendererComponent), new ModelRendererComponentEditor());
        }

        public string Model { get => model; set => model = value; }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;
            if (!gameObject.GetScene().TryGetSystem<RenderManager>(out var manager))
                return;
            renderer = manager.GetRenderer<ModelRenderer>();

            ModelSource source = ModelSource.Load(model);
            for (ulong i = 0; i < source.Header.MeshCount; i++)
            {
                var mesh = source.GetMesh(i);
                renderer.CreateInstanceAsync(mesh, gameObject).ContinueWith(t =>
                {
                    instances.Add(t.Result);
                });
            }
        }

        public void Destory()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                renderer.DestroyInstance(instances[i]);
            }
            instances.Clear();
        }

        private class ModelRendererComponentEditor : IObjectEditor
        {
            private object? instance;

            public ModelRendererComponentEditor()
            {
                Type = typeof(ModelRendererComponent);
                Name = "Model Renderer";
            }

            public Type Type { get; }

            public string Name { get; }

            public object? Instance { get => instance; set => instance = value; }

            public bool IsEmpty => false;

            public void Draw()
            {
                if (instance == null) return;
                ModelRendererComponent component = (ModelRendererComponent)instance;
                ImGui.Text($"{component.model}");
            }
        }
    }
}