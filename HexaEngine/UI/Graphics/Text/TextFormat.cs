namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using HexaEngine.Core.Graphics;
    using System;

    public class TextFormat : IDisposable
    {
        private readonly VectorFont font;
        private bool disposedValue;
        private float fontSize;

        public TextFormat(IGraphicsDevice device, FTLibrary library, string file, float fontSize = 1)
        {
            font = new VectorFont(device, library, file, fontSize);
            this.fontSize = fontSize;
        }

        public VectorFont Font => font;

        public float FontSize { get => fontSize; set => fontSize = value; }

        public float IncrementalTabStop { get; set; } = 48;

        public float LineSpacing { get; set; } = 0;

        public float WordSpacing { get; set; } = 0;

        public FlowDirection FlowDirection { get; set; }

        public ReadingDirection ReadingDirection { get; set; }

        public WordWrapping WordWrapping { get; set; }

        public ParagraphAlignment ParagraphAlignment { get; set; }

        public TextAlignment TextAlignment { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                font.Dispose();
                disposedValue = true;
            }
        }

        ~TextFormat()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}