namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.ImGui;

    public unsafe struct ImGuiFontBuilder
    {
        private ImFontAtlasPtr fontAtlas;
        private ImFontConfigPtr config;
        private ImFontPtr font;

        public ImGuiFontBuilder(ImFontAtlasPtr fontAtlasPtr)
        {
            config = ImGui.ImFontConfig();
            config.FontDataOwnedByAtlas = false;
            fontAtlas = fontAtlasPtr;
        }

        public readonly ImFontConfigPtr Config => config;

        public readonly ImFontPtr Font => font;

        public readonly ImGuiFontBuilder SetOption(Action<ImFontConfigPtr> action)
        {
            action(config);
            return this;
        }

        public ImGuiFontBuilder AddFontFromFileTTF(string path, float size, ReadOnlySpan<uint> glyphRanges)
        {
            fixed (uint* pGlyphRanges = glyphRanges)
                return AddFontFromFileTTF(path, size, pGlyphRanges);
        }

        public ImGuiFontBuilder AddFontFromFileTTF(string path, float size, uint* glyphRanges)
        {
            font = fontAtlas.AddFontFromFileTTF(path, size, config, glyphRanges);
            config.MergeMode = true;
            return this;
        }

        public ImGuiFontBuilder AddFontFromFileTTF(string path, float size)
        {
            font = fontAtlas.AddFontFromFileTTF(path, size, config);
            config.MergeMode = true;
            return this;
        }

        public ImGuiFontBuilder AddFontFromMemoryTTF(byte* fontData, int fontDataSize, float size)
        {
            // IMPORTANT: AddFontFromMemoryTTF() by default transfer ownership of the data buffer to the font atlas, which will attempt to free it on destruction.
            // This was to avoid an unnecessary copy, and is perhaps not a good API (a future version will redesign it).
            font = fontAtlas.AddFontFromMemoryTTF(fontData, fontDataSize, size, config);

            return this;
        }

        public ImGuiFontBuilder AddFontFromMemoryTTF(ReadOnlySpan<byte> fontData, float size, ReadOnlySpan<uint> glyphRanges)
        {
            fixed (byte* pFontData = fontData)
            {
                fixed (uint* pGlyphRanges = glyphRanges)
                {
                    return AddFontFromMemoryTTF(pFontData, fontData.Length, size, pGlyphRanges);
                }
            }
        }

        public ImGuiFontBuilder AddFontFromMemoryTTF(byte* fontData, int fontDataSize, float size, ReadOnlySpan<uint> glyphRanges)
        {
            fixed (uint* pGlyphRanges = glyphRanges)
                return AddFontFromMemoryTTF(fontData, fontDataSize, size, pGlyphRanges);
        }

        public ImGuiFontBuilder AddFontFromMemoryTTF(byte* fontData, int fontDataSize, float size, uint* pGlyphRanges)
        {
            // IMPORTANT: AddFontFromMemoryTTF() by default transfer ownership of the data buffer to the font atlas, which will attempt to free it on destruction.
            // This was to avoid an unnecessary copy, and is perhaps not a good API (a future version will redesign it).
            font = fontAtlas.AddFontFromMemoryTTF(fontData, fontDataSize, size, config, pGlyphRanges);

            return this;
        }

        public void Destroy()
        {
            config.Destroy();
            config = default;
            fontAtlas = default;
        }
    }
}