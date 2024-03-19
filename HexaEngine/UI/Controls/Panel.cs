namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public class Panel : FrameworkElement
    {
        public Panel()
        {
            Children = new(this);
        }

        public UIElementCollection Children { get; }

        public Brush? Background { get; set; }

        public override sealed void Render(UICommandList commandList)
        {
            if (Visibility != Visibility.Visible)
            {
                return;
            }

            commandList.PushClipRect(new(BoundingBox));
            var before = commandList.Transform;

            if (Background != null)
            {
                commandList.Transform = Matrix3x2.Identity;
                commandList.FillRect(BoundingBox, Background);
            }

            commandList.Transform = ContentOffset;
            OnRender(commandList);
            commandList.Transform = before;
            commandList.PopClipRect();
        }
    }
}