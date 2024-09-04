namespace HexaEngine.Editor.MaterialEditor.Generator
{
    using HexaEngine.Editor.MaterialEditor.Generator.Analyzers;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using System.Collections.Generic;
    using System.Text;

    public class ShaderGenerator
    {
        #region Instricts and Keywords.

        private readonly string[] functions =
        {
            "abs",
            "acos",
            "all",
            "any",
            "asdouble",
            "asfloat",
            "asin",
            "asint",
            "asuint",
            "atan",
            "atan2",
            "ceil",
            "clamp",
            "clip",
            "cos",
            "cosh",
            "cross",
            "ddx",
            "ddx_coarse",
            "ddx_fine",
            "ddy",
            "ddy_coarse",
            "ddy_fine",
            "degrees",
            "distance",
            "dot",
            "distance",
            "exp",
            "exp2",
            "floor",
            "fmod",
            "frac",
            "fwidth",
            "ldexp",
            "length",
            "lerp",
            "lit",
            "log",
            "log10",
            "log2",
            "max",
            "min",
            "-",
            "normalize",
            "pow",
            "radians",
            "reflect",
            "refract",
            "round",
            "rpc",
            "rsqrt",
            "saturate",
            "sin",
            "sinh",
            "smoothstep",
            "sqrt",
            "step",
            "tan",
            "tanh"
        };

        private readonly string[] keywords =
        {
            "AppendStructuredBuffer",
            "asm",
            "asm_fragment",
            "BlendState",
            "bool",
            "break",
            "Buffer",
            "ByteAddressBuffer",
            "case",
            "cbuffer",
            "centroid",
            "class",
            "column_major",
            "compile",
            "compile_fragment",
            "CompileShader",
            "const",
            "continue",
            "ComputeShader",
            "ConsumeStructuredBuffer",
            "default",
            "DepthStencilState",
            "DepthStencilView",
            "discard",
            "do",
            "double",
            "DomainShader",
            "dword",
            "else",
            "export",
            "extern",
            "false",
            "float",
            "for",
            "fxgroup",
            "GeometryShader",
            "groupshared",
            "half",
            "Hullshader",
            "if",
            "in",
            "inline",
            "inout",
            "InputPatch",
            "int",
            "interface",
            "line",
            "lineadj",
            "linear",
            "LineStream",
            "matrix",
            "min16float",
            "min10float",
            "min16int",
            "min12int",
            "min16uint",
            "namespace",
            "nointerpolation",
            "noperspective",
            "NULL",
            "out",
            "OutputPatch",
            "packoffset",
            "pass",
            "pixelfragment",
            "PixelShader",
            "point",
            "PointStream",
            "precise",
            "RasterizerState",
            "RenderTargetView",
            "return",
            "register",
            "row_major",
            "RWBuffer",
            "RWByteAddressBuffer",
            "RWStructuredBuffer",
            "RWTexture1D",
            "RWTexture1DArray",
            "RWTexture2D",
            "RWTexture2DArray",
            "RWTexture3D",
            "sample",
            "linearSampler",
            "SamplerState",
            "SamplerComparisonState",
            "shared",
            "snorm",
            "stateblock",
            "stateblock_state",
            "static",
            "string",
            "struct",
            "switch",
            "StructuredBuffer",
            "tbuffer",
            "technique",
            "technique10",
            "technique11",
            "texture",
            "Texture1D",
            "Texture1DArray",
            "Texture2D",
            "Texture2DArray",
            "Texture2DMS",
            "Texture2DMSArray",
            "Texture3D",
            "TextureCube",
            "TextureCubeArray",
            "true",
            "typedef",
            "triangle",
            "triangleadj",
            "TriangleStream",
            "uint",
            "uniform",
            "unorm",
            "unsigned",
            "vector",
            "vertexfragment",
            "VertexShader",
            "void",
            "volatile",
            "while",
            "float1x1",
            "float1x2",
            "float1x3",
            "float1x4",
            "float2",
            "float2x1",
            "float2x2",
            "float2x3",
            "float2x4",
            "float3",
            "float3x1",
            "float3x2",
            "float3x3",
            "float3x4",
            "float4",
            "float4x1",
            "float4x2",
            "float4x3",
            "float4x4",
            "int1x1",
            "int1x2",
            "int1x3",
            "int1x4",
            "int2",
            "int2x1",
            "int2x2",
            "int2x3",
            "int2x4",
            "int3",
            "int3x1",
            "int3x2",
            "int3x3",
            "int3x4",
            "int4",
            "int4x1",
            "int4x2",
            "int4x3",
            "int4x4",
            "uint1x1",
            "uint1x2",
            "uint1x3",
            "uint1x4",
            "uint2",
            "uint2x1",
            "uint2x2",
            "uint2x3",
            "uint2x4",
            "uint3",
            "uint3x1",
            "uint3x2",
            "uint3x3",
            "uint3x4",
            "uint4",
            "uint4x1",
            "uint4x2",
            "uint4x3",
            "uint4x4",
            "bool1x1",
            "bool1x2",
            "bool1x3",
            "bool1x4",
            "bool2",
            "bool2x1",
            "bool2x2",
            "bool2x3",
            "bool2x4",
            "bool3",
            "bool3x1",
            "bool3x2",
            "bool3x3",
            "bool3x4",
            "bool4",
            "bool4x1",
            "bool4x2",
            "bool4x3",
            "bool4x4",
            "min10float1x1",
            "min10float1x2",
            "min10float1x3",
            "min10float1x4",
            "min10float2",
            "min10float2x1",
            "min10float2x2",
            "min10float2x3",
            "min10float2x4",
            "min10float3",
            "min10float3x1",
            "min10float3x2",
            "min10float3x3",
            "min10float3x4",
            "min10float4",
            "min10float4x1",
            "min10float4x2",
            "min10float4x3",
            "min10float4x4",
            "min16float1x1",
            "min16float1x2",
            "min16float1x3",
            "min16float1x4",
            "min16float2",
            "min16float2x1",
            "min16float2x2",
            "min16float2x3",
            "min16float2x4",
            "min16float3",
            "min16float3x1",
            "min16float3x2",
            "min16float3x3",
            "min16float3x4",
            "min16float4",
            "min16float4x1",
            "min16float4x2",
            "min16float4x3",
            "min16float4x4",
            "min12int1x1",
            "min12int1x2",
            "min12int1x3",
            "min12int1x4",
            "min12int2",
            "min12int2x1",
            "min12int2x2",
            "min12int2x3",
            "min12int2x4",
            "min12int3",
            "min12int3x1",
            "min12int3x2",
            "min12int3x3",
            "min12int3x4",
            "min12int4",
            "min12int4x1",
            "min12int4x2",
            "min12int4x3",
            "min12int4x4",
            "min16int1x1",
            "min16int1x2",
            "min16int1x3",
            "min16int1x4",
            "min16int2",
            "min16int2x1",
            "min16int2x2",
            "min16int2x3",
            "min16int2x4",
            "min16int3",
            "min16int3x1",
            "min16int3x2",
            "min16int3x3",
            "min16int3x4",
            "min16int4",
            "min16int4x1",
            "min16int4x2",
            "min16int4x3",
            "min16int4x4",
            "min16uint1x1",
            "min16uint1x2",
            "min16uint1x3",
            "min16uint1x4",
            "min16uint2",
            "min16uint2x1",
            "min16uint2x2",
            "min16uint2x3",
            "min16uint2x4",
            "min16uint3",
            "min16uint3x1",
            "min16uint3x2",
            "min16uint3x3",
            "min16uint3x4",
            "min16uint4",
            "min16uint4x1",
            "min16uint4x2",
            "min16uint4x3",
            "min16uint4x4",
            "#define",
            "#elif",
            "#else",
            "#endif",
            "#error",
            "#if",
            "#ifdef",
            "#ifndef",
            "#include",
            "#line",
            "#pragma",
            "#undef",
            "auto",
            "case",
            "catch",
            "char",
            "class",
            "const_cast",
            "default",
            "delete",
            "dynamic_cast",
            "enum",
            "explicit",
            "friend",
            "goto",
            "long",
            "mutable",
            "new",
            "operator",
            "private",
            "protected",
            "public",
            "reinterpret_cast",
            "short",
            "signed",
            "sizeof",
            "static_cast",
            "template",
            "this",
            "throw",
            "try",
            "typename",
            "union",
            "unsigned",
            "using",
            "virtual",
        };

        #endregion Instricts and Keywords.

        private readonly GenerationContext context = new();
        private readonly List<INodeAnalyzer> analyzers = [];

        public ShaderGenerator()
        {
            analyzers.Add(new InputNodeAnalyzer());
            analyzers.Add(new ComponentMaskNodeAnalyzer());
            analyzers.Add(new ConstantNodeAnalyzer());
            analyzers.Add(new ConvertNodeAnalyzer());
            analyzers.Add(new FuncCallDeclarationNodeAnalyzer());
            analyzers.Add(new FuncCallNodeAnalyzer());
            analyzers.Add(new FuncCallVoidNodeAnalyzer());
            analyzers.Add(new FuncOperatorNodeAnalyzer());
            analyzers.Add(new PackNodeAnalyzer());
            analyzers.Add(new SplitNodeAnalyzer());
            analyzers.Add(new TextureNodeAnalyzer());
        }

        public const string VersionString = "v1.0.0";

        public const uint Version = 1;

        public event Action<VariableTable>? OnPreBuildTable;

        public event Action<VariableTable>? OnPostBuildTable;

        public string Generate(Node root, List<Node> nodes, string entryName, bool defineInputStruct, bool defineOutputStruct, IOSignature inputSignature, IOSignature outputSignature)
        {
            context.Reset();
            var table = context.Table;
            OnPreBuildTable?.Invoke(table);
            for (int i = 0; i < keywords.Length; i++)
            {
                table.AddKeyword(keywords[i]);
            }
            for (int i = 0; i < functions.Length; i++)
            {
                table.AddKeyword(functions[i]);
            }

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

            var order = TopologicalSort(nodes);
            StringBuilder builder = new();
            for (int i = 0; i < order.Count; i++)
            {
                builder.Clear();
                var node = order[i];
                var id = context.Mapping.Count;
                context.Mapping.Add(node, id);
                context.Id = id;

                for (int j = 0; j < analyzers.Count; j++)
                {
                    if (analyzers[j].TryAnalyze(node, context, builder))
                    {
                        break;
                    }
                }
            }

            OnPostBuildTable?.Invoke(table);

            builder.Clear();
            CodeWriter writer = new(builder);
            BuildHeader(writer);
            BuildBody(writer, root, entryName);

            return builder.ToString();
        }

        public IReadOnlyDictionary<ITextureNode, uint> TextureMapping => context.TextureMapping;

        public IReadOnlyDictionary<ITextureNode, uint> SamplerMapping => context.SamplerMapping;

        public static List<Node> TopologicalSort(IList<Node> nodes)
        {
            List<Node> sorted = new();
            HashSet<Node> visited = new();
            for (int i = 0; i < nodes.Count; i++)
            {
                Visit(nodes[i], sorted, visited);
            }
            return sorted;
        }

        public static void Visit(Node node, List<Node> sorted, HashSet<Node> visited)
        {
            bool alreadyVisited = visited.Contains(node);
            if (!alreadyVisited)
            {
                visited.Add(node);
                var dependencies = node.Links;
                for (int i = 0; i < dependencies.Count; i++)
                {
                    var dependency = dependencies[i];
                    if (dependency.InputNode != node)
                    {
                        continue;
                    }

                    Visit(dependency.OutputNode, sorted, visited);
                }

                if (node.Links.Count > 0)
                {
                    sorted.Add(node);
                }
            }
        }

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