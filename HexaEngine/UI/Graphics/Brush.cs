namespace HexaEngine.UI.Graphics
{
    using HexaEngine.Mathematics;

    public abstract class Brush : UIResource
    {
    }

    public sealed class SolidColorBrush : Brush
    {
        private Color color;

        public SolidColorBrush(Color color)
        {
            this.color = color;
        }

        public Color Color { get => color; set => color = value; }

        protected override void DisposeCore()
        {
        }
    }

    public class GradientBrush : Brush
    {
        protected override void DisposeCore()
        {
        }
    }

    public class ImageBrush : Brush
    {
        private ImageSource imageSource;

        public ImageBrush(ImageSource imageSource)
        {
            this.imageSource = imageSource;
        }

        public ImageSource ImageSource { get => imageSource; set => imageSource = value; }

        protected override void DisposeCore()
        {
            imageSource = null;
        }
    }
}