namespace HexaEngine.Editor.MaterialEditor.Generator
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Generator.Enums;
    using HexaEngine.Editor.MaterialEditor.Generator.Structs;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

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

    public class ShaderGenerator
    {
        private readonly VariableTable table = new();
        private readonly Dictionary<Node, int> mapping = new();
        private readonly Dictionary<ITextureNode, uint> textureMapping = new();
        private readonly Dictionary<ITextureNode, uint> samplerMapping = new();

        private Struct input;
        private Operation inputVar;

        private Struct output;
        private OutputDefinition outputDef;

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
            "sampler",
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

        private struct OutputDefinition
        {
            public SType Type;
        }

        public ShaderGenerator()
        {
        }

        public const string VersionString = "v1.0.0";

        public const uint Version = 1;

        public event Action<VariableTable>? OnPreBuildTable;

        public event Action<VariableTable>? OnPostBuildTable;

        public string Generate(Node root, List<Node> nodes, string entryName, bool defineInputStruct, bool defineOutputStruct, IOSignature inputSignature, IOSignature outputSignature)
        {
            table.Clear();
            OnPreBuildTable?.Invoke(table);
            for (int i = 0; i < keywords.Length; i++)
            {
                table.AddKeyword(keywords[i]);
            }
            for (int i = 0; i < functions.Length; i++)
            {
                table.AddKeyword(functions[i]);
            }
            mapping.Clear();
            textureMapping.Clear();

            input = new(inputSignature.Name);
            for (int i = 0; i < inputSignature.Defs.Count; i++)
            {
                input.Defs.Add(new(inputSignature.Defs[i].Name, inputSignature.Defs[i].Type));
            }

            if (defineInputStruct)
            {
                input = table.AddStruct(input);
            }

            inputVar = table.AddVariable(new(-1, table.GetUniqueName(input.Name.ToLower()), new(input.Name), string.Empty, false, true));
            SType type = new(input.Name);

            output = new(outputSignature.Name);
            for (int i = 0; i < outputSignature.Defs.Count; i++)
            {
                output.Defs.Add(new(outputSignature.Defs[i].Name, outputSignature.Defs[i].Type));
            }

            if (output.Name != input.Name && defineOutputStruct)
            {
                output = table.AddStruct(output);
            }

            outputDef = new()
            {
                Type = new(output.Name),
            };

            var order = TopologicalSort(nodes);
            StringBuilder builder = new();
            for (int i = 0; i < order.Count; i++)
            {
                builder.Clear();
                var node = order[i];
                var id = mapping.Count;
                mapping.Add(node, id);
                if (node is InputNode)
                {
                    table.AddVariable(new(id, inputVar.Name, type, string.Empty, false, true));
                }
                else if (node is ITextureNode texture)
                {
                    Build(texture, builder);
                }
                else if (node is IFuncCallVoidNode funcCallVoid)
                {
                    Build(funcCallVoid, builder);
                }
                else if (node is IFuncCallNode funcCall)
                {
                    Build(funcCall, builder);
                }
                else if (node is IFuncOperatorNode funcOperator)
                {
                    Build(funcOperator, builder);
                }
                else if (node is IFuncCallDeclarationNode method)
                {
                    Build(method, builder);
                }
                else if (node is ConvertNode converter)
                {
                    Build(converter, builder);
                }
                else if (node is ComponentMaskNode compose)
                {
                    Build(compose, builder);
                }
                else if (node is PackNode pack)
                {
                    Build(pack, builder);
                }
                else if (node is SplitNode split)
                {
                    Build(split, builder);
                }
                else if (node is ConstantNode constant)
                {
                    Build(constant, builder);
                }
            }

            OnPostBuildTable?.Invoke(table);

            builder.Clear();
            CodeWriter writer = new(builder);
            BuildHeader(writer);
            BuildBody(writer, root, entryName);

            return builder.ToString();
        }

        public IReadOnlyDictionary<ITextureNode, uint> TextureMapping => textureMapping;

        public IReadOnlyDictionary<ITextureNode, uint> SamplerMapping => samplerMapping;

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
                    sorted.Add(node);
            }
        }

        public void Analyse(Node[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
            }
        }

        public Definition GetVariable(Pin target, Node? other)
        {
            if (other == null)
            {
                throw new NullReferenceException();
            }

            var op = Find(other);
            if (!op.Type.IsStruct)
            {
                return new(op.Name, op.Type);
            }
            else
            {
                var link = Node.FindSourceLink(target, other);
                if (link == null)
                {
                    throw new NullReferenceException();
                }

                return new($"{op.Type.Name}.{link.Output.Name}", op.Type);
            }
        }

        public bool TryGetVariable(Pin target, Node? other, out Definition variable)
        {
            if (other == null)
            {
                variable = default;
                return false;
            }

            var op = Find(other);
            if (!op.Type.IsStruct)
            {
                variable = new(op.Name, op.Type);
                return true;
            }
            else
            {
                var link = Node.FindSourceLink(target, other);
                if (link == null)
                {
                    variable = default;
                    return false;
                }

                variable = new($"{op.Type.Name}.{link.Output.Name}", op.Type);
                return true;
            }
        }

        public Definition GetVariable(Link? link)
        {
            if (link == null)
            {
                throw new NullReferenceException();
            }

            var op = Find(link.OutputNode);
            if (!op.Type.IsStruct)
            {
                return new(op.Name, op.Type);
            }
            else
            {
                return new($"{op.Type.Name}.{link.Output.Name}", op.Type);
            }
        }

        public bool TryGetVariable(Link? link, out Definition variable)
        {
            if (link == null)
            {
                variable = default;
                return false;
            }

            var op = Find(link.OutputNode);
            if (!op.Type.IsStruct)
            {
                variable = new(op.Name, op.Type);
                return true;
            }
            else
            {
                variable = new($"{op.Type.Name}.{link.Output.Name}", op.Type);
                return true;
            }
        }

        public Definition GetVariableLink(Pin pin, int index)
        {
            if (pin.Links.Count <= index)
            {
                return new Definition("0", new(ScalarType.Unknown));
            }

            var link = pin.Links[index];

            var op = Find(link.OutputNode);
            if (!op.Type.IsStruct)
            {
                return new(op.Name, op.Type);
            }
            else
            {
                return new($"{op.Name}.{link.Output.Name}", op.Type);
            }
        }

        public Definition GetVariableFirstLink(Pin pin)
        {
            if (pin.Links.Count == 0 || pin.Kind == PinKind.Output)
            {
                if (pin.Parent is ITypedNode node && pin is IDefaultValuePin defaultValue)
                {
                    return new Definition(defaultValue.GetDefaultValue(), node.Type);
                }
                else if (pin is ITypedPin typedPin && pin is IDefaultValuePin defaultValue1)
                {
                    return new Definition(defaultValue1.GetDefaultValue(), typedPin.Type);
                }
                else if (pin is IDefaultValuePin defaultValue2)
                {
                    return new Definition(defaultValue2.GetDefaultValue(), new(ScalarType.Unknown));
                }
                else
                {
                    return new Definition("0", new(ScalarType.Unknown));
                }
            }

            var link = pin.Links[0];

            var op = Find(link.OutputNode);
            if (!op.Type.IsStruct)
            {
                return new(op.Name, op.Type);
            }
            else
            {
                return new($"{op.Name}.{link.Output.Name}", op.Type);
            }
        }

        public Definition GetVariableFirstLink(ITypedNode node, FloatPin pin)
        {
            if (pin.Links.Count == 0)
            {
                if (pin is IDefaultValuePin defaultValue)
                {
                    return new Definition(defaultValue.GetDefaultValue(), node.Type);
                }
                else
                {
                    return new Definition("0", new(ScalarType.Unknown));
                }
            }

            var link = pin.Links[0];

            var op = Find(link.OutputNode);

            if (!op.Type.IsStruct)
            {
                return new(op.Name, op.Type);
            }
            else
            {
                return new($"{op.Name}.{link.Output.Name}", op.Type);
            }
        }

        public bool TryGetVariableFirstLink(Pin pin, out Definition variable)
        {
            if (pin.Links.Count == 0)
            {
                variable = default;
                return false;
            }

            var link = pin.Links[0];

            var op = Find(link.OutputNode);
            if (!op.Type.IsStruct)
            {
                variable = new(op.Name, op.Type);
                return true;
            }
            else
            {
                variable = new($"{op.Type.Name}.{link.Output.Name}", op.Type);
                return true;
            }
        }

        private ShaderResourceView AddSrv(string name, SType srvType, SType type)
        {
            return table.AddShaderResourceView(new(table.GetUniqueName(name), srvType, type));
        }

        private SamplerState AddSampler(string name, SType samplerType)
        {
            return table.AddSamplerState(new(table.GetUniqueName(name), samplerType));
        }

        private Operation AddVariable(string name, Node node, SType type, string def, bool allowInline = true)
        {
            name = name.ToLower().Replace(" ", string.Empty);
            string newName = table.GetUniqueName(name);
            return table.AddVariable(new(mapping[node], newName, type, def, allowInline, true));
        }

        private Operation AddVariable(Node node, string def, bool allowInline = true)
        {
            return table.AddVariable(new(mapping[node], string.Empty, default, def, allowInline, false));
        }

        private Operation Find(Node node)
        {
            var id = mapping[node];
            return table.GetVariable(id);
        }

        private void BuildHeader(CodeWriter builder)
        {
            table.Build(builder);
        }

        public bool Inline { get; set; }

        private void BuildBody(CodeWriter builder, Node root, string entryName)
        {
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
                        continue;
                    op.Append(builder, Inline);
                }
                Build(root, builder);
            }
        }

        private Operation Build(ITextureNode node, StringBuilder builder)
        {
            var tex = GetVariableFirstLink(node.InUV);
            var srv = AddSrv($"Srv{node.Name}", new(TextureType.Texture2D), new(VectorType.Float4));
            textureMapping.Add(node, srv.Slot);
            var sampler = AddSampler($"Sampler{node.Name}", new(SamplerType.SamplerState));
            samplerMapping.Add(node, sampler.Slot);
            var output = AddVariable(node.Name, (Node)node, new(VectorType.Float4), $"{srv.Name}.Sample({sampler.Name}, {tex.Name})");
            return output;
        }

        private Operation Build(Definition left, Definition right, SType type, Node node, string op, StringBuilder builder)
        {
            if (VariableTable.NeedCastPerComponentMath(left.Type, right.Type))
            {
                var output = AddVariable(node.Name, node, type, $"{VariableTable.FromCastTo(left.Type, type)}{left.Name} {op} {VariableTable.FromCastTo(right.Type, type)}{right.Name}");
                table.AddRef(left.Name, output);
                table.AddRef(right.Name, output);
                return output;
            }
            else
            {
                var output = AddVariable(node.Name, node, type, $"{left.Name} {op} {right.Name}");
                table.AddRef(left.Name, output);
                table.AddRef(right.Name, output);
                return output;
            }
        }

        private Operation Build(IFuncOperatorNode node, StringBuilder builder)
        {
            var left = GetVariableFirstLink(node, node.InLeft);
            var right = GetVariableFirstLink(node, node.InRight);
            return Build(left, right, node.Type, (Node)node, node.Op, builder);
        }

        private Operation Build(Definition[] definitions, SType type, Node node, string func, StringBuilder builder, bool isMathFunc = true)
        {
            builder.Append($"{func}(");
            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                builder.Append(definition.Name);
                if (i + 1 < definitions.Length)
                {
                    builder.Append(',');
                }
            }
            builder.Append(')');
            var output = AddVariable(node.Name, node, type, builder.ToString());
            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];
                if (isMathFunc)
                    table.AddRef(def.Name, output);
            }

            return output;
        }

        private Operation Build(Definition[] definitions, Node node, string func, StringBuilder builder, bool isMathFunc = true)
        {
            builder.Append($"{func}(");
            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                builder.Append(definition.Name);
                if (i + 1 < definitions.Length)
                {
                    builder.Append(',');
                }
            }
            builder.Append(')');
            var output = AddVariable(node, builder.ToString());
            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];
                if (isMathFunc)
                    table.AddRef(def.Name, output);
            }

            return output;
        }

        private Operation Build(IFuncCallNode node, StringBuilder builder)
        {
            Definition[] definitions = new Definition[node.Params.Count];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = GetVariableFirstLink(node, node.Params[i]);
            }

            return Build(definitions, node.Type, (Node)node, node.Op, builder);
        }

        private Operation Build(IFuncCallVoidNode node, StringBuilder builder)
        {
            Definition[] definitions = new Definition[node.Params.Count];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = GetVariableFirstLink(node, node.Params[i]);
            }

            return Build(definitions, (Node)node, node.Op, builder);
        }

        private Operation Build(ConvertNode node, StringBuilder builder)
        {
            var inVal = GetVariableFirstLink(node.In);
            var output = AddVariable(node.Name, node, new(VectorType.Float4), $"float4({inVal.Name},{node.Value.ToString(CultureInfo.InvariantCulture)})");
            table.AddRef(inVal.Name, output);
            return output;
        }

        private Operation Build(ComponentMaskNode node, StringBuilder builder)
        {
            var def = GetVariableFirstLink(node.In);

            var type = node.Type;
            if (type.IsScalar)
            {
                var output = AddVariable(node.Name, node, new(ScalarType.Float), $"{def.Name}.{node.Mask}");
                table.AddRef(def.Name, output);
                return output;
            }
            if (type.IsVector)
            {
                if (type.VectorType == VectorType.Float2)
                {
                    var output = AddVariable(node.Name, node, new(VectorType.Float2), $"{def.Name}.{node.Mask}");
                    table.AddRef(def.Name, output);
                    return output;
                }
                if (type.VectorType == VectorType.Float3)
                {
                    var output = AddVariable(node.Name, node, new(VectorType.Float3), $"{def.Name}.{node.Mask}");
                    table.AddRef(def.Name, output);
                    return output;
                }
                if (type.VectorType == VectorType.Float4)
                {
                    var output = AddVariable(node.Name, node, new(VectorType.Float4), $"{def.Name}.{node.Mask}");
                    table.AddRef(def.Name, output);
                    return output;
                }
            }
            return default;
        }

        private Operation Build(Node node, CodeWriter builder)
        {
            if (outputDef.Type.IsStruct && inputVar.Type.IsStruct && outputDef.Type.Name == inputVar.Type.Name)
            {
                var name = inputVar.Name;
                for (int i = 0; i < output.Defs.Count; i++)
                {
                    var def = output.Defs[i];
                    var ip = GetVariableFirstLink(node.Pins[i]);
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
                    var ip = GetVariableFirstLink(node.Pins[i]);
                    builder.WriteLine($"{name}.{def.Name} = {ip.Name};");
                }
                builder.WriteLine($"return {name};");
            }

            return default;
        }

        private Operation Build(IFuncCallDeclarationNode node, StringBuilder builder)
        {
            node.DefineMethod(table);
            Definition[] definitions = new Definition[node.Params.Count];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = GetVariableFirstLink(node, node.Params[i]);
            }

            return Build(definitions, node.Type, (Node)node, node.MethodName, builder, false);
        }

        private Operation Build(ConstantNode node, StringBuilder builder)
        {
            var def = GetVariableFirstLink(node.Out);
            var output = AddVariable(node.Name, node, node.Type, def.Name);
            return output;
        }

        private Operation Build(SplitNode node, StringBuilder builder)
        {
            var def = GetVariableFirstLink(node.In);
            table.AddVariable(new(mapping[node], def.Name, node.Type, string.Empty, false, true));
            return default;
        }

        private Operation Build(PackNode node, StringBuilder builder)
        {
            var type = node.Type;
            if (type.IsScalar)
            {
                var def = GetVariableFirstLink(node.Pins[0]);
                var output = AddVariable(node.Name, node, new(ScalarType.Float), $"{def.Name}");
                return output;
            }
            if (type.IsVector)
            {
                var defX = GetVariableFirstLink(node.InPins[0]);
                var defY = GetVariableFirstLink(node.InPins[1]);
                var defZ = GetVariableFirstLink(node.InPins[2]);
                var defW = GetVariableFirstLink(node.InPins[3]);
                if (type.VectorType == VectorType.Float2)
                {
                    var output = AddVariable(node.Name, node, new(VectorType.Float2), $"float2({defX.Name},{defY.Name})");
                    return output;
                }
                if (type.VectorType == VectorType.Float3)
                {
                    var output = AddVariable(node.Name, node, new(VectorType.Float3), $"float3({defX.Name},{defY.Name},{defZ.Name})");
                    return output;
                }
                if (type.VectorType == VectorType.Float4)
                {
                    var output = AddVariable(node.Name, node, new(VectorType.Float4), $"float4({defX.Name},{defY.Name},{defZ.Name},{defW.Name})");
                    return output;
                }
            }
            return default;
        }
    }
}