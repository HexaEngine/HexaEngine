namespace HexaEngine.Editor.Materials.Generator.Structs
{
    public struct Definition
    {
        /// <summary>
        /// The name of the definition
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of the definition.
        /// </summary>
        public SType Type;

        /// <summary>
        /// The count of operations referencing this definition.<br/>
        /// If the count is equals to 1 the definition will be inlined.<br/>
        /// If the count is equals to 0 the definition is discarded and an generator warning is printed.<br/>
        /// </summary>
        public int References;

        public Definition(string name, SType type) : this()
        {
            Name = name;
            Type = type;
        }
    }
}