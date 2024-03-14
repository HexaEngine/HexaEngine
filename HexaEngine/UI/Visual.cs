namespace HexaEngine.UI
{
    using HexaEngine.UI.Graphics;

    public class Visual : DependencyElement
    {
        public Visual? VisualParent { get; private set; }

        public VisualCollection VisualChildren { get; } = [];

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

        public virtual void OnRender(UICommandList context)
        {
            return;
        }
    }
}