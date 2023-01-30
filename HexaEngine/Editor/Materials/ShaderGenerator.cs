namespace HexaEngine.Editor.Materials
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Materials.Generator;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Nodes;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public class ShaderMethod
    {
        public string Name;
    }

    public struct ShaderParameter
    {
        public int Index;
        public string Name;
        public Type Type;
    }

    public class ShaderGenerator
    {
        private readonly VariableTable table = new();
        private readonly Dictionary<Node, int> mapping = new();

        private Struct input;
        private Variable inputVar;
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
                    new("pos", VariableType.Float4, null, "SV_POSITION"),
                    new("tex", VariableType.Float2, null, "TEXCOORD0"),
                    new("normal", VariableType.Float3, null, "NORMAL"),
                    new("tangent", VariableType.Float3, null, "TANGENT"),
                },
            };
            input = table.AddStruct(input);
            inputVar = table.AddVariable(new(-1, table.GetUniqueName(input.Name.ToLower()), VariableType.Struct, input.Name));

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

        private ShaderResourceView AddSrv(string name, VariableType type)
        {
            return table.AddShaderResourceView(new(table.GetUniqueName(name), type));
        }

        private SamplerState AddSampler(string name)
        {
            return table.AddSamplerState(new(table.GetUniqueName(name)));
        }

        private Variable AddVariable(string name, Node node, VariableType type, string? structTypeName = null)
        {
            string newName = table.GetUniqueName(name);
            var id = mapping.Count;
            mapping.Add(node, id);
            return table.AddVariable(new(id, newName, type, structTypeName));
        }

        private Variable Find(Node node)
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
            var type = VariableTable.GetType(output.Type);

            if (type != VariableType.Struct)
                builder.AppendLine($"{output.Type} main({input.Name} {inputVar.Name}) : SV_TARGET");
            else
                builder.AppendLine($"{output.Type} main({input.Name} {inputVar.Name})");
            builder.AppendLine("{");
            builder.Append(body);
            builder.AppendLine("}");
        }

        private Variable Build(TextureFileNode node, StringBuilder builder)
        {
            var srv = AddSrv($"Srv{node.Name}", VariableType.Texture2D);
            var sampler = AddSampler($"Sampler{node.Name}");
            var output = AddVariable(node.Name, node, VariableType.Float4);
            builder.AppendLine($"float4 {output.Name} = {srv.Name}.Sample({sampler.Name}, {inputVar.Name}.tex);");
            return output;
        }

        private Variable Build(FloatConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, VariableType.Float);
            builder.AppendLine($"float {output.Name} = {node.Value.ToString(CultureInfo.InvariantCulture)};");
            return output;
        }

        private Variable Build(Vector2ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, VariableType.Float2);
            builder.AppendLine($"float2 {output.Name} = float2({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Variable Build(Vector3ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, VariableType.Float3);
            builder.AppendLine($"float3 {output.Name} = float3({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)},{node.Value.Z.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Variable Build(Vector4ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, VariableType.Float4);
            builder.AppendLine($"float4 {output.Name} = float4({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)},{node.Value.Z.ToString(CultureInfo.InvariantCulture)},{node.Value.W.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Variable Build(Color3ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, VariableType.Float3);
            builder.AppendLine($"float3 {output.Name} = float3({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)},{node.Value.Z.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Variable Build(Color4ConstantNode node, StringBuilder builder)
        {
            var output = AddVariable(node.Name, node, VariableType.Float4);
            builder.AppendLine($"float4 {output.Name} = float4({node.Value.X.ToString(CultureInfo.InvariantCulture)},{node.Value.Y.ToString(CultureInfo.InvariantCulture)},{node.Value.Z.ToString(CultureInfo.InvariantCulture)},{node.Value.W.ToString(CultureInfo.InvariantCulture)});");
            return output;
        }

        private Variable Build(MixNode node, StringBuilder builder)
        {
            var left = Find(node.InLeft.Links[0].SourceNode);
            var right = Find(node.InRight.Links[0].SourceNode);
            var val = Find(node.InMix.Links[0].SourceNode);
            var min = VariableTable.DetermineMinimumType(left.Type, right.Type);
            var type = VariableTable.GetTypeName(min);
            var output = AddVariable(node.Name, node, min);
            builder.AppendLine($"{type} {output.Name} = lerp({VariableTable.FromCastTo(left.Type, min)}{left.Name},{VariableTable.FromCastTo(right.Type, min)}{right.Name},{val.Name});");
            return output;
        }

        private Variable Build(Variable left, Variable right, Node node, string op, StringBuilder builder)
        {
            if (VariableTable.NeedCastPerComponentMath(left.Type, right.Type))
            {
                var min = VariableTable.DetermineMinimumType(left.Type, right.Type);
                var type = VariableTable.GetTypeName(min);
                var output = AddVariable(node.Name, node, min);
                builder.AppendLine($"{type} {output.Name} = {VariableTable.FromCastTo(left.Type, min)}{left.Name} {op} {VariableTable.FromCastTo(right.Type, min)}{right.Name};");
                return output;
            }
            else
            {
                var min = left.Type == VariableType.Float ? right.Type : left.Type;
                var type = VariableTable.GetTypeName(min);
                var output = AddVariable(node.Name, node, min);
                builder.AppendLine($"{type} {output.Name} = {left.Name} {op} {right.Name};");
                return output;
            }
        }

        private Variable Build(IMathOpNode node, StringBuilder builder)
        {
            var left = Find(node.InLeft.Links[0].SourceNode);
            var right = Find(node.InRight.Links[0].SourceNode);
            return Build(left, right, (Node)node, node.Op, builder);
        }

        private Variable Build(OutputNode node, StringBuilder builder)
        {
            var output = Find(node.Out.Links[0].SourceNode);
            builder.AppendLine($"return {VariableTable.FromCastTo(output.Type, VariableType.Float4)}{output.Name};");
            return default;
        }
    }
}