using HexaEngine.Core.Graphics;
using MappedSubresource = Hexa.NET.D3D11.MappedSubresource;
using Usage = Hexa.NET.D3D11.Usage;

namespace HexaEngine.D3D11
{
    public unsafe struct D3D11VariableList
    {
        public struct ConstantVariable
        {
            public char* Name;
            public uint Hash;
            public uint Offset;
            public uint Size;
        }

        public ShaderStage Stage;
        public ConstantVariable* Variables;
        public int VariableCount;
        public readonly uint SizeInBytes;
        public ComPtr<ID3D11Buffer> Buffer;
        public byte* StagingBuffer;

        public D3D11VariableList(D3D11GraphicsDevice device, ShaderStage stage, ConstantVariable* variables, int variableCount, uint sizeInBytes)
        {
            Stage = stage;
            Variables = variables;
            VariableCount = variableCount;
            SizeInBytes = AlignmentHelper.AlignUp(sizeInBytes, 16);
            StagingBuffer = AllocT<byte>(SizeInBytes);
            BufferDesc desc = new(sizeInBytes, Usage.Default, (uint)BindFlag.ConstantBuffer, 0, 0, sizeInBytes);
            device.Device.CreateBuffer(ref desc, null, out Buffer);
        }

        private static ConstantVariable* Find(ConstantVariable* buckets, int capacity, uint hash, char* key)
        {
            uint index = (uint)(hash % capacity);
            bool exit = false;
            while (true)
            {
                var entry = &buckets[index];
                if (entry->Hash == 0)
                {
                    return entry;
                }
                else if (entry->Hash == hash && StrCmp(key, entry->Name) == 0)
                {
                    return entry;
                }

                index++;
                if (index == capacity)
                {
                    if (exit)
                    {
                        break;
                    }

                    index = 0;
                    exit = true;
                }
            }

            return null; // return null means not found and full.
        }

        private readonly ConstantVariable GetByName(string name)
        {
            if (VariableCount == 0)
            {
                // special case, check ahead to avoid unnecessary steps.
                throw new KeyNotFoundException();
            }

            fixed (char* ptr = name)
            {
                uint hash = (uint)name.GetHashCode();
                fixed (char* pName = name)
                {
                    var pEntry = Find(Variables, VariableCount, hash, pName);
                    if (pEntry != null && pEntry->Hash == hash)
                    {
                        return *pEntry;
                    }
                    throw new KeyNotFoundException();
                }
            }
        }

        private readonly bool TryGetByName(string name, out ConstantVariable parameter)
        {
            if (VariableCount == 0)
            {
                // special case, check ahead to avoid unnecessary steps.
                parameter = default;
                return false;
            }

            uint hash = (uint)name.GetHashCode();
            fixed (char* pName = name)
            {
                var pEntry = Find(Variables, VariableCount, hash, pName);
                if (pEntry != null && pEntry->Hash == hash)
                {
                    parameter = *pEntry;
                    return true;
                }
                parameter = default;
                return false;
            }
        }

        public static D3D11VariableList CreateFrom(D3D11GraphicsDevice device, ShaderStage stage, ID3D11ShaderReflectionConstantBuffer* cb)
        {
            ShaderBufferDesc desc = default;
            cb->GetDesc(ref desc);
            int num = (int)desc.Variables;
            ConstantVariable* variables = AllocT<ConstantVariable>(num);

            for (uint i = 0; i < num; ++i)
            {
                ID3D11ShaderReflectionVariable* reflectVar = cb->GetVariableByIndex(i);
                ShaderVariableDesc varDesc = default;
                reflectVar->GetDesc(ref varDesc);

                string name = ToStringFromUTF8(varDesc.Name) ?? throw new Exception("Name cannot be null, check your shader code ensure all constant buffer variables are named!");

                ConstantVariable variable;
                variable.Name = name.ToUTF16Ptr();
                variable.Hash = (uint)name.GetHashCode();
                variable.Offset = varDesc.StartOffset;
                variable.Size = varDesc.Size;

                *Find(variables, num, variable.Hash, variable.Name) = variable;
            }

            return new D3D11VariableList(device, stage, variables, num, desc.Size);
        }

        public readonly void TrySetByName<T>(string name, in T value) where T : unmanaged
        {
            if (!TryGetByName(name, out var variable))
            {
                return;
            }
            if (sizeof(T) != variable.Size)
            {
                throw new ArgumentException($"Size of type {typeof(T)} does not match size of constant buffer variable {name}.");
            }

            *(T*)(StagingBuffer + variable.Offset) = value;
        }

        public readonly void TrySetByName<T>(string name, T* values, uint count) where T : unmanaged
        {
            if (!TryGetByName(name, out var variable))
            {
                return;
            }
            uint totalSize = (uint)(sizeof(T) * count);
            if (totalSize != variable.Size)
            {
                throw new ArgumentException($"Total size of type {typeof(T)} does not match size of constant buffer variable {name}.");
            }

            MemcpyT(values, (T*)(StagingBuffer + variable.Offset), count);
        }

        public readonly void SetVariables<T>(string name, ReadOnlySpan<T> values) where T : unmanaged
        {
            fixed (T* ptr = values)
            {
                TrySetByName(name, ptr, (uint)values.Length);
            }
        }

        public readonly void TrySetUnbound<T>(in T value) where T : unmanaged
        {
            if (sizeof(T) > SizeInBytes)
            {
                throw new ArgumentException($"Size of type {typeof(T)} exceeds size of constant buffer.");
            }
            *(T*)StagingBuffer = value;
        }

        public readonly void Upload(IGraphicsContext context)
        {
            D3D11GraphicsContext d3dContext = (D3D11GraphicsContext)context;
            MappedSubresource res;
            ID3D11Resource* resource = (ID3D11Resource*)Buffer.Handle;
            d3dContext.DeviceContext.Map(resource, 0, Map.WriteDiscard, (uint)MapFlags.None, &res);
            MemcpyT(StagingBuffer, (byte*)res.PData, SizeInBytes);
            d3dContext.DeviceContext.Unmap(resource, 0);
        }

        public readonly void Clear()
        {
            MemsetT(StagingBuffer, 0, SizeInBytes);
        }

        public void Release()
        {
            if (Variables != null)
            {
                for (int i = 0; i < VariableCount; i++)
                {
                    Free(Variables[i].Name);
                }
                Free(Variables);
                Variables = null;
            }
            if (StagingBuffer != null)
            {
                Free(StagingBuffer);
                StagingBuffer = null;
            }
            if (Buffer.Handle != null)
            {
                Buffer.Release();
            }
        }
    }
}