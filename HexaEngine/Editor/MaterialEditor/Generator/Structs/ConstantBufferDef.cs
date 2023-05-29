namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    public struct ConstantBufferDef
    {
        public string Name;
        public SType Type;

        public ConstantBufferDef(string name, SType type)
        {
            Name = name;
            Type = type;
        }
    }
}