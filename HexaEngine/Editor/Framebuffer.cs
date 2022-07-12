namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using ImGuizmoNET;
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
            ImGui.Begin("Framebuffer", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove);
            position = ImGui.GetWindowPos();
            size = ImGui.GetWindowSize();
            if (Keyboard.IsDown(Keys.T))
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