namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class UniformCollection
    {
        private readonly List<ConstantBufferVariable> constantBufferVariables = new();
        private readonly Dictionary<string, ConstantBufferVariable> nameToConstantBufferVariables = new();

        public enum ConstantBufferVariableType
        {
            Unknown,

            Bool,
            Bool2,
            Bool3,
            Bool4,

            Bool1x1,
            Bool1x2,
            Bool1x3,
            Bool1x4,
            Bool2x1,
            Bool2x2,
            Bool2x3,
            Bool2x4,
            Bool3x1,
            Bool3x2,
            Bool3x3,
            Bool3x4,
            Bool4x1,
            Bool4x2,
            Bool4x3,
            Bool4x4,

            Int,
            Int2,
            Int3,
            Int4,

            Int1x1,
            Int1x2,
            Int1x3,
            Int1x4,
            Int2x1,
            Int2x2,
            Int2x3,
            Int2x4,
            Int3x1,
            Int3x2,
            Int3x3,
            Int3x4,
            Int4x1,
            Int4x2,
            Int4x3,
            Int4x4,

            Float,
            Float2,
            Float3,
            Float4,

            Float1x1,
            Float1x2,
            Float1x3,
            Float1x4,
            Float2x1,
            Float2x2,
            Float2x3,
            Float2x4,
            Float3x1,
            Float3x2,
            Float3x3,
            Float3x4,
            Float4x1,
            Float4x2,
            Float4x3,
            Float4x4,

            UInt,
            UInt2,
            UInt3,
            UInt4,

            UInt1x1,
            UInt1x2,
            UInt1x3,
            UInt1x4,
            UInt2x1,
            UInt2x2,
            UInt2x3,
            UInt2x4,
            UInt3x1,
            UInt3x2,
            UInt3x3,
            UInt3x4,
            UInt4x1,
            UInt4x2,
            UInt4x3,
            UInt4x4,

            Double,
            Double2,
            Double3,
            Double4,

            Double1x1,
            Double1x2,
            Double1x3,
            Double1x4,
            Double2x1,
            Double2x2,
            Double2x3,
            Double2x4,
            Double3x1,
            Double3x2,
            Double3x3,
            Double3x4,
            Double4x1,
            Double4x2,
            Double4x3,
            Double4x4,

            Struct,
        }

        public enum MatrixMajor
        {
            Column,
            Row,
        }

        public struct ConstantBufferVariable
        {
            public uint BufferIndex;
            public uint VariableIndex;
            public string Name;
            public ConstantBufferVariableType Type;
            public uint Size;
            public uint Offset;
            public MatrixMajor Major;
        }

        public UniformCollection()
        {
        }

        private static ConstantBufferVariableType Transform(D3DShaderVariableClass variableClass, ConstantBufferVariableType baseVariableType, ConstantBufferVariableType matrixVariableType, uint rows, uint columns)
        {
            ConstantBufferVariableType result;
            if (variableClass is D3DShaderVariableClass.D3DSvcScalar or D3DShaderVariableClass.D3DSvcVector)
            {
                result = (ConstantBufferVariableType)((uint)baseVariableType + (columns - 1));
            }
            else if (variableClass == D3DShaderVariableClass.D3DSvcMatrixRows)
            {
                result = (ConstantBufferVariableType)((uint)matrixVariableType + (rows - 1) * 4);
                result = (ConstantBufferVariableType)((uint)result + (columns - 1));
            }
            else if (variableClass == D3DShaderVariableClass.D3DSvcMatrixColumns)
            {
                result = (ConstantBufferVariableType)((uint)matrixVariableType + (rows - 1) * 4);
                result = (ConstantBufferVariableType)((uint)result + (columns - 1));
            }
            else
            {
                throw new NotSupportedException($"Variable class {variableClass} is not supported!");
            }
            return result;
        }

        public void Append(Shader* pShader, ShaderStage stage)
        {
            ShaderCompiler.Reflect(pShader, out ComPtr<ID3D11ShaderReflection> reflection);

            ShaderDesc desc;
            reflection.GetDesc(&desc);
            for (uint i = 0; i < desc.ConstantBuffers; i++)
            {
                var cb = reflection.GetConstantBufferByIndex(i);
                ShaderBufferDesc bufferDesc;
                cb->GetDesc(&bufferDesc);
                for (uint j = 0; j < bufferDesc.Variables; j++)
                {
                    var v = cb->GetVariableByIndex(j);
                    ShaderVariableDesc varDesc;
                    v->GetDesc(&varDesc);

                    ConstantBufferVariable variable;
                    variable.BufferIndex = i;
                    variable.VariableIndex = j;
                    variable.Name = Utils.ToStr(varDesc.Name);
                    variable.Size = varDesc.Size;
                    variable.Offset = varDesc.StartOffset;
                    var type = v->GetType();
                    ShaderTypeDesc typeDesc;
                    type->GetDesc(&typeDesc);

                    uint rows = typeDesc.Rows;
                    uint columns = typeDesc.Columns;
                    ConstantBufferVariableType variableType = ConstantBufferVariableType.Unknown;

                    switch (typeDesc.Type)
                    {
                        case D3DShaderVariableType.D3DSvtBool:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.Bool, ConstantBufferVariableType.Bool1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtInt:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.Int, ConstantBufferVariableType.Int1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtFloat:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.Float, ConstantBufferVariableType.Float1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtString:
                            break;

                        case D3DShaderVariableType.D3DSvtUint:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.UInt, ConstantBufferVariableType.UInt1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtUint8:
                            break;

                        case D3DShaderVariableType.D3DSvtDouble:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.Double, ConstantBufferVariableType.Double1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtMin8float:
                            break;

                        case D3DShaderVariableType.D3DSvtMin10float:
                            break;

                        case D3DShaderVariableType.D3DSvtMin16float:
                            break;

                        case D3DShaderVariableType.D3DSvtMin12int:
                            break;

                        case D3DShaderVariableType.D3DSvtMin16int:
                            break;

                        case D3DShaderVariableType.D3DSvtMin16Uint:
                            break;

                        case D3DShaderVariableType.D3DSvtInt16:
                            break;

                        case D3DShaderVariableType.D3DSvtUint16:
                            break;

                        case D3DShaderVariableType.D3DSvtFloat16:
                            break;

                        case D3DShaderVariableType.D3DSvtInt64:
                            break;

                        case D3DShaderVariableType.D3DSvtUint64:
                            break;
                    }

                    variable.Type = variableType;
                }
            }

            reflection.Release();
        }
    }
}