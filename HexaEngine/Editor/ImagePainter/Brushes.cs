namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor;
    using ImGuiNET;

    public class Brushes : ImGuiWindow
    {
        private static readonly List<BrushMask> masks = new();
        private IGraphicsDevice device;
        private BrushMask? current;

        protected override string Name => "Masks";

        public BrushMask? Current => current;

        public override void Init(IGraphicsDevice device)
        {
            this.device = device;
            string[] brushes = FileSystem.GetFiles("assets/textures/brushes");
            for (int i = 0; i < brushes.Length; i++)
            {
                var brush = brushes[i];
                masks.Add(new(device, brush));
            }

            current = masks.FirstOrDefault();
        }

        public override void DrawContent(IGraphicsContext context)
        {
            ImGui.BeginListBox("Masks");
            for (int i = 0; i < masks.Count; i++)
            {
                var mask = masks[i];
                var cur = ImGui.GetCursorPos();

                var isActive = current == mask;

                if (isActive)
                    ImGui.BeginDisabled();

                mask.DrawPreview(new(32));

                ImGui.SetCursorPos(cur);
                if (ImGui.InvisibleButton($"Mask{i}", new(32)))
                {
                    current = mask;
                }

                if (isActive)
                    ImGui.EndDisabled();

                if (i % 4 != 0 || i == 0)
                {
                    ImGui.SameLine();
                }
            }
            ImGui.EndListBox();
        }

        public override void Dispose()
        {
            for (int i = 0; i < masks.Count; i++)
            {
                masks[i].Dispose();
            }
            base.Dispose();
        }

        public static void Register<T>(T mask) where T : BrushMask
        {
            lock (masks)
            {
                masks.Add(mask);
            }
        }

        public static bool Unregister<T>(T mask) where T : BrushMask
        {
            lock (masks)
            {
                return masks.Remove(mask);
            }
        }

        public static bool IsRegistered<T>(T mask) where T : BrushMask
        {
            lock (masks)
            {
                return masks.Contains(mask);
            }
        }
    }
}