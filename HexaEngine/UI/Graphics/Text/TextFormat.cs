namespace HexaEngine.UI.Graphics.Text
{
    public class TextFormat : UIResource
    {
        private readonly IFont font;
        private float fontSize;

        public TextFormat(IFont font, float fontSize = 1)
        {
            this.font = font;
            this.fontSize = fontSize;
        }

        public IFont Font => font;

        public float FontSize { get => fontSize; set => fontSize = value; }

        public float IncrementalTabStop { get; set; } = 48;

        public float LineSpacing { get; set; } = 0;

        public float WordSpacing { get; set; } = 0;

        public FlowDirection FlowDirection { get; set; }

        public ReadingDirection ReadingDirection { get; set; }

        public WordWrapping WordWrapping { get; set; }

        public ParagraphAlignment ParagraphAlignment { get; set; }

        public TextAlignment TextAlignment { get; set; }

        protected override void DisposeCore()
        {
            font.Dispose();
        }
    }
}