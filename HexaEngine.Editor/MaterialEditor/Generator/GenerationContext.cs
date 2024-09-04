namespace HexaEngine.Editor.MaterialEditor.Generator
{
    using HexaEngine.Editor.MaterialEditor.Generator.Enums;
    using HexaEngine.Editor.MaterialEditor.Generator.Structs;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using Silk.NET.OpenAL;
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

        public GenerationContext()
        {
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
        }

        #region Lookup Helpers

        public Operation Find(Node node)
        {
            var id = Mapping[node];
            return Table.GetVariable(id);
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

        public Operation BuildFunctionCall(Definition[] definitions, Node node, string func, StringBuilder builder, bool isMathFunc = true)
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

        public Operation BuildFunctionCall(Definition[] definitions, SType type, Node node, string func, StringBuilder builder, bool isMathFunc = true)
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