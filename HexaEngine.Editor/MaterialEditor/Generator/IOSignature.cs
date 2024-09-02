namespace HexaEngine.Editor.MaterialEditor.Generator
{
    using System.Collections.Generic;

    public struct IOSignature
    {
        public string Name;
        public List<SignatureDef> Defs;

        public IOSignature(string name)
        {
            Name = name;
            Defs = new();
        }

        public IOSignature(string name, params SignatureDef[] defs)
        {
            Name = name;
            Defs = new(defs);
        }
    }
}