namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.ImGuiNET;

    internal class HeightToNormalDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;

        public override string Name { get; } = "Height map to normal map";

        protected override ImGuiWindowFlags Flags { get; }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        protected override void DrawContent()
        {
            throw new NotImplementedException();
        }
    }
}