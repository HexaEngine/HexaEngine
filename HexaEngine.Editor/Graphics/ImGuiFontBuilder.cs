namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.IO;

    public unsafe struct ImGuiFontBuilder
    {
        private ImFontAtlasPtr fontAtlas;
        private ImFontConfigPtr config;
        private ImFontPtr font;

        public ImGuiFontBuilder(ImFontAtlasPtr fontAtlasPtr)
        {
            config = ImGui.ImFontConfig();
            config.FontDataOwnedByAtlas = true;
            fontAtlas = fontAtlasPtr;
        }

        public readonly ImFontConfigPtr Config => config;

        public readonly ImFontPtr Font => font;

        public readonly ImGuiFontBuilder SetOption(Action<ImFontConfigPtr> action)
        {
            action(config);
            return this;
        }

        public ImGuiFontBuilder AddFontFromAsset(AssetPath path, float size)
        {
            var mem = FileSystem.ReadAllBytes(path);
            var pMem = mem.ToPtr();
            font = fontAtlas.AddFontFromMemoryTTF(pMem, mem.Length, size, config); 
            config.MergeMode = true;
            return this;
        }

        public ImGuiFontBuilder AddFontFromAsset(AssetPath path, float size, uint* pGlyphRanges)
        {
            var mem = FileSystem.ReadAllBytes(path);
            var pMem = mem.ToPtr();
            font = fontAtlas.AddFontFromMemoryTTF(pMem, mem.Length, size, config, pGlyphRanges);
            config.MergeMode = true;
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