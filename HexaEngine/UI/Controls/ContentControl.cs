namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics;

    public class ContentControl : Control
    {
        private object? content;

        public object? Content
        {
            get => content;
            set => content = value;
        }

        public override void OnRender(UICommandList context)
        {
            if (content is not Visual visual)
            {
                return;
            }
        }
    }
}