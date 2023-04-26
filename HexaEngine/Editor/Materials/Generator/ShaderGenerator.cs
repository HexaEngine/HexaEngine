namespace HexaEngine.Editor.Materials.Generator
{
    using HexaEngine.Editor.Materials.Generator.Enums;
    using HexaEngine.Editor.Materials.Generator.Structs;
    using HexaEngine.Editor.Materials.Nodes;
    using HexaEngine.Editor.Materials.Nodes.Shaders;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public class ShaderSource
    {
    }

    public class ShaderGenerator
    {
        private readonly VariableTable table = new();
        private readonly Dictionary<Node, int> mapping = new();

        private Struct input;
        private Operation inputVar;

        private Struct output;
        private OutputDefinition outputDef;

        private struct OutputDefinition
        {
            public SType Type;
        }

        public ShaderGenerator()
        {
        }

        public string Generate(OutputNode root)
        {
            table.Clear();
            mapping.Clear();
            input = new()
            {
                Name = "PixelInput",
                Defs = new()
                {
                    new("position", new(VectorType.Float4, "SV_POSITION")),
                    new("pos", new(VectorType.Float4, "POSITION")),
                    new("tex", new(VectorType.Float2, "TEXCOORD0")),
                    new("normal", new(VectorType.Float3, "NORMAL")),
                    new("tangent", new(VectorType.Float3, "TANGENT")),
                },
            };

            input = table.AddStruct(input);
            inputVar = table.AddVariable(new(-1, table.GetUniqueName(input.Name.ToLower()), new(input.Name), string.Empty));
            SType type = new(input.Name);

            output = new()
            {
                Name = "PixelOutput",
                Defs = new()
                {
                    new("color", new(VectorType.Float4, "SV_TARGET0")),
                },
            };

            output = table.AddStruct(output);

            outputDef = new()
            {
                Type = new(output.Name),
            };

            var order = TreeTraversal(root, true);
            StringBuilder builder = new();
            for (int i = 0; i < order.Length; i++)
            {
                builder.Clear();
                var node = order[i];
                var id = mapping.Count;
                mapping.Add(node, id);
                if (node is InputNode)
                {
                    table.AddVariable(new(id, inputVar.Name, type, string.Empty));
                }
                else if (node is TextureFileNode textureFile)
                {
                    Build(textureFile, builder);
                }
                else if (node is IMathFuncNode mathFunc)
                {
                    Build(mathFunc, builder);
                }
                else if (node is IMathOpNode mathOp)
                {
                    Build(mathOp, builder);
                }
                else if (node is ConvertNode converter)
                {
                    Build(converter, builder);
                }
                else if (node is PackNode pack)
                {
                    Build(pack, builder);
                }
                else if (node is SplitNode split)
                {
                    Build(split, builder);
                }
                else if (node is MethodNode method)
                {
                    Build(method, builder);
                }
            }

            builder.Clear();
            BuildHeader(builder);
            BuildBody(builder, root);
            return builder.ToString();
        }

        public static Node[] TreeTraversal(Node root, bool includeStatic)
        {
            Stack<Node> stack1 = new();
            Stack<Node> stack2 = new();

            Node node = root;
            stack1.Push(node);
            while (stack1.Count != 0)
            {
                node = stack1.Pop();
                if (stack2.Contains(node))
                {
                    RemoveFromStack(stack2, node);
                }
                stack2.Push(node);

                for (int i = 0; i < node.Links.Count; i++)
                {
                    if (node.Links[i].InputNode == node)
                    {
                        var src = node.Links[i].OutputNode;
                        if (includeStatic && src.IsStatic || !src.IsStatic)
                        {
                            stack1.Push(node.Links[i].OutputNode);
                        }
                    }
                }
            }

            return stack2.ToArray();
        }

        public static void RemoveFromStack<T>(Stack<T> values, T value) where T : class
        {
            Stack<T> swap = new();
            while (values.Count > 0)
            {
                var val = values.Pop();
                if (val.Equals(value))
                {
                    break;
                }

                swap.Push(val);
            }
            while (swap.Count > 0)
            {
                values.Push(swap.Pop());
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
            if (pin.Links.Count == 0)
            {
                if (pin.Parent is ITypedNode node && pin is IDefaultValuePin defaultValue)
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

        private Operation AddVariable(string name, Node node, SType type, string def)
        {
            name = name.ToLower();
            string newName = table.GetUniqueName(name);
            return table.AddVariable(new(mapping[node], newName, type, def));
        }

        private Operation Find(Node node)
        {
            var id = mapping[node];
            return table.GetVariable(id);
        }

        private void BuildHeader(StringBuilder builder)
        {
            table.Build(builder);
        }

        private void BuildBody(StringBuilder builder, OutputNode root)
        {
            var type = outputDef.Type;

            if (type.IsStruct)
            {
                builder.AppendLine($"{outputDef.Type.Name} main({input.Name} {inputVar.Name})");
            }
            else
            {
                builder.AppendLine($"{outputDef.Type.Name} main({input.Name} {inputVar.Name}) : SV_TARGET");
            }

            builder.AppendLine("{");
            for (int i = 0; i < table.OperationCount; i++)
            {
                table.GetOperation(i).Append(builder);
            }
            Build(root, builder);
            builder.AppendLine("}");
        }

        private Operation Build(TextureFileNode node, StringBuilder builder)
        {
            var tex = GetVariableFirstLink(node.InUV);
            var srv = AddSrv($"Srv{node.Name}", new(TextureType.Texture2D), new(VectorType.Float4));
            var sampler = AddSampler($"Sampler{node.Name}", new(SamplerType.SamplerState));
            var output = AddVariable(node.Name, node, new(VectorType.Float4), $"{srv.Name}.Sample({sampler.Name}, {tex.Name})");
            return output;
        }

        private Operation Build(Definition left, Definition right, SType type, Node node, string op, StringBuilder builder)
        {
            if (VariableTable.NeedCastPerComponentMath(left.Type, right.Type))
            {
                var output = AddVariable(node.Name, node, type, $"{VariableTable.FromCastTo(left.Type, type)}{left.Name} {op} {VariableTable.FromCastTo(right.Type, type)}{right.Name}");
                return output;
            }
            else
            {
                var output = AddVariable(node.Name, node, type, $"{left.Name} {op} {right.Name}");
                return output;
            }
        }

        private Operation Build(IMathOpNode node, StringBuilder builder)
        {
            var left = GetVariableFirstLink(node, node.InLeft);
            var right = GetVariableFirstLink(node, node.InRight);
            return Build(left, right, node.Type, (Node)node, node.Op, builder);
        }

        private Operation Build(Definition[] definitions, SType type, Node node, string func, StringBuilder builder)
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

            return output;
        }

        private Operation Build(IMathFuncNode node, StringBuilder builder)
        {
            Definition[] definitions = new Definition[node.Params.Count];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = GetVariableFirstLink(node, node.Params[i]);
            }

            return Build(definitions, node.Type, (Node)node, node.Op, builder);
        }

        private Operation Build(ConvertNode node, StringBuilder builder)
        {
            var inVal = GetVariableFirstLink(node.In);
            var output = AddVariable(node.Name, node, new(VectorType.Float4), $"float4({inVal.Name},{node.Value.ToString(CultureInfo.InvariantCulture)})");
            return output;
        }

        private Operation Build(InputNode node, StringBuilder builder)
        {
            return default;
        }

        private Operation Build(OutputNode node, StringBuilder builder)
        {
            if (outputDef.Type.IsStruct)
            {
                var name = table.GetUniqueName(outputDef.Type.Name.ToLower());
                builder.AppendLine($"{outputDef.Type.Name} {name};");
                for (int i = 0; i < output.Defs.Count; i++)
                {
                    var def = output.Defs[i];
                    var ip = GetVariableFirstLink(node.Pins[i]);
                    builder.AppendLine($"{name}.{def.Name} = {ip.Name};");
                }
                builder.AppendLine($"return {name};");
            }

            return default;
        }

        private Operation Build(MethodNode node, StringBuilder builder)
        {
            node.DefineMethod(table);
            Definition[] definitions = new Definition[node.Params.Count];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitions[i] = GetVariableFirstLink(node, node.Params[i]);
            }

            return Build(definitions, node.Type, node, node.MethodName, builder);
        }

        private Operation Build(SplitNode node, StringBuilder builder)
        {
            var def = GetVariableFirstLink(node.In);
            table.AddVariable(new(mapping[node], def.Name, node.Type, string.Empty));
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
                    var output = AddVariable(node.Name, node, new(VectorType.Float4), $"float2({defX.Name},{defY.Name})");
                    return output;
                }
                if (type.VectorType == VectorType.Float3)
                {
                    var output = AddVariable(node.Name, node, new(VectorType.Float4), $"float3({defX.Name},{defY.Name},{defZ.Name})");
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