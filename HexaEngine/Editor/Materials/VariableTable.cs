namespace HexaEngine.Editor.Materials
{
    using HexaEngine.Core.Graphics.Reflection;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    public class VariableTable
    {
        private readonly List<UnorderedAccessView> unorderedAccessViews = new();
        private readonly List<ShaderResourceView> shaderResourceViews = new();
        private readonly List<ConstantBuffer> constantBuffers = new();
        private readonly List<SamplerState> samplers = new();
        private readonly List<Identifier> identifyers = new();
        private readonly List<Variable> variables = new();
        private readonly List<Struct> structs = new();

        public void Build(StringBuilder builder)
        {
            builder.AppendLine("/// Unordered Access Views");
            for (int i = 0; i < unorderedAccessViews.Count; i++)
            {
                unorderedAccessViews[i].Build(builder);
            }
            builder.AppendLine("/// Shader Resource Views");
            for (int i = 0; i < shaderResourceViews.Count; i++)
            {
                shaderResourceViews[i].Build(builder);
            }
            builder.AppendLine("/// Samplers");
            for (int i = 0; i < samplers.Count; i++)
            {
                samplers[i].Build(builder);
            }
            builder.AppendLine("/// Structures");
            for (int i = 0; i < structs.Count; i++)
            {
                structs[i].Build(builder);
            }
            builder.AppendLine("/// Constant Buffers");
            for (int i = 0; i < constantBuffers.Count; i++)
            {
                constantBuffers[i].Build(builder);
            }
        }

        public static VariableType DetermineMinimumType(VariableType a, VariableType b)
        {
            return (VariableType)Math.Min((int)a, (int)b);
        }

        public static string GetTypeName(VariableType type)
        {
            return type switch
            {
                VariableType.Unknown => throw new InvalidDataException(),
                VariableType.Struct => throw new InvalidOperationException(),
                VariableType.SamplerState => "SamplerState",
                VariableType.Texture1D => "Texture1D",
                VariableType.Texture2D => "Texture2D",
                VariableType.Texture3D => "Texture3D",
                VariableType.Texture1DArray => "Texture1DArray",
                VariableType.Texture2DArray => "Texture2DArray",
                VariableType.TextureCube => "TextureCube",
                VariableType.Float => "float",
                VariableType.Float2 => "float2",
                VariableType.Float3 => "float3",
                VariableType.Float4 => "float4",
                VariableType.Int => "int",
                VariableType.Int2 => "int2",
                VariableType.Int3 => "int3",
                VariableType.Int4 => "int4",
                VariableType.UInt => "uint",
                VariableType.UInt2 => "uint2",
                VariableType.UInt3 => "uint3",
                VariableType.UInt4 => "uint4",
                VariableType.Float1x1 => "float1x1",
                VariableType.Float1x2 => "float1x2",
                VariableType.Float1x3 => "float1x3",
                VariableType.Float1x4 => "float1x4",
                VariableType.Float2x1 => "float2x1",
                VariableType.Float2x2 => "float2x2",
                VariableType.Float2x3 => "float2x3",
                VariableType.Float2x4 => "float2x4",
                VariableType.Float3x1 => "float3x1",
                VariableType.Float3x2 => "float3x2",
                VariableType.Float3x3 => "float3x3",
                VariableType.Float3x4 => "float3x4",
                VariableType.Float4x1 => "float4x1",
                VariableType.Float4x2 => "float4x2",
                VariableType.Float4x3 => "float4x3",
                VariableType.Float4x4 => "float4x4",
                _ => throw new InvalidDataException(),
            };
        }

        public static VariableType GetType(string type)
        {
            return type switch
            {
                "SamplerState" => VariableType.SamplerState,
                "Texture1D" => VariableType.Texture1D,
                "Texture2D" => VariableType.Texture2D,
                "Texture3D" => VariableType.Texture3D,
                "Texture1DArray" => VariableType.Texture1DArray,
                "Texture2DArray" => VariableType.Texture2DArray,
                "TextureCube" => VariableType.TextureCube,
                "float" => VariableType.Float,
                "float2" => VariableType.Float2,
                "float3" => VariableType.Float3,
                "float4" => VariableType.Float4,
                "int" => VariableType.Int,
                "int2" => VariableType.Int2,
                "int3" => VariableType.Int3,
                "int4" => VariableType.Int4,
                "uint" => VariableType.UInt,
                "uint2" => VariableType.UInt2,
                "uint3" => VariableType.UInt3,
                "uint4" => VariableType.UInt4,
                "float1x1" => VariableType.Float1x1,
                "float1x2" => VariableType.Float1x2,
                "float1x3" => VariableType.Float1x3,
                "float1x4" => VariableType.Float1x4,
                "float2x1" => VariableType.Float2x1,
                "float2x2" => VariableType.Float2x2,
                "float2x3" => VariableType.Float2x3,
                "float2x4" => VariableType.Float2x4,
                "float3x1" => VariableType.Float3x1,
                "float3x2" => VariableType.Float3x2,
                "float3x3" => VariableType.Float3x3,
                "float3x4" => VariableType.Float3x4,
                "float4x1" => VariableType.Float4x1,
                "float4x2" => VariableType.Float4x2,
                "float4x3" => VariableType.Float4x3,
                "float4x4" => VariableType.Float4x4,
                _ => VariableType.Struct,
            };
        }

        public static string CastTo(VariableType type)
        {
            return type switch
            {
                VariableType.Unknown => throw new InvalidDataException(),
                VariableType.Struct => throw new InvalidOperationException(),
                VariableType.SamplerState => throw new InvalidOperationException(),
                VariableType.Texture1D => throw new InvalidOperationException(),
                VariableType.Texture2D => throw new InvalidOperationException(),
                VariableType.Texture3D => throw new InvalidOperationException(),
                VariableType.Texture1DArray => throw new InvalidOperationException(),
                VariableType.Texture2DArray => throw new InvalidOperationException(),
                VariableType.TextureCube => throw new InvalidOperationException(),
                VariableType.Float => "(float)",
                VariableType.Float2 => "(float2)",
                VariableType.Float3 => "(float3)",
                VariableType.Float4 => "(float4)",
                VariableType.Int => "(int)",
                VariableType.Int2 => "(int2)",
                VariableType.Int3 => "(int3)",
                VariableType.Int4 => "(int4)",
                VariableType.UInt => "(uint)",
                VariableType.UInt2 => "(uint2)",
                VariableType.UInt3 => "(uint3)",
                VariableType.UInt4 => "(uint4)",
                VariableType.Float1x1 => "(float1x1)",
                VariableType.Float1x2 => "(float1x2)",
                VariableType.Float1x3 => "(float1x3)",
                VariableType.Float1x4 => "(float1x4)",
                VariableType.Float2x1 => "(float2x1)",
                VariableType.Float2x2 => "(float2x2)",
                VariableType.Float2x3 => "(float2x3)",
                VariableType.Float2x4 => "(float2x4)",
                VariableType.Float3x1 => "(float3x1)",
                VariableType.Float3x2 => "(float3x2)",
                VariableType.Float3x3 => "(float3x3)",
                VariableType.Float3x4 => "(float3x4)",
                VariableType.Float4x1 => "(float4x1)",
                VariableType.Float4x2 => "(float4x2)",
                VariableType.Float4x3 => "(float4x3)",
                VariableType.Float4x4 => "(float4x4)",
                _ => throw new InvalidDataException(),
            };
        }

        public static bool IsScalar(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => true,
                VariableType.Int => true,
                VariableType.UInt => true,
                VariableType.Half => true,
                VariableType.Float => true,
                VariableType.Double => true,
                _ => false
            };
        }

        public static string FromCastTo(VariableType from, VariableType to)
        {
            if (from == to)
                return string.Empty;
            return CastTo(to);
        }

        public static bool NeedCastPerComponentMath(VariableType a, VariableType b)
        {
            return a != b && !IsScalar(a) && !IsScalar(b);
        }

        public void Clear()
        {
            unorderedAccessViews.Clear();
            shaderResourceViews.Clear();
            constantBuffers.Clear();
            identifyers.Clear();
            variables.Clear();
            samplers.Clear();
            structs.Clear();
        }

        public UnorderedAccessView AddUnorderedAccessView(UnorderedAccessView unorderedAccessView)
        {
            identifyers.Add(new(unorderedAccessView.Name));
            var last = unorderedAccessViews.LastOrDefault();
            if (last.Name == null)
            {
                unorderedAccessView.Slot = 0;
            }
            else
            {
                unorderedAccessView.Slot = last.Slot + 1;
            }
            unorderedAccessViews.Add(unorderedAccessView);
            return unorderedAccessView;
        }

        public ShaderResourceView AddShaderResourceView(ShaderResourceView shaderResourceView)
        {
            identifyers.Add(new(shaderResourceView.Name));
            var last = shaderResourceViews.LastOrDefault();
            if (last.Name == null)
            {
                shaderResourceView.Slot = 0;
            }
            else
            {
                shaderResourceView.Slot = last.Slot + 1;
            }
            shaderResourceViews.Add(shaderResourceView);
            return shaderResourceView;
        }

        public ConstantBuffer AddConstantBuffer(ConstantBuffer constantBuffer)
        {
            identifyers.Add(new(constantBuffer.Name));
            var last = constantBuffers.LastOrDefault();

            if (last.Name == null)
            {
                constantBuffer.Slot = 0;
            }
            else
            {
                constantBuffer.Slot = last.Slot + 1;
            }

            constantBuffers.Add(constantBuffer);
            return constantBuffer;
        }

        public SamplerState AddSamplerState(SamplerState samplerState)
        {
            identifyers.Add(new(samplerState.Name));
            var last = samplers.LastOrDefault();

            if (last.Name == null)
            {
                samplerState.Slot = 0;
            }
            else
            {
                samplerState.Slot = last.Slot + 1;
            }

            samplers.Add(samplerState);
            return samplerState;
        }

        public Variable AddVariable(Variable variable)
        {
            identifyers.Add(new(variable.Name));
            variables.Add(variable);
            return variable;
        }

        public Struct AddStruct(Struct @struct)
        {
            identifyers.Add(new(@struct.Name));
            structs.Add(@struct);
            return @struct;
        }

        public UnorderedAccessView GetUnorderedAccessView(string name)
        {
            for (int i = 0; i < unorderedAccessViews.Count; i++)
            {
                if (unorderedAccessViews[i].Name == name)
                    return unorderedAccessViews[i];
            }
            return default;
        }

        public ShaderResourceView GetShaderResourceView(string name)
        {
            for (int i = 0; i < shaderResourceViews.Count; i++)
            {
                if (shaderResourceViews[i].Name == name)
                    return shaderResourceViews[i];
            }
            return default;
        }

        public ConstantBuffer GetConstantBuffer(string name)
        {
            for (int i = 0; i < constantBuffers.Count; i++)
            {
                if (constantBuffers[i].Name == name)
                    return constantBuffers[i];
            }
            return default;
        }

        public SamplerState GetSamplerState(string name)
        {
            for (int i = 0; i < samplers.Count; i++)
            {
                if (samplers[i].Name == name)
                    return samplers[i];
            }
            return default;
        }

        public Variable GetVariable(string name)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Name == name)
                    return variables[i];
            }
            return default;
        }

        public void AddRef(string name)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Name == name)
                {
                    var va = variables[i];
                    va.Refs++;
                    variables[i] = va;
                }
            }
        }

        public Variable GetVariable(int id)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Id == id)
                    return variables[i];
            }
            return default;
        }

        public Struct GetStruct(string name)
        {
            for (int i = 0; i < structs.Count; i++)
            {
                if (structs[i].Name == name)
                    return structs[i];
            }
            return default;
        }

        public bool VariableExists(string name)
        {
            for (int i = 0; i < identifyers.Count; i++)
            {
                if (identifyers[i].Name == name)
                    return true;
            }
            return false;
        }

        public string GetUniqueName(string name)
        {
            string newName = name;
            int al = 0;
            while (VariableExists(newName))
            {
                newName = $"{name}{al.ToString(CultureInfo.InvariantCulture)}";
                al++;
            }

            return newName;
        }
    }
}