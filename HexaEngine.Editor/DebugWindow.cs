namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Unsafes;
    using System.Numerics;

    public class DebugWindow : EditorWindow
    {
        private BezierCurve curve = new(Vector2.Zero, Vector2.One);

        protected override string Name => "Debug";

        public DebugWindow()
        {
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
#if DEBUG
            ImGuiBezierWidget.Bezier("Test Bezier", ref curve);

#endif
        }
    }
}