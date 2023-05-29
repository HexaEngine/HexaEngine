namespace HexaEngine.Editor.MaterialEditor.Generator
{
    using HexaEngine.Editor.MaterialEditor.Generator.Structs;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public struct Include
    {
        public string Name;
    }

    public class VariableTable
    {
        private readonly List<Identifier> identifiers = new();

        private readonly List<UnorderedAccessView> unorderedAccessViews = new();
        private readonly List<ShaderResourceView> shaderResourceViews = new();
        private readonly List<ConstantBuffer> constantBuffers = new();
        private readonly List<SamplerState> samplers = new();
        private readonly List<Operation> operations = new();
        private readonly List<Struct> structs = new();
        private readonly List<Include> includes = new();

        private int srvCounter;
        private int uavCounter;
        private int cbvCounter;
        private int sptCounter;

        public int OperationCount => operations.Count;

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

        public static string CastTo(SType type)
        {
            if (type.IsScalar || type.IsVector || type.IsMatrix || type.IsStruct)
            {
                return $"({type.Name})";
            }
            throw new InvalidCastException();
        }

        public static string FromCastTo(SType from, SType to)
        {
            if (from == to)
            {
                return string.Empty;
            }

            return CastTo(to);
        }

        public static bool NeedCastPerComponentMath(SType a, SType b)
        {
            return a != b && !a.IsScalar && !b.IsScalar;
        }

        public void Clear()
        {
            unorderedAccessViews.Clear();
            shaderResourceViews.Clear();
            constantBuffers.Clear();
            identifiers.Clear();
            operations.Clear();
            samplers.Clear();
            structs.Clear();
            includes.Clear();
            srvCounter = 0;
            uavCounter = 0;
            cbvCounter = 0;
            sptCounter = 0;
        }

        public bool IsIncluded(string name)
        {
            for (int i = 0; i < includes.Count; i++)
            {
                var include = includes[i];
                if (include.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOfInclude(string name)
        {
            for (int i = 0; i < includes.Count; i++)
            {
                var include = includes[i];
                if (include.Name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public void AddInclude(string name)
        {
            if (!IsIncluded(name))
            {
                includes.Add(new() { Name = name });
            }
        }

        public void RemoveInclude(string name)
        {
            var index = IndexOfInclude(name);
            if (index != -1)
            {
                includes.RemoveAt(index);
            }
        }

        public UnorderedAccessView AddUnorderedAccessView(UnorderedAccessView unorderedAccessView)
        {
            identifiers.Add(new(unorderedAccessView.Name));
            unorderedAccessView.Slot = uavCounter++;
            unorderedAccessViews.Add(unorderedAccessView);
            return unorderedAccessView;
        }

        public ShaderResourceView AddShaderResourceView(ShaderResourceView shaderResourceView)
        {
            identifiers.Add(new(shaderResourceView.Name));
            shaderResourceView.Slot = srvCounter++;
            shaderResourceViews.Add(shaderResourceView);
            return shaderResourceView;
        }

        public ConstantBuffer AddConstantBuffer(ConstantBuffer constantBuffer)
        {
            identifiers.Add(new(constantBuffer.Name));
            constantBuffer.Slot = cbvCounter++;
            constantBuffers.Add(constantBuffer);
            return constantBuffer;
        }

        public SamplerState AddSamplerState(SamplerState samplerState)
        {
            identifiers.Add(new(samplerState.Name));
            samplerState.Slot = sptCounter++;
            samplers.Add(samplerState);
            return samplerState;
        }

        public Operation AddVariable(Operation variable)
        {
            identifiers.Add(new(variable.Name));
            operations.Add(variable);
            return variable;
        }

        public Struct AddStruct(Struct @struct)
        {
            identifiers.Add(new(@struct.Name));
            structs.Add(@struct);
            return @struct;
        }

        public UnorderedAccessView GetUnorderedAccessView(string name)
        {
            for (int i = 0; i < unorderedAccessViews.Count; i++)
            {
                if (unorderedAccessViews[i].Name == name)
                {
                    return unorderedAccessViews[i];
                }
            }
            return default;
        }

        public ShaderResourceView GetShaderResourceView(string name)
        {
            for (int i = 0; i < shaderResourceViews.Count; i++)
            {
                if (shaderResourceViews[i].Name == name)
                {
                    return shaderResourceViews[i];
                }
            }
            return default;
        }

        public ConstantBuffer GetConstantBuffer(string name)
        {
            for (int i = 0; i < constantBuffers.Count; i++)
            {
                if (constantBuffers[i].Name == name)
                {
                    return constantBuffers[i];
                }
            }
            return default;
        }

        public SamplerState GetSamplerState(string name)
        {
            for (int i = 0; i < samplers.Count; i++)
            {
                if (samplers[i].Name == name)
                {
                    return samplers[i];
                }
            }
            return default;
        }

        public Operation GetVariable(string name)
        {
            for (int i = 0; i < operations.Count; i++)
            {
                if (operations[i].Name == name)
                {
                    return operations[i];
                }
            }
            return default;
        }

        public void AddRef(string name)
        {
            for (int i = 0; i < operations.Count; i++)
            {
                if (operations[i].Name == name)
                {
                    var va = operations[i];
                    va.Refs++;
                    operations[i] = va;
                }
            }
        }

        public Operation GetVariable(int id)
        {
            for (int i = 0; i < operations.Count; i++)
            {
                if (operations[i].Id == id)
                {
                    return operations[i];
                }
            }
            return default;
        }

        public Struct GetStruct(string name)
        {
            for (int i = 0; i < structs.Count; i++)
            {
                if (structs[i].Name == name)
                {
                    return structs[i];
                }
            }
            return default;
        }

        public bool VariableExists(string name)
        {
            for (int i = 0; i < identifiers.Count; i++)
            {
                if (identifiers[i].Name == name)
                {
                    return true;
                }
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

        public Operation GetOperation(int i)
        {
            return operations[i];
        }
    }
}