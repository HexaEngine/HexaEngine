namespace HexaEngine.UI.Controls
{
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
            InvalidateLayout();
        }

        protected virtual void OnChildRemoved(object? sender, UIElement e)
        {
            InvalidateLayout();
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

        public override void DrawContent(UICommandList commandList)
        {
            Children.ForEach(child => child.Draw(commandList));
        }

        public override void CalculateBounds()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].CalculateBounds();
            }

            base.CalculateBounds();
        }

        protected override Vector2 GetPositionInElement(UIElement? child)
        {
            Vector2 origin = base.GetPositionInElement(child);

            if (child == null)
            {
                return origin;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                var child1 = Children[i];

                if (child == child1)
                    break;

                var childSize = child1.GetBounds(this).ToSize();
                if (Direction == StackPanelDirection.Vertical)
                {
                    origin.Y += childSize.Y;
                }
                else
                {
                    origin.X += childSize.X;
                }
            }

            return origin;
        }

        public override Vector2 GetAvailableContentSize(UIElement? child)
        {
            Vector2 avail = base.GetAvailableContentSize(child);

            if (child == null)
            {
                return avail;
            }

            return avail;
        }

        public override Vector2 GetContentSize(UIElement? ancestor)
        {
            Vector2 size = default;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var childSize = child.GetBounds(this).ToSize();
                if (Direction == StackPanelDirection.Vertical)
                {
                    size.Y += childSize.Y;
                    size.X = MathF.Max(size.X, childSize.X);
                }
                else
                {
                    size.X += childSize.X;
                    size.Y = MathF.Max(size.Y, childSize.Y);
                }
            }

            return size;
        }
    }
}