namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using ImGuizmoNET;
    using Silk.NET.SDL;
    using System;
    using System.Diagnostics;
    using System.Numerics;

    public class Framebuffer
    {
        private readonly IGraphicsDevice device;

        private Vector2 position;
        private Vector2 size;
        private bool isShown;

        public bool IsShown { get => isShown; set => isShown = value; }

        public Vector2 Position => position;

        public Vector2 Size => size;

        public Viewport Viewport;
        public Viewport SourceViewport;

        public static bool Fullframe;

        private static readonly Profiler fpsProfiler = new("latency", () => Time.Delta, x => $"{x * 1000:n4}ms\n({1000 / Time.Delta:n0}fps)", 100);
        private static readonly Profiler memProfiler = new("memory", () => Process.GetCurrentProcess().PrivateMemorySize64 / 1000f / 1000f, x => $"{x}MB", 200);

        public Framebuffer(IGraphicsDevice device)
        {
            this.device = device;
        }

        internal void Update(IGraphicsContext context)
        {
            ImGuizmo.SetRect(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height);
        }

        internal void Draw()
        {
            ImGui.Begin("Framebuffer", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.MenuBar);
            var scene = SceneManager.Current;
            if (scene != null && ImGui.BeginMenuBar())
            {
                if (ImGui.Button(scene.IsSimulating ? "\xE769" : "\xE768"))
                    scene.IsSimulating = !scene.IsSimulating;

                int cameraIndex = scene.ActiveCamera;
                if (ImGui.Combo("Current Camera", ref cameraIndex, scene.Cameras.Select(x => x.Name).ToArray(), scene.Cameras.Count))
                {
                    scene.ActiveCamera = cameraIndex;
                }
                ImGui.EndMenuBar();
            }
            position = ImGui.GetWindowPos();
            size = ImGui.GetWindowSize();

            if (Fullframe)
            {
                Viewport = SourceViewport;
            }
            else
            {
                float ratioX = size.X / SourceViewport.Width;
                float ratioY = size.Y / SourceViewport.Height;
                var s = Math.Min(ratioX, ratioY);
                var w = SourceViewport.Width * s;
                var h = SourceViewport.Height * s;
                var x = (size.X - w) / 2;
                var y = (size.Y - h) / 2;
                Viewport = new(position.X + x, position.Y + y, w, h);
            }

            fpsProfiler.Draw();
            memProfiler.Draw();

            ImGui.End();
        }
    }
}