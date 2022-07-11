namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Rendering;
    using ImGuiNET;
    using System.Collections.Generic;
    using System.Numerics;

    public static class FramebufferDebugger
    {
        private static readonly List<IShaderResourceView> views = new();
        private static float aspectRatio = 1920f / 1080f;
        private static float invAspectRatio = 1f / (1920f / 1080f);
        private static bool isShown;

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static float AspectRatio
        { get => aspectRatio; set { aspectRatio = value; invAspectRatio = 1 / value; } }

        public static void Add(IShaderResourceView view)
        {
            ImGuiRenderer.RegisterTexture(view);
            views.Add(view);
        }

        public static void AddRange(IEnumerable<IShaderResourceView> views)
        {
            foreach (var view in views)
                ImGuiRenderer.RegisterTexture(view);
            FramebufferDebugger.views.AddRange(views);
        }

        public static void Remove(IShaderResourceView view)
        {
            ImGuiRenderer.UnregisterTexture(view);
            views.Remove(view);
        }

        public static void Clear()
        {
            foreach (IShaderResourceView view in views)
            {
                ImGuiRenderer.UnregisterTexture(view);
            }
            views.Clear();
        }

        internal static void Draw()
        {
            if (!ImGui.Begin("Framebuffers", ref isShown, ImGuiWindowFlags.MenuBar))
            {
                ImGui.End();
                return;
            }
            var sizeMax = ImGui.GetWindowContentRegionMax();
            var sizeMin = ImGui.GetWindowContentRegionMin();
            var size = sizeMax - sizeMin;
            var min = ImGui.GetWindowPos();
            var max = ImGui.GetWindowSize() - new Vector2(0, 20);
            size.X -= 20;
            for (int i = 0; i < views.Count; i++)
            {
                ImGui.PushClipRect(min, min + max, true);
                ImGui.Text(views[i].DebugName);
                ImGui.Image(views[i].NativePointer, new(size.X, size.X * invAspectRatio));
                ImGui.PopClipRect();
            }

            ImGui.End();
        }
    }
}