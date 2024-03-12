namespace HexaEngine.UI
{
    using HexaEngine.UI.Graphics.Text;
    using System.Collections.Generic;

    public class TextBaseElement : UIElement
    {
        private readonly List<TextLayout> layouts = new();
        private FontStyle fontStyle = FontStyle.Regular;
        private FontWeight fontWeight = FontWeight.Normal;
        private float incrementalTabStop = 48;
        private FlowDirection flowDirection = FlowDirection.TopToBottom;
        private ReadingDirection readingDirection = ReadingDirection.LeftToRight;
        private WordWrapping wordWrapping = WordWrapping.NoWrap;
        private ParagraphAlignment paragraphAlignment = ParagraphAlignment.Near;
        private string fontFamilyName = "Arial";
        private TextAlignment textAlignment = TextAlignment.Leading;
        private float fontSize = 17;
        private TextFactory? textFactory;

        public TextFormat? Format { get; set; }

        public string FontFamilyName
        { get => fontFamilyName; set { fontFamilyName = value; CreateTextFormat(); } }

        public FontStyle FontStyle
        { get => fontStyle; set { fontStyle = value; CreateTextFormat(); } }

        public FontWeight FontWeight
        { get => fontWeight; set { fontWeight = value; CreateTextFormat(); } }

        public float IncrementalTabStop
        { get => incrementalTabStop; set { incrementalTabStop = value; UpdateTextFormat(); } }

        public FlowDirection FlowDirection
        { get => flowDirection; set { flowDirection = value; UpdateTextFormat(); } }

        public ReadingDirection ReadingDirection
        { get => readingDirection; set { readingDirection = value; UpdateTextFormat(); } }

        public WordWrapping WordWrapping
        { get => wordWrapping; set { wordWrapping = value; UpdateTextFormat(); } }

        public ParagraphAlignment ParagraphAlignment
        { get => paragraphAlignment; set { paragraphAlignment = value; UpdateTextFormat(); } }

        public TextAlignment TextAlignment
        { get => textAlignment; set { textAlignment = value; UpdateTextFormat(); } }

        public float FontSize
        { get => fontSize; set { fontSize = value; UpdateTextFormat(); } }

        internal void CreateTextFormat()
        {
            DestroyTextFormat();
            textFactory ??= UISystem.Current?.TextFactory;
            if (textFactory == null)
            {
                return;
            }
            Format = textFactory.CreateTextFormat(FontFamilyName, FontStyle, FontWeight, FontSize);
            Format.FontSize = FontSize;
            Format.IncrementalTabStop = IncrementalTabStop;
            Format.FlowDirection = FlowDirection;
            Format.ReadingDirection = ReadingDirection;
            Format.WordWrapping = WordWrapping;
            Format.ParagraphAlignment = ParagraphAlignment;
            Format.TextAlignment = TextAlignment;
            OnCreateTextFormat();
        }

        internal void DestroyTextFormat()
        {
            Format?.Dispose();
            OnDestroyTextFormat();
        }

        protected virtual void OnDestroyTextFormat()
        {
            layouts.ForEach(layout => layout.Dispose());
            layouts.Clear();
        }

        protected virtual void OnCreateTextFormat()
        {
        }

        protected TextLayout? CreateTextLayout(string text, float maxWidth, float maxHeight)
        {
            if (Format is null)
            {
                CreateTextFormat();
            }
            if (Format is null)
            {
                return null;
            }

            var layout = new TextLayout(text, Format, maxWidth, maxHeight);
            layouts.Add(layout);
            return layout;
        }

        private void UpdateTextFormat()
        {
            if (Format == null)
            {
                return;
            }
            Format.FontSize = FontSize;
            Format.IncrementalTabStop = IncrementalTabStop;
            Format.FlowDirection = FlowDirection;
            Format.ReadingDirection = ReadingDirection;
            Format.WordWrapping = WordWrapping;
            Format.ParagraphAlignment = ParagraphAlignment;
            Format.TextAlignment = TextAlignment;
        }
    }
}