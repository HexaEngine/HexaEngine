namespace HexaEngine.Materials.Generator
{
    public struct SignatureDef
    {
        public string Name;
        public SType Type;

        public SignatureDef(string name, SType type)
        {
            Type = type;
            Name = name;
        }
    }
}