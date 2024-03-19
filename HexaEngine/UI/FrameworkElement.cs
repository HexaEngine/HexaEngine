namespace HexaEngine.UI
{
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public class FrameworkElement : UIElement
    {
        private Matrix3x2 baseOffset;
        private Matrix3x2 contentOffset;

        public static readonly DependencyProperty<float> ActualWidthProperty = DependencyProperty.Register<FrameworkElement, float>(nameof(ActualWidth), false, new FrameworkMetadata(0f));

        public static readonly DependencyProperty<float> ActualHeightProperty = DependencyProperty.Register<FrameworkElement, float>(nameof(ActualHeight), false, new FrameworkMetadata(0f));

        public float ActualWidth
        {
            get => GetValue(ActualWidthProperty);
            private set => SetValue(ActualWidthProperty, value);
        }

        public float ActualHeight
        {
            get => GetValue(ActualHeightProperty);
            private set => SetValue(ActualHeightProperty, value);
        }

        public Matrix3x2 BaseOffset => baseOffset;

        public Matrix3x2 ContentOffset => contentOffset;

        public override void Render(UICommandList commandList)
        {
            if (Visibility != Visibility.Visible)
            {
                return;
            }

            OnRender(commandList);
        }

        protected override sealed Vector2 MeasureCore(Vector2 availableSize)
        {
            // retrieve all props at once at the start to reduce overhead.
            var width = Width;
            var height = Height;
            var minWidth = MinWidth;
            var minHeight = MinHeight;
            var maxWidth = MaxWidth;
            var maxHeight = MaxHeight;

            var horizontalAlign = HorizontalAlignment;
            var verticalAlign = VerticalAlignment;
            var marginSize = Margin.ToSize();
            var paddingSize = Padding.ToSize();

            if (!float.IsNaN(width))
            {
                availableSize.X = width;
            }

            if (!float.IsNaN(height))
            {
                availableSize.Y = height;
            }

            if (minWidth != 0)
            {
                availableSize.X = Math.Max(availableSize.X, minWidth);
            }

            if (minHeight != 0)
            {
                availableSize.Y = Math.Max(availableSize.Y, minHeight);
            }

            if (maxWidth != 0)
            {
                availableSize.X = Math.Min(availableSize.X, maxWidth);
            }

            if (maxHeight != 0)
            {
                availableSize.Y = Math.Min(availableSize.Y, maxHeight);
            }

            Vector2 actualAvailableSize = availableSize;

            actualAvailableSize -= marginSize;
            actualAvailableSize -= paddingSize;

            Vector2 desiredSize = MeasureOverwrite(actualAvailableSize);

            desiredSize += paddingSize;

            if (horizontalAlign == HorizontalAlignment.Stretch)
            {
                desiredSize.X = availableSize.X;
            }

            if (verticalAlign == VerticalAlignment.Stretch)
            {
                desiredSize.Y = availableSize.Y;
            }

            if (!float.IsNaN(width))
            {
                desiredSize.X = width;
            }

            if (!float.IsNaN(height))
            {
                desiredSize.Y = height;
            }

            if (minWidth != 0)
            {
                availableSize.X = Math.Max(availableSize.X, minWidth);
            }

            if (minHeight != 0)
            {
                availableSize.Y = Math.Max(availableSize.Y, minHeight);
            }

            if (maxWidth != 0)
            {
                availableSize.X = Math.Min(availableSize.X, maxWidth);
            }

            if (maxHeight != 0)
            {
                availableSize.Y = Math.Min(availableSize.Y, maxHeight);
            }

            return desiredSize;
        }

        protected virtual Vector2 MeasureOverwrite(Vector2 availableSize)
        {
            return default;
        }

        protected override sealed void ArrangeCore(RectangleF finalRect)
        {
            Vector2 origin = new(finalRect.Left, finalRect.Top);
            Vector2 rectSize = finalRect.Size;

            // retrieve all props at once at the start to reduce overhead.
            var desiredSize = DesiredSize;
            var horizontalAlign = HorizontalAlignment;
            var verticalAlign = VerticalAlignment;
            var margin = Margin;
            var padding = Padding;

            switch (horizontalAlign)
            {
                case HorizontalAlignment.Left:
                    origin.X = finalRect.Left;
                    break;

                case HorizontalAlignment.Right:
                    origin.X = finalRect.Right - desiredSize.X;
                    break;

                case HorizontalAlignment.Center:
                    origin.X = finalRect.Left + (rectSize.X - desiredSize.X) / 2;
                    break;
            }

            switch (verticalAlign)
            {
                case VerticalAlignment.Top:
                    origin.Y = finalRect.Top;
                    break;

                case VerticalAlignment.Bottom:
                    origin.Y = finalRect.Bottom - desiredSize.Y;
                    break;

                case VerticalAlignment.Center:
                    origin.Y += (rectSize.Y - desiredSize.Y) / 2;
                    break;
            }

            Vector2 position = Vector2.Zero;
            Vector2 positionExtend = Vector2.Zero;

            position.X += margin.Left;
            position.Y += margin.Top;

            // do not compensate size loss in stretch mode, this would cause overlap.
            if (horizontalAlign != HorizontalAlignment.Stretch)
            {
                position.X -= margin.Right;
                positionExtend.X += margin.Left;
            }

            positionExtend.X -= margin.Right;
            positionExtend.Y -= margin.Bottom;

            // do not compensate size loss in stretch mode, this would cause overlap.
            if (verticalAlign != VerticalAlignment.Stretch)
            {
                positionExtend.Y += margin.Top;
                position.Y -= margin.Bottom;
            }

            position += origin;
            positionExtend += origin + desiredSize;

            Vector2 size = new(positionExtend.X - position.X, positionExtend.Y - position.Y);
            size = ArrangeOverwrite(size);

            positionExtend = position + size;

            Vector2 contentPosition = position;
            contentPosition.X += padding.Left;
            contentPosition.Y += padding.Top;
            Vector2 contentPositionExtend = positionExtend;
            contentPositionExtend.X -= padding.Right;
            contentPositionExtend.Y -= padding.Bottom;
            Vector2 contentSize = contentPositionExtend - contentPosition;

            ActualWidth = size.X;
            ActualHeight = size.Y;
            RenderSize = size;
            BoundingBox = VisualClip = new RectangleF(position, size);
            InnerContentBounds = new RectangleF(contentPosition, contentSize);
            baseOffset = Matrix3x2.CreateTranslation(position);
            contentOffset = Matrix3x2.CreateTranslation(contentPosition);
        }

        protected virtual Vector2 ArrangeOverwrite(Vector2 size)
        {
            return size;
        }
    }
}