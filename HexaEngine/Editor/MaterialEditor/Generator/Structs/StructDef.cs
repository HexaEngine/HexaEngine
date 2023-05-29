namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    using HexaEngine.Editor.MaterialEditor.Generator;

    public struct StructDef
    {
        public string Name;
        public SType Type;

        public StructDef(string name, SType type)
        {
            Name = name;
            Type = type;
        }
    }
}