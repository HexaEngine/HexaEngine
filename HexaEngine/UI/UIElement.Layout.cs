namespace HexaEngine.UI
{
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public partial class UIElement
    {
        private Vector2 absolutePosition;
        private Thickness bounds;

        private float actualHeight;
        private float actualWidth;

        private RectangleF outerRectangle;
        private RectangleF innerRectangle;
        private RectangleF contentRectangle;

        private Matrix3x2 positionMatrix;
        private Matrix3x2 positionInnerMatrix;
        private Matrix3x2 positionContentMatrix;
        private HorizontalAlignment horizontalAlignment;
        private VerticalAlignment verticalAlignment;
        private Thickness padding;
        private Thickness margin;
        private Thickness border;
        private float height = float.NaN;
        private float width = float.NaN;
        private float contentWidth;
        private float contentHeight;

        public Brush? Background { get; set; }

        public Brush? Border { get; set; }

        public Color BackgroundColor { get; set; }

        public Color BorderColor { get; set; }

        public HorizontalAlignment HorizontalAlignment
        {
            get => horizontalAlignment;
            set
            {
                if (horizontalAlignment == value)
                    return;
                horizontalAlignment = value;
                InvalidateLayout();
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
                InvalidateLayout();
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
                InvalidateLayout();
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
                InvalidateLayout();
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
                InvalidateLayout();
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
                InvalidateLayout();
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
                InvalidateLayout();
            }
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

        public RectangleF BoundingBox { get; protected set; }

        public virtual void Draw(UICommandList commandList)
        {
            var before = commandList.Transform;
            commandList.Transform = positionMatrix;

            commandList.FillRect(outerRectangle, BorderColor.ToVector4().Col4ToUInt());
            commandList.Transform = positionInnerMatrix;

            commandList.FillRect(innerRectangle, BackgroundColor.ToVector4().Col4ToUInt());
            commandList.Transform = positionContentMatrix;
            DrawContent(commandList);
            commandList.Transform = before;
        }

        public virtual void DrawContent(UICommandList context)
        {
            return;
        }

        public virtual void InvalidateLayout()
        {
            if (!IsInitialized) return;
            ResolveObject<UIWindow>()?.CalculateBounds();
        }

        public virtual void CalculateBounds()
        {
            UIElement? ancestor = ResolveObject<UIElement>();
            UpdateBounds(ancestor);
            GetCenter(ancestor, out Vector2 center);
            UpdateLayout(ancestor, center);

            BoundingBox = new RectangleF(absolutePosition.X, absolutePosition.Y, ActualWidth, ActualHeight);
            OnBoundsUpdated(BoundingBox);
        }

        protected virtual void OnBoundsUpdated(RectangleF boundingBox)
        {
        }

        protected virtual Thickness GetBounds()
        {
            Vector2 contentSizeHalf = GetContentSize() / 2;
            Thickness thickness = new(
                contentSizeHalf.X + Padding.Left + BorderThickness.Left,
                contentSizeHalf.Y + Padding.Top + BorderThickness.Top,
                contentSizeHalf.X + Padding.Right + BorderThickness.Right,
                contentSizeHalf.Y + Padding.Bottom + BorderThickness.Bottom);

            return thickness;
        }

        protected void UpdateBounds(UIElement? ancestor)
        {
            var bounds = GetBounds();

            Thickness absBounds = ancestor?.bounds ?? default;
            if (horizontalAlignment == HorizontalAlignment.Stretch)
            {
                bounds.Left = absBounds.Left;
                bounds.Right = absBounds.Right;
            }
            if (verticalAlignment == VerticalAlignment.Stretch)
            {
                bounds.Top = absBounds.Top;
                bounds.Bottom = absBounds.Bottom;
            }

            this.bounds = bounds;
        }

        protected void GetCenter(UIElement? ancestor, out Vector2 center)
        {
            center = Vector2.Zero;

            float actualWidth = ancestor?.contentWidth ?? 0;
            float actualHeight = ancestor?.contentHeight ?? 0;

            float actualWidthHalf = ancestor?.contentWidth / 2 ?? 0;
            float actualHeightHalf = ancestor?.contentHeight / 2 ?? 0;

            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    center.X = bounds.Left;
                    break;

                case HorizontalAlignment.Center:
                    center.X = actualWidthHalf;
                    break;

                case HorizontalAlignment.Right:
                    center.X = actualWidth + bounds.Right;
                    break;

                case HorizontalAlignment.Stretch:
                    center.X = actualWidthHalf;
                    break;
            }

            switch (verticalAlignment)
            {
                case VerticalAlignment.Top:
                    center.Y = bounds.Top;
                    break;

                case VerticalAlignment.Center:
                    center.Y = actualHeightHalf;
                    break;

                case VerticalAlignment.Bottom:
                    center.Y = actualHeight + bounds.Bottom;
                    break;

                case VerticalAlignment.Stretch:
                    center.Y = actualHeightHalf;
                    break;
            }
        }

        protected void UpdateLayout(UIElement? ancestor, Vector2 center)
        {
            Vector2 origin = ancestor?.absolutePosition ?? default;

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

            if (float.IsNaN(height))
            {
                position.Y -= bounds.Top / 2 + bounds.Bottom / 2;
                positionExtend.Y += bounds.Bottom / 2 + bounds.Top / 2;
            }
            else
            {
                position.Y -= height / 2;
                positionExtend.Y += height / 2;
            }

            if (float.IsNaN(Width))
            {
                position.X -= bounds.Left / 2 + bounds.Right / 2;
                positionExtend.X += bounds.Right / 2 + bounds.Left / 2;
            }
            else
            {
                position.X -= width / 2;
                positionExtend.X += width / 2;
            }

            actualWidth = positionExtend.X - position.X;
            actualHeight = positionExtend.Y - position.Y;

            if (HorizontalAlignment == HorizontalAlignment.Right)
            {
                position.X -= actualWidth;
                positionExtend.X -= actualWidth;
            }

            if (VerticalAlignment == VerticalAlignment.Bottom)
            {
                position.Y -= actualHeight;
                positionExtend.Y -= actualHeight;
            }

            position += center + origin;
            positionExtend += center + origin;

            actualWidth = positionExtend.X - position.X;
            actualHeight = positionExtend.Y - position.Y;

            absolutePosition = position;

            Vector2 borderPosition = position;
            borderPosition.X += border.Left;
            borderPosition.Y += border.Top;
            Vector2 borderPositionExtend = positionExtend;
            borderPositionExtend.X -= border.Right;
            borderPositionExtend.Y -= border.Bottom;
            float borderWidth = borderPositionExtend.X - borderPosition.X;
            float borderHeight = borderPositionExtend.Y - borderPosition.Y;

            Vector2 contentPosition = borderPosition;
            contentPosition.X += padding.Left;
            contentPosition.Y += padding.Top;
            Vector2 contentPositionExtend = borderPositionExtend;
            contentPositionExtend.X -= padding.Right;
            contentPositionExtend.Y -= padding.Bottom;
            contentWidth = contentPositionExtend.X - contentPosition.X;
            contentHeight = contentPositionExtend.Y - contentPosition.Y;

            outerRectangle = new RectangleF(0, 0, actualWidth, actualHeight);
            innerRectangle = new RectangleF(0, 0, borderWidth, borderHeight);
            contentRectangle = new RectangleF(0, 0, contentWidth, contentHeight);
            positionMatrix = Matrix3x2.CreateTranslation(position);
            positionInnerMatrix = Matrix3x2.CreateTranslation(borderPosition);
            positionContentMatrix = Matrix3x2.CreateTranslation(contentPosition);
        }

        public virtual Vector2 GetContentSize()
        {
            return Vector2.Zero;
        }
    }
}