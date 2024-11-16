﻿namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    using Hexa.NET.SPIRVReflect;
    using System.Diagnostics.CodeAnalysis;

    public unsafe class ShaderReflector
    {
        public static bool ReflectShader(ShaderSpirvIL shader, [NotNullWhen(true)] out ShaderReflection? reflection)
        {
            SpvReflectShaderModule module;
            SpvReflectResult result = SPIRVReflect.CreateShaderModule((nuint)shader.Length, shader.Bytecode, &module);
            if (result != SpvReflectResult.Success)
            {
                reflection = null;
                return false;
            }

            List<ConstantBuffer> constantBuffers = new();
            for (int i = 0; i < module.DescriptorBindingCount; i++)
            {
                SpvReflectDescriptorBinding binding = module.DescriptorBindings[i];

                if (binding.DescriptorType == SpvReflectDescriptorType.SampledImage ||
                    binding.DescriptorType == SpvReflectDescriptorType.StorageImage ||
                    binding.DescriptorType == SpvReflectDescriptorType.UniformBuffer)
                {
                    Console.WriteLine($"Descriptor binding: {ToStringFromUTF8(binding.Name)}, Type: {binding.DescriptorType}");
                }

                if (binding.DescriptorType == SpvReflectDescriptorType.UniformBuffer)
                {
                    var name = ToStringFromUTF8(binding.Name);
                    var members = GetConstantBufferMembers(binding);
                    constantBuffers.Add(new ConstantBuffer(name, binding.Binding, binding.Block.Size, binding.Block.PaddedSize, members));
                }
            }

            reflection = new([.. constantBuffers]);

            SPIRVReflect.DestroyShaderModule(&module);

            return true;
        }

        private static ConstantBufferMemberInfo[] GetConstantBufferMembers(SpvReflectDescriptorBinding binding)
        {
            List<ConstantBufferMemberInfo> members = new();

            // Helper function to recursively process members
            ProcessBlockVariable(binding.Block, members);

            return [.. members];
        }

        private static void ProcessBlockVariable(SpvReflectBlockVariable blockVariable, List<ConstantBufferMemberInfo> members)
        {
            for (uint i = 0; i < blockVariable.MemberCount; ++i)
            {
                SpvReflectBlockVariable member = blockVariable.Members[i];
                if ((member.TypeDescription->TypeFlags & SpvReflectTypeFlags.Struct) != 0)
                {
                    // If the member is a struct, recursively process its members
                    ProcessBlockVariable(member, members);
                }
                else
                {
                    // Otherwise, add the member to the list
                    members.Add(new ConstantBufferMemberInfo
                    {
                        Name = ToStringFromUTF8(member.Name)!,
                        AbsoluteOffset = member.AbsoluteOffset,
                        Offset = member.Offset,
                        PaddedSize = member.PaddedSize,
                        Size = member.Size,
                        Type = AllocT(Convert(*member.TypeDescription))
                    });
                }
            }
        }

        private static TypeInfo Convert(SpvReflectTypeDescription description)
        {
            TypeInfo info;
            info.Id = description.Id;

            info.TypeName = default;
            if (description.TypeName != null)
            {
                info.TypeName.Append(description.TypeName);
            }

            info.StructMemberName = default;
            if (description.StructMemberName != null)
            {
                info.StructMemberName.Append(description.StructMemberName);
            }

            info.TypeFlags = (TypeFlags)description.TypeFlags;
            info.DecorationFlags = (DecorationFlags)description.DecorationFlags;
            info.Traits = Convert(description.Traits);

            if (description.StructTypeDescription != null)
            {
                info.StructTypeDescription = AllocT(Convert(*description.StructTypeDescription));
            }
            else
            {
                info.StructTypeDescription = null;
            }

            if (description.Members != null)
            {
                info.MemberCount = description.MemberCount;
                info.Members = AllocT<TypeInfo>(description.MemberCount);
                for (int i = 0; i < info.MemberCount; i++)
                {
                    info.Members[i] = Convert(description.Members[i]);
                }
            }
            else
            {
                info.MemberCount = 0;
                info.Members = null;
            }

            return info;
        }

        private static TypeTraits Convert(SpvReflectTypeDescription.TraitsUnion traits)
        {
            return new TypeTraits()
            {
                Array = Convert(traits.Array),
                Image = Convert(traits.Image),
                Numeric = Convert(traits.Numeric),
            };
        }

        private static ArrayTraits Convert(SpvReflectArrayTraits array)
        {
            ArrayTraits result;
            result.Stride = array.Stride;

            result.DimsCount = array.DimsCount;
            for (int i = 0; i < result.DimsCount; i++)
            {
                result.Dims[i] = (&array.Dims_0)[i];
                result.SpecConstantOpIds[i] = (&array.SpecConstantOpIds_0)[i];
            }

            return result;
        }

        private static ImageTraits Convert(SpvReflectImageTraits image)
        {
            ImageTraits result;
            result.Dim = (Dimension)image.Dim;
            result.Depth = image.Depth;
            result.Arrayed = image.Arrayed != 0;
            result.Ms = image.Ms != 0;
            result.Sampled = image.Sampled != 0;
            result.ImageFormat = (ImageFormat)image.ImageFormat;

            return result;
        }

        private static NumericTraits Convert(SpvReflectNumericTraits numeric)
        {
            NumericTraits result;
            result.Scalar = new Scalar() { Width = numeric.Scalar.Width, Signed = numeric.Scalar.Signedness != 0 };
            result.Vector = new Vector() { ComponentCount = numeric.Vector.ComponentCount };
            result.Matrix = new Matrix() { ColumnCount = numeric.Matrix.ColumnCount, RowCount = numeric.Matrix.RowCount, Stride = numeric.Matrix.Stride };

            return result;
        }
    }
}