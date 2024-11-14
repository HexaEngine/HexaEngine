namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using Hexa.NET.Logging;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using System;

    public unsafe class TextFactory : IDisposable
    {
        private readonly List<IUIResource> resources = [];

        private bool disposedValue;
        private readonly FTLibrary library;
        private readonly IGraphicsDevice device;

        private readonly Dictionary<FontIdentifier, IFont> fontCache = [];

        private int refCounter = 1;

        public TextFactory(IGraphicsDevice device)
        {
            this.device = device;
            FTError error;
            FTLibrary library;
            error = (FTError)FreeType.InitFreeType(&library);
            if (error != FTError.Ok)
            {
                throw new($"Failed to initialize library, {error}");
            }
            this.library = library;
            FontResolver = new DefaultFontResolver(library);
        }

        public IFontResolver FontResolver { get; set; }

        public FontType FontType { get; set; } = FontType.Vector;

        public TextFormat CreateTextFormat(string fontFamily, FontStyle style, FontWeight weight, float fontSize)
        {
            TextFormat format;
            FontIdentifier fontIdentifier = new(fontFamily, style, weight);

            IFont? font;

            lock (fontCache)
            {
                if (fontCache.TryGetValue(fontIdentifier, out font) && !font.IsDisposed)
                {
                    font.AddRef();
                    format = new TextFormat(font, fontSize);
                    AddResource(format);
                    return format;
                }

                if (font != null && font.IsDisposed)
                {
                    fontCache.Remove(fontIdentifier);
                    RemoveResource(font);
                }

                string file = FontResolver.Resolve(fontFamily, style, weight) ?? throw new Exception($"Couldn't find font, ({fontFamily}, {style}, {weight})");

                font = FontType switch
                {
                    FontType.Sprite => new SpriteFont(device, library, file, fontSize),
                    FontType.Vector => new VectorFont(device, library, file, fontSize),
                    _ => throw new NotSupportedException("Font Type \"{FontType}\" is not supported"),
                };

                AddResource(font);

                fontCache.Add(fontIdentifier, font);
            }

            format = new TextFormat(font, fontSize);
            AddResource(format);
            return format;
        }

        private void AddResource(IUIResource resource)
        {
            lock (resources)
            {
                resources.Add(resource);
            }
        }

        private void RemoveResource(IUIResource resource)
        {
            lock (resources)
            {
                resources.Remove(resource);
            }
        }

        public TextLayout CreateTextLayout(string text, TextFormat format, float maxWidth, float maxHeight)
        {
            TextLayout textLayout = new(text, format, maxWidth, maxHeight);
            AddResource(textLayout);
            return textLayout;
        }

        public void AddRef()
        {
            Interlocked.Increment(ref refCounter);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (Interlocked.Decrement(ref refCounter) > 0)
                {
                    if (!disposing)
                    {
                        LoggerFactory.GetLogger(nameof(LeakTracer)).Warn($"Live Instance: {nameof(TextFactory)}, outstanding references {refCounter}");
                    }
                    return;
                }

                if (disposing)
                {
                    fontCache.Clear();
                }

                FTError error = (FTError)library.DoneFreeType();
                if (error != FTError.Ok)
                {
                    throw new Exception($"Failed to free \"FreeType Library\"");
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}