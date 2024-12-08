namespace HexaEngine.Editor.ImagePainter
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public class ToolContext
    {
        private readonly ImagePainterWindow window;
        private Vector2 lastPosition;
        private Vector2 position;

        public ToolContext(ImagePainterWindow window)
        {
            this.window = window;
        }

        public bool FirstTouch { get; set; }

        public ImagePainterWindow Window => window;

        public ImageSource ImageSource => window.Source ?? throw new ArgumentNullException(nameof(window));

        public Vector4 BrushColor { get => Window.BrushColor; set => Window.BrushColor = value; }

        public Vector2 Position
        {
            get => position;
            internal set
            {
                if (FirstTouch)
                {
                    lastPosition = value;
                    FirstTouch = false;
                }
                else
                {
                    lastPosition = position;
                }
                position = value;
            }
        }

        public Vector2 LastPosition => lastPosition;

        public Vector2 Ratio { get; internal set; }

        public Viewport ComputeViewport(Vector2 brushSize)
        {
            var size = brushSize;
            var pos = Position * Ratio - size / 2f;
            return new Viewport(pos, size);
        }
    }
}