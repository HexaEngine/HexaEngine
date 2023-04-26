namespace HexaEngine.Editor.Materials.Generator.Structs
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

        public void Build(StringBuilder builder)
        {
            builder.AppendLine($"struct {Name}");
            builder.AppendLine("{");
            for (int i = 0; i < Defs.Count; i++)
            {
                StructDef def = Defs[i];
                if (def.Type.Semantic != null)
                {
                    builder.AppendLine($"{def.Type.GetTypeName()} {def.Name} : {def.Type.Semantic};");
                }
                else
                {
                    builder.AppendLine($"{def.Type.GetTypeName()} {def.Name};");
                }
            }
            builder.AppendLine("};");
        }
    }
}