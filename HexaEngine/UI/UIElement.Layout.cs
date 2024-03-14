namespace HexaEngine.UI
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public partial class UIElement
    {
        private float actualHeight;
        private float actualWidth;

        private HorizontalAlignment horizontalAlignment;
        private VerticalAlignment verticalAlignment;
        private Thickness padding;
        private Thickness margin;
        private Thickness border;
        private float height = float.NaN;
        private float width = float.NaN;
        private float contentWidth;
        private float contentHeight;
        private Vector2 desiredSize;

        public HorizontalAlignment HorizontalAlignment
        {
            get => horizontalAlignment;
            set
            {
                if (horizontalAlignment == value)
                    return;
                horizontalAlignment = value;
                InvalidateArrange();
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get => verticalAlignment;
            set
            {
                if (verticalAlignment == value)
                    return;
                verticalAlignment = value;
                InvalidateArrange();
            }
        }

        public Thickness Padding
        {
            get => padding;
            set
            {
                if (padding == value)
                    return;
                padding = value;
                InvalidateArrange();
            }
        }

        public Thickness Margin
        {
            get => margin;
            set
            {
                if (margin == value)
                    return;
                margin = value;
                InvalidateArrange();
            }
        }

        public Thickness BorderThickness
        {
            get => border;
            set
            {
                if (border == value)
                    return;
                border = value;
                InvalidateArrange();
            }
        }

        public float Height
        {
            get => height;
            set
            {
                if (height == value)
                {
                    return;
                }
                height = value;
                InvalidateArrange();
            }
        }

        public float Width
        {
            get => width;
            set
            {
                if (width == value)
                {
                    return;
                }
                width = value;
                InvalidateArrange();
            }
        }

        public float MinWidth
        {
            get;
            set;
        }

        public float MinHeight
        {
            get;
            set;
        }

        public float MaxWidth
        {
            get;
            set;
        }

        public float MaxHeight
        {
            get;
            set;
        }

        public float ActualWidth
        {
            get => actualWidth;
            protected set => actualWidth = value;
        }

        public float ActualHeight
        {
            get => actualHeight;
            protected set => actualHeight = value;
        }

        public float ContentWidth => contentWidth;

        public float ContentHeight => contentHeight;

        public RectangleF ContentBounds => contentRectangle;

        public Vector2 DesiredSize => desiredSize;

        public virtual void InvalidateArrange()
        {
            if (!IsInitialized)
            {
                return;
            }

            ResolveObject<UIWindow>()?.InvalidateArrange();
        }

        public virtual void InvalidateVisual()
        {
            if (!IsInitialized)
            {
                return;
            }

            ResolveObject<UIWindow>()?.InvalidateVisual();
        }

        protected virtual Vector2 MeasureCore(Vector2 availableSize)
        {
            return default;
        }

        public void Measure(Vector2 availableSize)
        {
            if (visibility == Visibility.Collapsed)
            {
                desiredSize = default;
                return;
            }

            if (!float.IsNaN(width))
            {
                availableSize.X = width;
            }

            if (!float.IsNaN(height))
            {
                availableSize.Y = height;
            }

            if (MinWidth != 0)
            {
                availableSize.X = Math.Max(availableSize.X, MinWidth);
            }

            if (MaxWidth != 0)
            {
                availableSize.X = Math.Min(availableSize.X, MaxWidth);
            }

            if (MinHeight != 0)
            {
                availableSize.Y = Math.Max(availableSize.Y, MinHeight);
            }

            if (MaxHeight != 0)
            {
                availableSize.Y = Math.Min(availableSize.Y, MaxHeight);
            }

            Vector2 actualAvailableSize = availableSize;

            actualAvailableSize.X -= Margin.Left + Margin.Right;
            actualAvailableSize.Y -= Margin.Top + Margin.Bottom;
            actualAvailableSize.X -= BorderThickness.Left + BorderThickness.Right;
            actualAvailableSize.Y -= BorderThickness.Top + BorderThickness.Bottom;
            actualAvailableSize.X -= Padding.Left + Padding.Right;
            actualAvailableSize.Y -= Padding.Top + Padding.Bottom;

            desiredSize = MeasureCore(actualAvailableSize);

            desiredSize.X += BorderThickness.Left + BorderThickness.Right;
            desiredSize.Y += BorderThickness.Top + BorderThickness.Bottom;
            desiredSize.X += Padding.Left + Padding.Right;
            desiredSize.Y += Padding.Top + Padding.Bottom;

            if (horizontalAlignment == HorizontalAlignment.Stretch)
            {
                desiredSize.X = availableSize.X;
            }

            if (verticalAlignment == VerticalAlignment.Stretch)
            {
                desiredSize.Y = availableSize.Y;
            }
        }

        public void Arrange(RectangleF finalRect)
        {
            if (visibility == Visibility.Collapsed)
            {
                return;
            }

            ArrangeCore(finalRect);
        }

        protected virtual void ArrangeCore(RectangleF finalRect)
        {
            Vector2 origin = new(finalRect.Left, finalRect.Top);
            Vector2 rectSize = finalRect.Size;

            if (horizontalAlignment == HorizontalAlignment.Left)
            {
                origin.X = finalRect.Left;
            }

            if (horizontalAlignment == HorizontalAlignment.Right)
            {
                origin.X = finalRect.Right - DesiredSize.X;
            }
            if (horizontalAlignment == HorizontalAlignment.Center)
            {
                origin.X = finalRect.Left + (rectSize.X - DesiredSize.X) / 2;
            }

            if (verticalAlignment == VerticalAlignment.Top)
            {
                origin.Y = finalRect.Top;
            }

            if (verticalAlignment == VerticalAlignment.Bottom)
            {
                origin.Y = finalRect.Bottom - DesiredSize.Y;
            }

            if (verticalAlignment == VerticalAlignment.Center)
            {
                origin.Y += (rectSize.Y - DesiredSize.Y) / 2;
            }

            Vector2 position = Vector2.Zero;
            Vector2 positionExtend = Vector2.Zero;

            position.X += margin.Left;
            position.Y += margin.Top;

            // do not compensate size loss in stretch mode, this would cause overlap.
            if (horizontalAlignment != HorizontalAlignment.Stretch)
            {
                position.X -= margin.Right;
                positionExtend.X += margin.Left;
            }

            positionExtend.X -= margin.Right;
            positionExtend.Y -= margin.Bottom;

            // do not compensate size loss in stretch mode, this would cause overlap.
            if (verticalAlignment != VerticalAlignment.Stretch)
            {
                positionExtend.Y += margin.Top;
                position.Y -= margin.Bottom;
            }

            if (float.IsNaN(Width))
            {
                actualWidth = positionExtend.X;
            }
            else
            {
                positionExtend.X = width;
                actualWidth = width;
            }

            if (float.IsNaN(height))
            {
                actualHeight = positionExtend.Y;
            }
            else
            {
                positionExtend.Y = height;
                actualHeight = height;
            }

            position += origin;
            positionExtend += origin + DesiredSize;

            actualWidth = positionExtend.X - position.X;
            actualHeight = positionExtend.Y - position.Y;

            Vector2 contentPosition = position;
            contentPosition.X += padding.Left + border.Left;
            contentPosition.Y += padding.Top + border.Top;
            Vector2 contentPositionExtend = positionExtend;
            contentPositionExtend.X -= padding.Right + border.Right;
            contentPositionExtend.Y -= padding.Bottom + border.Bottom;
            contentWidth = contentPositionExtend.X - contentPosition.X;
            contentHeight = contentPositionExtend.Y - contentPosition.Y;

            outerRectangle = new RectangleF(0, 0, actualWidth, actualHeight);
            contentRectangle = new RectangleF(contentPosition.X, contentPosition.Y, contentWidth, contentHeight);
            positionMatrix = Matrix3x2.CreateTranslation(position);
            positionContentMatrix = Matrix3x2.CreateTranslation(contentPosition);
        }
    }
}