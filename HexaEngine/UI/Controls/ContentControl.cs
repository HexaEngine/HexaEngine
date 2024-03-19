namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Graphics.Text;
    using System.Numerics;

    public class ContentControl : Control
    {
        private object? content;
        private TextLayout? textLayout;
        private string? contentString;

        public object? Content
        {
            get => content;
            set
            {
                if (textLayout != null)
                {
                    textLayout?.Dispose();
                    textLayout = null;
                }

                if (content is UIElement oldElement)
                {
                    RemoveVisualChild(oldElement);
                }

                if (value is UIElement newElement)
                {
                    AddVisualChild(newElement);
                    newElement.Initialize();
                }

                if (value != null)
                {
                    contentString = value.ToString();

                    if (textLayout == null)
                    {
                        textLayout = CreateTextLayout(contentString, float.MaxValue, float.MaxValue);
                    }
                    else
                    {
                        textLayout.Text = contentString;
                    }
                }
                else
                {
                    textLayout?.Dispose();
                    textLayout = null;
                }

                content = value;
                InvalidateArrange();
            }
        }

        public string? ContentString => contentString;

        protected override Vector2 MeasureOverwrite(Vector2 availableSize)
        {
            var borderSize = BorderThickness.ToSize();

            availableSize -= borderSize;

            if (content is UIElement element)
            {
                element.Measure(availableSize);
                return element.DesiredSize + borderSize;
            }

            if (textLayout != null)
            {
                textLayout.MaxWidth = availableSize.X;
                textLayout.MaxHeight = availableSize.Y;
                textLayout.UpdateLayout();

                return new Vector2(textLayout.Metrics.Width, textLayout.Metrics.Height) + borderSize;
            }

            return borderSize;
        }

        protected override Vector2 ArrangeOverwrite(Vector2 size)
        {
            if (content is UIElement element)
            {
                element.Arrange(new(BaseOffset.Translation, size));
            }

            return size;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (content is UIElement element)
            {
                element.Initialize();
            }
        }

        protected override void OnRender(UICommandList commandList)
        {
            if (content is Visual visual)
            {
                visual.Render(commandList);
                return;
            }

            textLayout?.DrawText(commandList, Foreground);
        }
    }
}