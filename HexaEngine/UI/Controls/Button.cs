namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Graphics.Text;
    using System.Numerics;

    public class Button : TextBaseElement
    {
        private TextLayout? textLayout;
        private string content = string.Empty;

        private Brush foreground;
        private Brush background;
        private Brush HighlightBrush;

        public override void InitializeComponent()
        {
            foreground = UIFactory.CreateSolidColorBrush(Colors.Black);
            Background = UIFactory.CreateSolidColorBrush(Colors.White);
            Border = UIFactory.CreateSolidColorBrush(Colors.DarkGray);
            HighlightBrush = UIFactory.CreateSolidColorBrush(Colors.LightGray);
        }

        public string Content
        {
            get => content; set
            {
                if (content == value)
                {
                    return;
                }

                content = value;

                InvalidateArrange();
                textLayout ??= CreateTextLayout(content, float.MaxValue, float.MaxValue);
            }
        }

        public override void OnRender(UICommandList commandList)
        {
            if (textLayout == null)
            {
                return;
            }

            textLayout.DrawText(commandList, foreground);
        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            if (textLayout == null)
            {
                return Vector2.Zero;
            }

            textLayout.MaxWidth = availableSize.X;
            textLayout.MaxHeight = availableSize.Y;
            textLayout.UpdateLayout();

            return new Vector2(textLayout.Metrics.Width, textLayout.Metrics.Height);
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