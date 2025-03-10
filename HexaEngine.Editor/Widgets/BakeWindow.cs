﻿namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Logging;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using System.Threading;
    using System.Threading.Tasks;

    public class BakeWindow : EditorWindow
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger("Baking");
        private MeshBaker2 baker = null!;
        private Task? task;
        private CancellationTokenSource cancellationToken = new();

        protected override string Name { get; } = $"{UwU.BreadSlice} Bake";

        protected override void InitWindow(IGraphicsDevice device)
        {
            base.InitWindow(device);
            baker = new();
        }

        public override void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            if (task != null && !task.IsCompleted)
            {
                if (ImGui.Button("Cancel"u8))
                {
                    cancellationToken.Cancel();
                }
            }
            else
            {
                if (ImGui.Button("Bake"u8))
                {
                    Bake(context);
                }
            }

            for (int i = 0; i < 6; i++)
            {
                ImGui.Image((ulong)baker.Texture.SRVArraySlices![i].NativePointer, new(256));
                if (i < 5)
                {
                    ImGui.SameLine();
                }
            }

            for (int i = 0; i < 6; i++)
            {
                ImGui.Image((ulong)baker.TextureFinal.SRVArraySlices![i].NativePointer, new(256));
                if (i < 5)
                {
                    ImGui.SameLine();
                }
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

            var sky = renderers.Drawables.FirstOrDefault(x => x is SkyRendererComponent);

            if (sky == null)
            {
                Logger.Error("Failed to bake, no sky found!");
                return;
            }

            cancellationToken = new();

            task = Task.Run(() =>
            {
                foreach (var renderer in renderers.Drawables)
                {
                    if (renderer is MeshRendererComponent rendererComponent && rendererComponent.ModelInstance != null)
                    {
                        rendererComponent.GameObject.IsEnabled = false;
                        baker.Bake(Application.MainWindow.Dispatcher, context, rendererComponent.GameObject.Transform, rendererComponent.ModelInstance, SceneRenderer.Current);
                        rendererComponent.GameObject.IsEnabled = true;
                    }
                }
            });
        }
    }
}