namespace HexaEngine.Editor.Materials
{
    public readonly struct Identifier
    {
        public readonly string Name;

        public Identifier(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}