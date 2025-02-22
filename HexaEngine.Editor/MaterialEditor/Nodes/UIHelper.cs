namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;

    public static class UIHelper
    {
        public static bool EditSType(string name, ref SType type)
        {
            ImGui.PushID(name);
            bool result = false;
            var flags = type.Flags;
            if (ComboEnumHelper<TypeFlags>.Combo("##Type", ref flags))
            {
                type = default;
                type.Flags = flags;
                result = true;
            }

            if (flags != 0)
            {
                ImGui.SameLine();
            }

            if (type.IsScalar)
            {
                result |= ComboEnumHelper<ScalarType>.Combo("##SubType", ref type.ScalarType);
            }
            else if (type.IsVector)
            {
                result |= ComboEnumHelper<VectorType>.Combo("##SubType", ref type.VectorType);
            }
            else if (type.IsMatrix)
            {
                result |= ComboEnumHelper<MatrixType>.Combo("##SubType", ref type.MatrixType);
            }
            else if (type.IsSampler)
            {
                result |= ComboEnumHelper<SamplerType>.Combo("##SubType", ref type.SamplerType);
            }
            else if (type.IsBuffer)
            {
                result |= ComboEnumHelper<BufferType>.Combo("##SubType", ref type.BufferType);
            }
            else if (type.IsTexture)
            {
                result |= ComboEnumHelper<TextureType>.Combo("##SubType", ref type.TextureType);
            }
            else if (type.IsUavBuffer)
            {
                result |= ComboEnumHelper<UavBufferType>.Combo("##SubType", ref type.UavBufferType);
            }
            else if (type.IsUavTexture)
            {
                result |= ComboEnumHelper<UavTextureType>.Combo(name, ref type.UavTextureType);
            }
            ImGui.PopID();

            return result;
        }
    }
}