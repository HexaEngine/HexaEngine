namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    /// <summary>
    /// An unique identifier to distinguish definitions from each other.
    /// </summary>
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