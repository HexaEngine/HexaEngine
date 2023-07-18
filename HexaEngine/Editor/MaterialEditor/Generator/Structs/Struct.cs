namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    using System.Collections.Generic;
    using System.Text;

    public struct Struct
    {
        public string Name;
        public List<StructDef> Defs = new();

        public Struct(string name) : this()
        {
            Name = name;
        }

        public readonly void Build(CodeWriter builder)
        {
            using (builder.PushBlockSemicolon($"struct {Name}"))
            {
                for (int i = 0; i < Defs.Count; i++)
                {
                    StructDef def = Defs[i];
                    if (def.Type.Semantic != null)
                    {
                        builder.WriteLine($"{def.Type.GetTypeName()} {def.Name} : {def.Type.Semantic};");
                    }
                    else
                    {
                        builder.WriteLine($"{def.Type.GetTypeName()} {def.Name};");
                    }
                }
            }
            builder.WriteLine();
        }
    }
}