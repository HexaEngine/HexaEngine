namespace HexaEngine.UI
{
    using Hexa.NET.Mathematics;
    using HexaEngine.UI.Graphics;
    using System;
    using System.Numerics;

    public class Visual : DependencyObject
    {
        private Type? type;

        public Visual? VisualParent { get; private set; }

        public VisualCollection VisualChildren { get; } = [];

        public override Type DependencyObjectType => type ??= GetType();

        protected internal Matrix3x2 BaseOffset { get; protected set; }

        protected internal Matrix3x2 ContentOffset { get; protected set; }

        protected internal Vector2 VisualOffset { get; protected set; }

        protected internal RectangleF BoundingBox { get; protected set; }

        protected internal RectangleF InnerContentBounds { get; protected set; }

        protected internal RectangleF VisualClip { get; protected set; }

        public Vector2 PointFromScreen(Vector2 pointOnScreen)
        {
            Matrix3x2.Invert(BaseOffset, out var result);
            return Vector2.Transform(pointOnScreen, result);
        }

        public Vector2 PointToScreen(Vector2 pointInElement)
        {
            return Vector2.Transform(pointInElement, BaseOffset);
        }

        public void AddVisualChild(Visual visual)
        {
            VisualChildren.Add(visual);
            visual.Parent = this;
        }

        public void RemoveVisualChild(Visual visual)
        {
            visual.Parent = null;
            VisualChildren.Remove(visual);
        }

        public virtual void Render(UICommandList commandList)
        {
        }

        protected virtual void OnRender(UICommandList commandList)
        {
            return;
        }

        public bool IsDescendantOf(DependencyObject element)
        {
            var current = Parent;
            while (current != null)
            {
                if (current == element)
                {
                    return true;
                }

                current = current.Parent;
            }
            return false;
        }

        public bool IsAncestorOf(DependencyObject element)
        {
            var current = element.Parent;
            while (current != null)
            {
                if (current == this)
                {
                    return true;
                }

                current = current.Parent;
            }
            return false;
        }

        public DependencyObject? FindCommonVisualAncestor(DependencyObject element)
        {
            DependencyObject? currentA = this;
            DependencyObject? currentB = element;

            int depthA = GetDepth(currentA);
            int depthB = GetDepth(currentB);

            while (depthB > depthA)
            {
                currentB = currentB?.Parent;
                depthB--;
            }

            while (currentA != null && currentB != null)
            {
                if (currentA == currentB)
                {
                    return currentA;
                }

                currentA = currentA.Parent;
                currentB = currentB.Parent;
            }

            return null;
        }

        private static int GetDepth(DependencyObject element)
        {
            int depth = 0;
            DependencyObject? current = element;
            while (current != null)
            {
                depth++;
                current = current.Parent;
            }
            return depth;
        }
    }
}