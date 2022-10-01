namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using ImGuizmoNET;
    using Silk.NET.SDL;
    using System;
    using System.Numerics;

    internal class Framebuffer
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

        public Framebuffer(IGraphicsDevice device)
        {
            this.device = device;
        }

        internal void Update(IGraphicsContext context)
        {
            ImGuizmo.SetRect(position.X, position.Y, size.X, size.Y);
        }

        internal void Draw()
        {
            ImGui.Begin("Framebuffer", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.MenuBar);
            if (ImGui.BeginMenuBar())
            {
                var scene = SceneManager.Current;
                if (scene is null)
                {
                    ImGui.EndMenuBar();
                    return;
                }
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
            if (Keyboard.IsDown(KeyCode.KT))
                ImGui.SetWindowFocus();
            float ratioX = size.X / SourceViewport.Width;
            float ratioY = size.Y / SourceViewport.Height;
            var s = Math.Min(ratioX, ratioY);
            var w = SourceViewport.Width * s;
            var h = SourceViewport.Height * s;
            var x = (size.X - w) / 2;
            var y = (size.Y - h) / 2;
            Viewport = new(position.X + x, position.Y + y, w, h);
            ImGui.End();
        }
    }
}