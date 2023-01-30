namespace HexaEngine.Editor.Materials
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
                if (def.Semantic != null)
                    builder.AppendLine($"{VariableTable.GetTypeName(def.Type)} {def.Name} : {def.Semantic};");
                else
                    builder.AppendLine($"{VariableTable.GetTypeName(def.Type)} {def.Name};");
            }
            builder.AppendLine("};");
        }
    }
}