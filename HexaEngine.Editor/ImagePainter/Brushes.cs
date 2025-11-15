namespace HexaEngine.Editor.ImagePainter
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor;
    using System.Numerics;

    public class Brushes : EditorWindow
    {
        private static readonly List<BrushMask> masks = new();
        private BrushMask? current;

        protected override string Name => "Masks";

        public BrushMask? Current => current;

        protected override void InitWindow(IGraphicsDevice device)
        {
            foreach (var brush in FileSystem.GetFiles("assets/textures/brushes"))
            {
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
                Vector2 cur = default;
                ImGui.GetCursorPos(ref cur);

                var isActive = current == mask;

                if (isActive)
                {
                    ImGui.BeginDisabled();
                }

                mask.DrawPreview(new(32));

                ImGui.SetCursorPos(cur);
                if (ImGui.InvisibleButton(mask.Id, new(32)))
                {
                    current = mask;
                }

                if (isActive)
                {
                    ImGui.EndDisabled();
                }

                if (i % 4 != 0 || i == 0)
                {
                    ImGui.SameLine();
                }
            }
            ImGui.EndListBox();
        }

        protected override void DisposeCore()
        {
            for (int i = 0; i < masks.Count; i++)
            {
                masks[i].Dispose();
            }
            base.DisposeCore();
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