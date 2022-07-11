namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using ImGuiNET;
    using ImGuizmoNET;
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
            ImGui.Begin("Framebuffer", ImGuiWindowFlags.NoBackground);
            position = ImGui.GetWindowPos();
            size = ImGui.GetWindowSize();
            if (Keyboard.IsDown(Keys.T))
                ImGui.SetWindowFocus();
            Viewport = new(position.X, position.Y, size.X, size.Y);
            ImGui.End();
        }
    }
}