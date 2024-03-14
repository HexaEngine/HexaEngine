namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public enum Stretch
    {
        Fill,
        None,
        Uniform,
        UniformFill,
    }

    public enum StretchDirection
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = Horizontal | Vertical,
    }

    public class Image : UIElement
    {
        public ImageSource? ImageSource { get; set; }

        public Stretch Stretch { get; set; } = Stretch.Fill;

        public StretchDirection StretchDirection { get; set; } = StretchDirection.Both;

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            return default;
        }

        public override void OnRender(UICommandList commandList)
        {
            if (ImageSource == null)
            {
                return;
            }

            Vector2 contentSize = new(ActualWidth, ActualHeight);
            Vector2 imageSize = new(ImageSource.Width, ImageSource.Height);

            Vector2 size = Vector2.Zero;

            switch (Stretch)
            {
                case Stretch.Fill:
                    size = contentSize;
                    break;

                case Stretch.None:
                    size = imageSize;
                    break;

                case Stretch.Uniform:
                    float ratio = Math.Min(contentSize.X / imageSize.X, contentSize.Y / imageSize.Y);
                    size = imageSize * ratio;
                    break;

                case Stretch.UniformFill:
                    float ratioFill = Math.Max(contentSize.X / imageSize.X, contentSize.Y / imageSize.Y);
                    size = imageSize * ratioFill;
                    break;
            }

            if (StretchDirection.HasFlag(StretchDirection.Horizontal))
            {
                size.X = Stretch == Stretch.None ? Math.Min(size.X, contentSize.X) : size.X;
            }

            if (StretchDirection.HasFlag(StretchDirection.Vertical))
            {
                size.Y = Stretch == Stretch.None ? Math.Min(size.Y, contentSize.Y) : size.Y;
            }

            Vector2 position = Vector2.Zero;
            if (StretchDirection.HasFlag(StretchDirection.Horizontal))
            {
                position.X = (contentSize.X - size.X) / 2;
            }

            if (StretchDirection.HasFlag(StretchDirection.Vertical))
            {
                position.Y = (contentSize.Y - size.Y) / 2;
            }

            ImageSource.DrawImage(commandList, position, size);
        }
    }
}