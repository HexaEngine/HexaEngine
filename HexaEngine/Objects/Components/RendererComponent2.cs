namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using System;

    public class RendererComponent2 : IComponent, IDrawable
    {
        private readonly List<int> meshes = new();
        private string name;
        private Mesh mesh;
        private GameObject? gameObject;
        private bool initialized;

        public RendererComponent2()
        {
            Editor = new RendererComponentEditor2(this);
        }

        public IPropertyEditor? Editor { get; }

        public IReadOnlyList<int> Meshes => meshes;

        public void Awake(IGraphicsDevice device, GameObject node)
        {
            mesh = GraphicsManager.Resources.LoadMesh(name);
            GraphicsManager.Queue.Enqueue(RenderQueueIndex.Opaque, this);
            gameObject = node;
            initialized = true;
        }

        public void Destory()
        {
            GraphicsManager.Queue.Dequeue(RenderQueueIndex.Opaque, this);

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

        public void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void DrawDepth(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void DrawBackDepth(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Draw(IGraphicsContext context, RenderQueueIndex index)
        {
            throw new NotImplementedException();
        }

        private class RendererComponentEditor2 : IPropertyEditor
        {
            private readonly RendererComponent2 component;
            private int currentMesh;

            public RendererComponentEditor2(RendererComponent2 component)
            {
                Type = typeof(RendererComponent2);
                Name = "Renderer";
                this.component = component;
            }

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