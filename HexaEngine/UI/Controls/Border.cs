namespace HexaEngine.UI.Controls
{
    using Hexa.NET.Mathematics;
    using HexaEngine.UI.Graphics;
    using System.Diagnostics.CodeAnalysis;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public sealed class Border : Decorator
    {
        public static readonly DependencyProperty<Brush> BackgroundProperty = DependencyProperty.Register<Control, Brush>(nameof(Background), false, new(Brushes.Transparent));

        public Brush? Background { get => GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }

        public static readonly DependencyProperty<Brush> BorderBrushProperty = DependencyProperty.Register<Control, Brush>(nameof(BorderBrush), false, new(Brushes.Transparent));

        public Brush? BorderBrush { get => GetValue(BorderBrushProperty); set => SetValue(BorderBrushProperty, value); }

        public static readonly DependencyProperty<Thickness> BorderThicknessProperty = DependencyProperty.Register<Control, Thickness>(nameof(BorderThickness), false, new FrameworkMetadata(new Thickness(0)) { AffectsArrange = true });

        public Thickness BorderThickness { get => GetValue(BorderThicknessProperty); set => SetValue(BorderThicknessProperty, value); }

        public static readonly DependencyProperty<CornerRadius> CornerRadiusProperty = DependencyProperty.Register<Border, CornerRadius>(nameof(CornerRadius), false, new FrameworkMetadata(new CornerRadius(0f)) { AffectsRender = true });

        public CornerRadius CornerRadius { get => GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }

        protected override void OnRender(UICommandList commandList)
        {
            var radius = CornerRadius;

            var padding = Padding;
            var border = BorderThickness;
            var bounds = new RectangleF(0, 0, ActualWidth, ActualHeight);
            var contentBounds = InnerContentBounds;
            var contentRectSize = contentBounds.Size - border.Size;
            var contentPaddingSize = contentRectSize + padding.Size;

            if (radius != new CornerRadius(0))
            {
                //commandList.DrawRoundedRect(contentBounds.Offset, contentRectSize, radius, BorderBrush);
            }
            else
            {
                //commandList.DrawRect(contentBounds.Offset, contentBounds.Offset + contentRectSize, BorderBrush);
            }

            base.OnRender(commandList);
        }
    }
}