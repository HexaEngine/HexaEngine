namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core.Input.Events;
    using HexaEngine.UI.Graphics;
    using System.Diagnostics.CodeAnalysis;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class Button : ButtonBase
    {
        private Brush? background;

        public Button()
        {
            Focusable = true;
        }

        public static readonly DependencyProperty<Brush> HighlightBrushProperty = DependencyProperty.Register<Button, Brush>(nameof(HighlightBrush), false, new(Brushes.LightGray));

        public Brush? HighlightBrush { get => GetValue(HighlightBrushProperty); set => SetValue(HighlightBrushProperty, value); }

        protected override void OnMouseEnter(MouseEventArgs args)
        {
            base.OnMouseEnter(args);
            background = Background;
            Background = HighlightBrush;
            InvalidateVisual();
        }

        protected override void OnMouseLeave(MouseEventArgs args)
        {
            base.OnMouseLeave(args);
            Background = background;
            InvalidateVisual();
        }
    }
}