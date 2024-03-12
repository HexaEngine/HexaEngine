namespace HexaEngine.UI.Controls
{
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

        public Button()
        {
            BackgroundColor = Colors.Black;
            BorderColor = Colors.DarkGray;
        }

        public override void InitializeComponent()
        {
            BackgroundColor = Colors.Black;
            BorderColor = Colors.DarkGray;
            //foreground = DeviceManager.CreateSolidColorBrush(new((Vector4)Colors.Black));
            //background = DeviceManager.CreateSolidColorBrush(new((Vector4)Colors.White));
            //Border = DeviceManager.CreateSolidColorBrush(new((Vector4)Colors.DarkGray));
            //HighlightBrush = DeviceManager.CreateSolidColorBrush(new((Vector4)Colors.LightGray));
            CreateTextFormat();
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

                InvalidateLayout();
                textLayout ??= CreateTextLayout(content, float.MaxValue, float.MaxValue);
            }
        }

        public override Vector2 GetContentSize(UIElement? ancestor)
        {
            if (textLayout == null)
            {
                return default;
            }

            var avail = NormalizeAvailSize(ancestor.GetAvailableContentSize(this));

            textLayout.MaxWidth = avail.X;
            textLayout.MaxHeight = avail.Y;

            return new Vector2(textLayout?.Metrics.Width ?? 0, textLayout?.Metrics.Height ?? 0);
        }

        protected override void OnBoundsUpdated(RectangleF boundingBox)
        {
            var size = boundingBox.Size;
            if (textLayout == null)
            {
                return;
            }
            if (textLayout.MaxWidth != size.X || textLayout.MaxHeight != size.Y)
            {
                textLayout.MaxWidth = size.X;
                textLayout.MaxHeight = size.Y;
                InvalidateLayout();
            }
        }

        public override void DrawContent(UICommandList commandList)
        {
            if (textLayout == null)
            {
                return;
            }

            textLayout.DrawText(commandList);

            //commandList.DrawRect(textLayout.Metrics.Origin, textLayout.Metrics.Size, new Vector4(1, 0, 0, 1).Col4ToUInt(), 1);
        }
    }
}