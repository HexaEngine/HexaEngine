namespace HexaEngine.UI.Graphics.Text
{
    using Hexa.NET.FreeType;
    using HexaEngine.Core.Graphics;
    using System;

    public unsafe class TextFactory
    {
        private readonly FTLibrary library;
        private readonly IGraphicsDevice device;

        public TextFactory(IGraphicsDevice device)
        {
            this.device = device;
            FTError error;
            FTLibrary library;
            error = (FTError)FreeType.FTInitFreeType(&library);
            if (error != FTError.FtErrOk)
            {
                throw new($"Failed to initialize library, {error}");
            }
            this.library = library;
            FontResolver = new DefaultFontResolver(library);
        }

        public IFontResolver FontResolver { get; set; }

        public TextFormat CreateTextFormat(string fontFamily, FontStyle style, FontWeight weight, float fontSize)
        {
            var file = FontResolver.Resolve(fontFamily, style, weight) ?? throw new Exception($"Couldn't find font, ({fontFamily}, {style}, {weight})");

            return new TextFormat(device, library, file, fontSize);
        }

        public void Dispose()
        {
            library.DoneFreeType();
        }
    }
}