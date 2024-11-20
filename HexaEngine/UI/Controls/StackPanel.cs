﻿namespace HexaEngine.UI.Controls
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public class StackPanel : Panel, IChildContainer
    {
        public StackPanelDirection Direction { get; set; }

        public override void InitializeComponent()
        {
            Children.ElementAdded += OnChildAdded;
            Children.ElementRemoved += OnChildRemoved;
        }

        protected virtual void OnChildAdded(object? sender, UIElement e)
        {
            InvalidateArrange();
        }

        protected virtual void OnChildRemoved(object? sender, UIElement e)
        {
            InvalidateArrange();
        }

        public override void Initialize()
        {
            base.Initialize();
            Children.ForEach(child => child.Initialize());
        }

        internal override void Uninitialize()
        {
            Children.ElementAdded -= OnChildAdded;
            Children.ElementRemoved -= OnChildRemoved;
            Children.ForEach(child => child.Uninitialize());
            base.Uninitialize();
        }

        protected override Vector2 ArrangeOverwrite(Vector2 size)
        {
            Vector2 origin = ContentOffset.Translation;

            float pen = 0;
            Vector2 innerSize = size;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                Vector2 desiredSize = Vector2.Min(child.DesiredSize, size);

                Vector2 childOrigin;
                if (Direction == StackPanelDirection.Vertical)
                {
                    childOrigin = new(0, pen);
                    pen += desiredSize.Y;
                    innerSize.Y -= desiredSize.Y;
                }
                else
                {
                    childOrigin = new(pen, 0);
                    pen += desiredSize.X;
                    innerSize.X -= desiredSize.X;
                }

                innerSize = Vector2.Max(innerSize, Vector2.Zero);

                RectangleF childRect = new(origin + childOrigin, desiredSize);
                child.Arrange(childRect);
            }

            return size;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            Vector2 size = default;
            Vector2 availableInnerSize = availableSize;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.Measure(availableInnerSize);
                Vector2 desiredSize = child.DesiredSize;

                if (Direction == StackPanelDirection.Vertical)
                {
                    size.Y += desiredSize.Y;
                    size.X = MathF.Max(size.X, desiredSize.X);
                    availableInnerSize.Y -= desiredSize.Y;
                }
                else
                {
                    size.X += desiredSize.X;
                    size.Y = MathF.Max(size.Y, desiredSize.Y);
                    availableInnerSize.X -= desiredSize.X;
                }

                availableInnerSize = Vector2.Max(availableInnerSize, Vector2.Zero);
            }

            return size;
        }
    }
}