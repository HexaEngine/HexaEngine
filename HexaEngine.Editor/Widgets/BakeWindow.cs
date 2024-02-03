namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Scenes;
    using System.Threading;
    using System.Threading.Tasks;

    public class BakeWindow : EditorWindow
    {
        private MeshBaker baker = new();
        private Task? task;
        private CancellationTokenSource cancellationToken = new();

        protected override string Name { get; } = "Bake";

        protected override void InitWindow(IGraphicsDevice device)
        {
            base.InitWindow(device);
            baker.Initialize(device);
        }

        public override void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;
            if (scene == null)
                return;

            if (task != null && !task.IsCompleted)
            {
                if (ImGui.Button("Cancel"))
                {
                    cancellationToken.Cancel();
                }
            }
            else
            {
                if (ImGui.Button("Bake"))
                {
                    Bake(context);
                }
            }

            var stats = MeshBaker.Statistics;

            if (stats.Running)
            {
                ImGui.Text($"Baking Radiosity");
                ImGui.Text($"Bounce {stats.CurrentBounce} of {stats.BounceCount}");
                ImGui.Text($"Mesh {stats.CurrentMesh} of {stats.MeshCount}");
                ImGui.Text($"Vertex {stats.CurrentVertex} of {stats.VertexCount}");
            }
        }

        public void Bake(IGraphicsContext context)
        {
            if (task != null && !task.IsCompleted)
            {
                return;
            }

            task = null;

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            var sky = renderers.Renderers.FirstOrDefault(x => x is SkyRendererComponent);

            if (sky == null)
            {
                Logger.Error("Failed to bake, no sky found!");
                return;
            }

            cancellationToken = new();

            task = Task.Run(async () =>
            {
                foreach (var renderer in renderers.Renderers)
                {
                    if (renderer is MeshRendererComponent rendererComponent && rendererComponent.ModelInstance != null)
                    {
                        await baker.BakeAsync(context.Device, context, SceneRenderer.Current, rendererComponent.ModelInstance, rendererComponent, (SkyRendererComponent)sky, cancellationToken.Token);
                    }
                }
            });
        }
    }
}