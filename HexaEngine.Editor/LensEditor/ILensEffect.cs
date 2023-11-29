namespace HexaEngine.Editor.LensEditor
{
    using HexaEngine.Editor.MaterialEditor.Generator;

    public interface ILensEffect
    {
        public string Name { get; }

        bool Enabled { get; set; }

        public bool Draw();

        public void Generate(CodeWriter writer, int index);
    }
}