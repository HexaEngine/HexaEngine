namespace HexaEngine.UI.Controls
{
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public class StackPanel : UIElement, IChildContainer
    {
        public StackPanel()
        {
            Children = new(this);
        }

        public UIElementCollection Children { get; }

        public StackPanelDirection Direction { get; set; }

        public override void InitializeComponent()
        {
            Children.ForEach(child => child.InitializeComponent());
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

        internal override void Initialize()
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

        public override void OnRender(UICommandList commandList)
        {
            Children.ForEach(child => child.Draw(commandList));
        }

        protected override void ArrangeCore(RectangleF finalRect)
        {
            base.ArrangeCore(finalRect);

            RectangleF content = ContentBounds;
            Vector2 origin = content.Offset;

            float pen = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                Vector2 desiredSize = child.DesiredSize;
                Vector2 childOrigin;
                if (Direction == StackPanelDirection.Vertical)
                {
                    childOrigin = new(0, pen);
                    pen += desiredSize.Y;
                }
                else
                {
                    childOrigin = new(pen, 0);
                    pen += desiredSize.X;
                }

                RectangleF childRect = new(origin + childOrigin, desiredSize);
                child.Arrange(childRect);
            }
        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            Vector2 size = default;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.Measure(availableSize);
                Vector2 desiredSize = child.DesiredSize;

                if (Direction == StackPanelDirection.Vertical)
                {
                    size.Y += desiredSize.Y;
                    size.X = MathF.Max(size.X, desiredSize.X);
                }
                else
                {
                    size.X += desiredSize.X;
                    size.Y = MathF.Max(size.Y, desiredSize.Y);
                }
            }

            return size;
        }
    }
}