namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Xml.Serialization;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class Image : FrameworkElement
    {
        public static readonly DependencyProperty<ImageSource> ImageSourceProperty = DependencyProperty.Register<Image, ImageSource>(nameof(ImageSource), false);

        [XmlIgnore]
        public ImageSource? ImageSource { get => GetValue(ImageSourceProperty); set => SetValue(ImageSourceProperty, value); }

        public static readonly DependencyProperty<Stretch> StretchProperty = DependencyProperty.Register<Image, Stretch>(nameof(Stretch), false, new PropertyMetadata(Stretch.Fill));

        public Stretch Stretch { get => GetValue(StretchProperty); set => SetValue(StretchProperty, value); }

        public static readonly DependencyProperty<StretchDirection> StretchDirectionProperty = DependencyProperty.Register<Image, StretchDirection>(nameof(StretchDirection), false, new PropertyMetadata(StretchDirection.Both));

        public StretchDirection StretchDirection { get => GetValue(StretchDirectionProperty); set => SetValue(StretchDirectionProperty, value); }

        protected override void OnRender(UICommandList commandList)
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