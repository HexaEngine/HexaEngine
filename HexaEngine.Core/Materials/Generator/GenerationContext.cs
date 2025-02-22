namespace HexaEngine.Materials.Generator
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Generator.Structs;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;
    using System.Collections.Generic;
    using System.Text;

    public class GenerationContext
    {
        public readonly VariableTable Table = new();
        public readonly Dictionary<Node, int> Mapping = new();
        public readonly Dictionary<ITextureNode, uint> TextureMapping = new();
        public readonly Dictionary<ITextureNode, uint> SamplerMapping = new();

        public Struct Input;
        public Operation InputVar;
        public SType InputType;

        public Struct Output;
        public OutputDefinition OutputDef;

        public int Id;

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

        public GenerationContext()
        {
            InputVar = null!;
        }

        public GenerationContext(Struct input, Operation inputVar, Struct output, OutputDefinition outputDef)
        {
            Input = input;
            InputVar = inputVar;
            Output = output;
            OutputDef = outputDef;
        }

        public void Reset()
        {
            Table.Clear();
            Mapping.Clear();
            TextureMapping.Clear();
            SamplerMapping.Clear();

            for (int i = 0; i < keywords.Length; i++)
            {
                Table.AddKeyword(keywords[i]);
            }
            for (int i = 0; i < functions.Length; i++)
            {
                Table.AddKeyword(functions[i]);
            }
        }

        private readonly StringBuilder builder = new();

        public void AnalyzeNode(Node node)
        {
            if (Mapping.ContainsKey(node)) // skip.
                return;

            builder.Clear();

            var id = Mapping.Count;
            Mapping.Add(node, id);
            Id = id;

            for (int j = 0; j < NodeAnalyzerRegistry.Analyzers.Count; j++)
            {
                if (NodeAnalyzerRegistry.Analyzers[j].TryAnalyze(node, this, builder))
                {
                    break;
                }
            }
        }

        #region Lookup Helpers

        public Operation Find(Node node)
        {
            if (!Mapping.ContainsKey(node))
            {
                AnalyzeNode(node);
            }

            var id = Mapping[node];
            var op = Table.GetVariable(id);

            if (op == null)
            {
                AnalyzeNode(node);
                return Find(node);
            }
            return op;
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

        public Definition GetVariableFirstLink(ITypedNode node, PrimitivePin pin)
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

        #endregion Lookup Helpers

        public ShaderResourceView AddSrv(ITextureNode node, string name, SType srvType, SType type)
        {
            var srv = Table.AddShaderResourceView(new(Table.GetUniqueName(name), srvType, type));
            TextureMapping.Add(node, srv.Slot);
            return srv;
        }

        public SamplerState AddSampler(ITextureNode node, string name, SType samplerType)
        {
            var sampler = Table.AddSamplerState(new(Table.GetUniqueName(name), samplerType));
            SamplerMapping.Add(node, sampler.Slot);
            return sampler;
        }

        public Operation AddVariable(string name, Node node, SType type, string def, bool allowInline = true)
        {
            name = name.ToLower().Replace(" ", string.Empty);
            string newName = Table.GetUniqueName(name);
            return Table.AddVariable(new(Mapping[node], newName, type, def, allowInline, true));
        }

        public Operation AddVariable(Node node, string def, bool allowInline = true)
        {
            return Table.AddVariable(new(Mapping[node], string.Empty, default, def, allowInline, false));
        }

        public Operation AddVariable(Operation operation)
        {
            return Table.AddVariable(operation);
        }

        public void BuildTable(CodeWriter builder)
        {
            Table.Build(builder);
        }

        public void AddRef(string name, Operation refTo)
        {
            Table.AddRef(name, refTo);
        }

        public Operation BuildFunctionCallVoid(Definition[] definitions, Node node, string func, StringBuilder builder, bool isMathFunc = true)
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
                {
                    AddRef(def.Name, output);
                }
            }

            return output;
        }

        public Operation BuildFunctionCall(Definition[] definitions, SType returnType, Node node, string func, StringBuilder builder, bool isMathFunc = true)
        {
            if (returnType.IsVoid)
            {
                return BuildFunctionCallVoid(definitions, node, func, builder, false);
            }

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
            var output = AddVariable(node.Name, node, returnType, builder.ToString());
            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];
                if (isMathFunc)
                {
                    AddRef(def.Name, output);
                }
            }

            return output;
        }

        public Operation BuildOperatorCall(Definition left, Definition right, SType type, Node node, string op, StringBuilder builder)
        {
            if (VariableTable.NeedCastPerComponentMath(left.Type, right.Type))
            {
                var output = AddVariable(node.Name, node, type, $"{VariableTable.FromCastTo(left.Type, type)}{left.Name} {op} {VariableTable.FromCastTo(right.Type, type)}{right.Name}");
                AddRef(left.Name, output);
                AddRef(right.Name, output);
                return output;
            }
            else
            {
                var output = AddVariable(node.Name, node, type, $"{left.Name} {op} {right.Name}");
                AddRef(left.Name, output);
                AddRef(right.Name, output);
                return output;
            }
        }
    }
}