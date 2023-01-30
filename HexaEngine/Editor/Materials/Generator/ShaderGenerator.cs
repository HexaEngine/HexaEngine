namespace HexaEngine.Editor.Materials.Generator
{
    using HexaEngine.Editor.Materials.Generator.Enums;
    using HexaEngine.Editor.Materials.Generator.Structs;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Nodes;
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
        private OutputDefinition output;

        private struct OutputDefinition
        {
            public string Type;
        }

        public ShaderGenerator()
        {
        }

        public string Generate(Node root)
        {
            table.Clear();
            mapping.Clear();
            input = new()
            {
                Name = "PixelInput",
                Defs = new()
                {
                    new("pos", new(VectorType.Float4, "SV_POSITION")),
                    new("tex", new(VectorType.Float2, "TEXCOORD0")),
                    new("normal", new(VectorType.Float3, "NORMAL")),
                    new("tangent", new(VectorType.Float3, "TANGENT")),
                },
            };
            input = table.AddStruct(input);
            inputVar = table.AddVariable(new(-1, table.GetUniqueName(input.Name.ToLower()), new(input.Name)));

            output = new()
            {
                Type = "float4",
            };

            var order = TreeTraversal(root, true);
            StringBuilder builder = new();
            for (int i = 0; i < order.Length; i++)
            {
                var node = order[i];
                if (node is TextureFileNode textureFile)
                {
                    Build(textureFile, builder);
                }
                else if (node is FloatConstantNode floatConstant)
                {
                    Build(floatConstant, builder);
                }
                else if (node is Vector2ConstantNode vector2Constant)
                {
                    Build(vector2Constant, builder);
                }
                else if (node is Vector3ConstantNode vector3Constant)
                {
                    Build(vector3Constant, builder);
                }
                else if (node is Vector4ConstantNode vector4Constant)
                {
                    Build(vector4Constant, builder);
                }
                else if (node is Color3ConstantNode color3Constant)
                {
                    Build(color3Constant, builder);
                }
                else if (node is Color4ConstantNode color4Constant)
                {
                    Build(color4Constant, builder);
                }
                else if (node is MixNode mix)
                {
                    Build(mix, builder);
                }
                else if (node is IMathOpNode mathOp)
                {
                    Build(mathOp, builder);
                }
                else if (node is OutputNode outNode)
                {
                    Build(outNode, builder);
                }
            }
            var body = builder.ToString();
            StringBuilder builder1 = new();
            BuildHeader(builder1);
            BuildBody(body, builder1);
            return builder1.ToString();
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
                    if (node.Links[i].TargetNode == node)
                    {
                        var src = node.Links[i].SourceNode;
                        if (includeStatic && src.IsStatic || !src.IsStatic)
                            stack1.Push(node.Links[i].SourceNode);
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
                    break;
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
                throw new NullReferenceException();
            var op = Find(other);
            if (!op.Type.IsStruct)
            {
                return new(op.Name, op.Type);
            }
            else
            {
                var link = Node.FindSourceLink(target, other);
                if (link == null)
                    throw new NullReferenceException();
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
                throw new NullReferenceException();
            var op = Find(link.SourceNode);
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

            var op = Find(link.SourceNode);
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

        public Definition GetVariableFirstLink(Pin pin)
        {
            if (pin.Links.Count == 0)
                throw new NullReferenceException();
            var link = pin.Links[0];

            var op = Find(link.SourceNode);
            if (!op.Type.IsStruct)
            {
                return new(op.Name, op.Type);
            }
            else
            {
                return new($"{op.Type.Name}.{link.Output.Name}", op.Type);
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

            var op = Find(link.SourceNode);
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

        private Operation AddVariable(string name, Node node, SType type)
        {
            string newName = table.GetUniqueName(name);
            var id = mapping.Count;
            mapping.Add(node, id);
            return table.AddVariable(new(id, newName, type));
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

        private void BuildBody(string body, StringBuilder builder)
        {
            var type = SType.Parse(output.Type);

            if (type.IsStruct)
                builder.AppendLine($"{output.Type} main({input.Name} {inputVar.Name})");
            else
                builder.AppendLine($"{output.Type} main({input.Name} {inputVar.Name}) : SV_TARGET");
            builder.AppendLine("{");
            builder.Append(body);
            builder.AppendLine("}");
        }

        private Operation Build(TextureFileNode node, StringBuilder builder)
        {
            var srv = AddSrv($"Srv{node.Name}", new(TextureType.Texture2D), new(VectorType.Float4));
            var sampler = AddSampler($"Sampler{node.Name}", new(SamplerType.SamplerState));
            var output = AddVariable(node.Name, node, new(VectorType.Float4));
            builder.AppendLine($"float4 {output.Name} = {srv.Name}.Sample({sampler.Name}, {inputVar.Name}.tex);");
            return output;
        }

        private Operation Build(FloatConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, new(ScalarType.Float));
            builder.AppendLine($"float {output.Name} = {node.Value.ToString(CultureInfo.InvariantCulture)};");
            return output;
        }

        private Operation Build(Vector2ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, new(VectorType.Float2));
            builder.AppendLine($"float2 {output.Name} = float2({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Operation Build(Vector3ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, new(VectorType.Float3));
            builder.AppendLine($"float3 {output.Name} = float3({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)},{node.Value.Z.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Operation Build(Vector4ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, new(VectorType.Float4));
            builder.AppendLine($"float4 {output.Name} = float4({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)},{node.Value.Z.ToString(CultureInfo.InvariantCulture)},{node.Value.W.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Operation Build(Color3ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, new(VectorType.Float3));
            builder.AppendLine($"float3 {output.Name} = float3({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)},{node.Value.Z.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Operation Build(Color4ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, new(VectorType.Float4));
            builder.AppendLine($"float4 {output.Name} = float4({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)},{node.Value.Z.ToString(CultureInfo.InvariantCulture)},{node.Value.W.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Operation Build(MixNode node, StringBuilder builder)
        {
            var left = GetVariableFirstLink(node.InLeft);
            var right = GetVariableFirstLink(node.InRight);
            var val = GetVariableFirstLink(node.InMix);
            var min = node.Type;
            var output = AddVariable(node.Name, node, min);
            builder.AppendLine($"{min.Name} {output.Name} = lerp({VariableTable.FromCastTo(left.Type, min)}{left.Name},{VariableTable.FromCastTo(right.Type, min)}{right.Name},{val.Name});");
            return output;
        }

        private Operation Build(Definition left, Definition right, SType type, Node node, string op, StringBuilder builder)
        {
            if (VariableTable.NeedCastPerComponentMath(left.Type, right.Type))
            {
                var output = AddVariable(node.Name, node, type);
                builder.AppendLine($"{type.Name} {output.Name} = {VariableTable.FromCastTo(left.Type, type)}{left.Name} {op} {VariableTable.FromCastTo(right.Type, type)}{right.Name};");
                return output;
            }
            else
            {
                var min = left.Type.IsScalar ? right.Type : left.Type;
                var output = AddVariable(node.Name, node, min);
                builder.AppendLine($"{min.Name} {output.Name} = {left.Name} {op} {right.Name};");
                return output;
            }
        }

        private Operation Build(IMathOpNode node, StringBuilder builder)
        {
            var left = GetVariableFirstLink(node.InLeft);
            var right = GetVariableFirstLink(node.InRight);
            return Build(left, right, node.Type, (Node)node, node.Op, builder);
        }

        private Operation Build(OutputNode node, StringBuilder builder)
        {
            var output = GetVariableFirstLink(node.Out);
            builder.AppendLine($"return {VariableTable.FromCastTo(output.Type, new(VectorType.Float4))}{output.Name};");
            return default;
        }
    }
}