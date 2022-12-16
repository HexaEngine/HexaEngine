namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Reflection;

    public class RendererComponent : IComponent
    {
        private readonly List<int> meshes = new();
        private GameObject? gameObject;
        private bool initialized;

        public RendererComponent()
        {
            Editor = new RendererComponentEditor(this);
        }

        public IPropertyEditor? Editor { get; }

        public IReadOnlyList<int> Meshes => meshes;

        public void Awake(IGraphicsDevice device, GameObject node)
        {
            gameObject = node;
            initialized = true;
        }

        public void Destory()
        {
            initialized = false;
        }

        public void AddMesh(int mesh)
        {
            meshes.Add(mesh);
            if (initialized)
                gameObject?.GetScene().CommandQueue.Enqueue(new SceneCommand(CommandType.Update, gameObject, ChildCommandType.Added, mesh));
        }

        public void RemoveMesh(int mesh)
        {
            meshes.Remove(mesh);
            if (initialized)
                gameObject?.GetScene().CommandQueue.Enqueue(new SceneCommand(CommandType.Update, gameObject, ChildCommandType.Removed, mesh));
        }

        private class RendererComponentEditor : IPropertyEditor
        {
            private readonly RendererComponent component;
            private int currentMesh;

            public RendererComponentEditor(RendererComponent component)
            {
                Type = typeof(RendererComponent);
                Properties = Type.GetProperties();
                Name = "Renderer";
                this.component = component;
            }

            public PropertyInfo[] Properties { get; }

            public Type Type { get; }

            public string Name { get; }

            public void Draw()
            {
                var scene = SceneManager.Current;
                if (ImGui.Button("Add mesh"))
                {
                    if (currentMesh > -1 && currentMesh < scene.Meshes.Count)
                    {
                        component.AddMesh(currentMesh);
                    }
                }
                ImGui.SameLine();
                ImGui.Combo("Mesh", ref currentMesh, scene.Meshes.Select(x => x.Name).ToArray(), scene.Meshes.Count);

                for (int i = 0; i < component.Meshes.Count; i++)
                {
                    var mesh = component.Meshes[i];
                    ImGui.Text(scene.Meshes[mesh].Name);
                    ImGui.SameLine();
                    if (ImGui.Button("Remove Mesh"))
                    {
                        component.RemoveMesh(mesh);
                    }
                }
            }
        }
    }
}