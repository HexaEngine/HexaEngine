namespace HexaEngine.UI.Controls
{
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Graphics.Text;
    using System.Numerics;

    public class Label : TextBaseElement
    {
        private TextLayout? textLayout;
        private string? text;

        private readonly Brush foreground;

        public Label()
        {
            foreground = UIFactory.CreateSolidColorBrush(Colors.Black);
        }

        public string? Text
        {
            get => text;
            set
            {
                text = value;

                if (value == null)
                {
                    var tmp = textLayout;
                    textLayout = null;
                    tmp?.Dispose();

                    return;
                }

                if (textLayout == null)
                {
                    textLayout = CreateTextLayout(value, float.MaxValue, float.MaxValue);
                }
                else
                {
                    textLayout.Text = value;
                }

                InvalidateArrange();
            }
        }

        public Brush Foreground => foreground;

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            if (textLayout == null)
            {
                return default;
            }

            textLayout.MaxWidth = availableSize.X;
            textLayout.MaxHeight = availableSize.Y;
            textLayout.UpdateLayout();

            return new Vector2(textLayout.Metrics.Width, textLayout.Metrics.Height);
        }

        public override void OnRender(UICommandList commandList)
        {
            if (textLayout == null)
            {
                return;
            }

            textLayout.DrawText(commandList, foreground);
        }
    }
}