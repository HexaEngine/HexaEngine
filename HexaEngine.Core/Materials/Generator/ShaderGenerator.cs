namespace HexaEngine.Materials.Generator
{
    using HexaEngine.Core.Collections;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using System.Collections.Generic;
    using System.Text;

    public class ShaderGenerator
    {
        private readonly GenerationContext context = new();
        private readonly TopologicalSorter<Node> sorter = new();

        public const string VersionString = "v1.0.0";

        public const uint Version = 1;

        public event Action<VariableTable>? OnPreBuildTable;

        public event Action<VariableTable>? OnPostBuildTable;

        public string Generate(Node root, List<Node> nodes, string entryName, bool defineInputStruct, bool defineOutputStruct, IOSignature inputSignature, IOSignature outputSignature)
        {
            context.Reset();
            var table = context.Table;
            OnPreBuildTable?.Invoke(table);

            var input = context.Input = new(inputSignature.Name);
            for (int i = 0; i < inputSignature.Defs.Count; i++)
            {
                input.Defs.Add(new(inputSignature.Defs[i].Name, inputSignature.Defs[i].Type));
            }

            if (defineInputStruct)
            {
                context.Input = input = table.AddStruct(input);
            }

            var inputVar = context.InputVar = table.AddVariable(new(-1, table.GetUniqueName(input.Name.ToLower()), new(input.Name), string.Empty, false, true));
            context.InputType = new(input.Name);

            var output = context.Output = new(outputSignature.Name);
            for (int i = 0; i < outputSignature.Defs.Count; i++)
            {
                output.Defs.Add(new(outputSignature.Defs[i].Name, outputSignature.Defs[i].Type));
            }

            if (output.Name != input.Name && defineOutputStruct)
            {
                output = table.AddStruct(output);
            }

            var outputDef = context.OutputDef = new()
            {
                Type = new(output.Name),
            };

            var order = sorter.TopologicalSort(nodes);

            foreach (Node node in order)
            {
                context.AnalyzeNode(node);
            }

            context.DynamicVarCBBuilder.Finish(false);

            OnPostBuildTable?.Invoke(table);

            StringBuilder builder = new();
            CodeWriter writer = new(builder);
            BuildHeader(writer);
            BuildBody(writer, root, entryName);

            return builder.ToString();
        }

        public IReadOnlyDictionary<ITextureNode, uint> TextureMapping => context.TextureMapping;

        public IReadOnlyDictionary<ITextureNode, uint> SamplerMapping => context.SamplerMapping;

        private void BuildHeader(CodeWriter builder)
        {
            context.BuildTable(builder);
        }

        public bool Inline { get; set; }

        private void BuildBody(CodeWriter builder, Node root, string entryName)
        {
            var table = context.Table;
            var outputDef = context.OutputDef;
            var input = context.Input;
            var inputVar = context.InputVar;

            var type = outputDef.Type;
            var signature = "";
            if (type.IsStruct)
            {
                signature = $"{outputDef.Type.Name} {entryName}({input.Name} {inputVar.Name})";
            }
            else
            {
                signature = $"{outputDef.Type.Name} {entryName}({input.Name} {inputVar.Name}) : SV_TARGET";
            }

            using (builder.PushBlock(signature))
            {
                for (int i = 0; i < table.OperationCount; i++)
                {
                    var op = table.GetOperation(i);
                    if (op.CanInline && Inline)
                    {
                        continue;
                    }

                    op.Append(builder, Inline);
                }
                WriteRootNode(root, builder);
            }
        }

        private void WriteRootNode(Node node, CodeWriter builder)
        {
            var table = context.Table;
            var outputDef = context.OutputDef;
            var inputVar = context.InputVar;
            var output = context.Output;

            if (outputDef.Type.IsStruct && inputVar.Type.IsStruct && outputDef.Type.Name == inputVar.Type.Name)
            {
                var name = inputVar.Name;
                for (int i = 0; i < output.Defs.Count; i++)
                {
                    var def = output.Defs[i];
                    var ip = context.GetVariableFirstLink(node.Pins[i]);
                    builder.WriteLine($"{name}.{def.Name} = {ip.Name};");
                }
                builder.WriteLine($"return {name};");
            }
            else if (outputDef.Type.IsStruct)
            {
                var name = table.GetUniqueName(outputDef.Type.Name.ToLower());
                builder.WriteLine($"{outputDef.Type.Name} {name};");
                for (int i = 0; i < output.Defs.Count; i++)
                {
                    var def = output.Defs[i];
                    var ip = context.GetVariableFirstLink(node.Pins[i]);
                    builder.WriteLine($"{name}.{def.Name} = {ip.Name};");
                }
                builder.WriteLine($"return {name};");
            }
        }
    }
}