namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;

    public class Button : ButtonBase
    {
        private Brush? background;
        private Brush? HighlightBrush;

        public Button()
        {
            Focusable = true;
        }

        public override void InitializeComponent()
        {
            Background = UIFactory.CreateSolidColorBrush(Colors.White);
            Border = UIFactory.CreateSolidColorBrush(Colors.DarkGray);
            HighlightBrush = UIFactory.CreateSolidColorBrush(Colors.LightGray);
        }

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