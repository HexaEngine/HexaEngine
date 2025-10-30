namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Markup;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [ContentProperty("Children")]
    public class Panel : FrameworkElement, IAddChild
    {
        public Panel()
        {
            Children = new(this);
        }

        public UIElementCollection Children { get; }

        public static readonly DependencyProperty<Brush> BackgroundProperty = DependencyProperty.Register<Panel, Brush>(nameof(Background), false, new(Brushes.Transparent));

        public Brush? Background { get => GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }

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

        protected override void OnRender(UICommandList commandList)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Render(commandList);
            }
        }

        void IAddChild.AddChild(object value)
        {
            Children.Add((UIElement)value);
        }

        void IAddChild.AddText(string text)
        {
            throw new NotSupportedException();
        }
    }
}