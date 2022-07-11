namespace HexaEngine.Meshes
{
    using HexaEngine.Objects;
    using System;
    using System.Numerics;

    public struct MeshMaterial : IBinarySerializable
    {
        public string Name;
        public Vector3 AmbientColor;
        public Vector3 DiffuseColor;
        public Vector3 SpecularColor;
        public float SpecularCoefficient;
        public float Transparency;
        public int IlluminationModel;
        public string AmbientTextureMap;
        public string DiffuseTextureMap;
        public string SpecularTextureMap;
        public string SpecularHighlightTextureMap;
        public string BumpMap;
        public string DisplacementMap;
        public string StencilDecalMap;
        public string AlphaTextureMap;
        public string RoughnessTextureMap;
        public string MetallicTextureMap;

        public unsafe int SizeOf()
        {
            int size = BinaryHelper.SizeOfString(Name);

            size += sizeof(int);

            size += sizeof(uint);
            size += sizeof(Vector3);
            size += sizeof(uint);
            size += sizeof(Vector3);
            size += sizeof(uint);
            size += sizeof(Vector3);

            size += sizeof(uint);
            size += sizeof(float);
            size += sizeof(uint);
            size += sizeof(float);

            size += sizeof(uint);
            size += sizeof(int);

            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(AmbientTextureMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(DiffuseTextureMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(SpecularTextureMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(SpecularHighlightTextureMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(BumpMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(DisplacementMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(StencilDecalMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(AlphaTextureMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(RoughnessTextureMap);
            size += sizeof(uint);
            size += BinaryHelper.SizeOfString(MetallicTextureMap);

            return size;
        }

        public int Read(Span<byte> data)
        {
            int read = BinaryHelper.ReadString(data, out string name);
            Name = name;

            read += BinaryHelper.ReadInt32(data[read..], out int values);
            for (int i = 0; i < values; i++)
            {
                read += BinaryHelper.ReadEnumU32(data[read..], out MaterialValueType type);
                read += type switch
                {
                    MaterialValueType.AmbientColor => BinaryHelper.ReadVector3(data[read..], out AmbientColor),
                    MaterialValueType.DiffuseColor => BinaryHelper.ReadVector3(data[read..], out DiffuseColor),
                    MaterialValueType.SpecularColor => BinaryHelper.ReadVector3(data[read..], out SpecularColor),
                    MaterialValueType.SpecularCoefficient => BinaryHelper.ReadFloat(data[read..], out SpecularCoefficient),
                    MaterialValueType.Transparency => BinaryHelper.ReadFloat(data[read..], out Transparency),
                    MaterialValueType.IlluminationModel => BinaryHelper.ReadInt32(data[read..], out IlluminationModel),
                    MaterialValueType.AmbientTextureMap => BinaryHelper.ReadString(data[read..], out AmbientTextureMap),
                    MaterialValueType.DiffuseTextureMap => BinaryHelper.ReadString(data[read..], out DiffuseTextureMap),
                    MaterialValueType.SpecularTextureMap => BinaryHelper.ReadString(data[read..], out SpecularTextureMap),
                    MaterialValueType.SpecularHighlightTextureMap => BinaryHelper.ReadString(data[read..], out SpecularHighlightTextureMap),
                    MaterialValueType.BumpMap => BinaryHelper.ReadString(data[read..], out BumpMap),
                    MaterialValueType.DisplacementMap => BinaryHelper.ReadString(data[read..], out DisplacementMap),
                    MaterialValueType.StencilDecalMap => BinaryHelper.ReadString(data[read..], out StencilDecalMap),
                    MaterialValueType.AlphaTextureMap => BinaryHelper.ReadString(data[read..], out AlphaTextureMap),
                    MaterialValueType.RoughnessTextureMap => BinaryHelper.ReadString(data[read..], out RoughnessTextureMap),
                    MaterialValueType.MetallicTextureMap => BinaryHelper.ReadString(data[read..], out MetallicTextureMap),
                    _ => throw new Exception($"Invalid material value type {type}"),
                };
            }

            return read;
        }

        public int Write(Span<byte> dest)
        {
            int idx = BinaryHelper.WriteString(dest, Name);

            idx += BinaryHelper.WriteInt32(dest[idx..], 16);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.AmbientColor);
            idx += BinaryHelper.WriteVector3(dest[idx..], AmbientColor);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.DiffuseColor);
            idx += BinaryHelper.WriteVector3(dest[idx..], DiffuseColor);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.SpecularColor);
            idx += BinaryHelper.WriteVector3(dest[idx..], SpecularColor);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.SpecularCoefficient);
            idx += BinaryHelper.WriteFloat(dest[idx..], SpecularCoefficient);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.Transparency);
            idx += BinaryHelper.WriteFloat(dest[idx..], Transparency);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.IlluminationModel);
            idx += BinaryHelper.WriteInt32(dest[idx..], IlluminationModel);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.AmbientTextureMap);
            idx += BinaryHelper.WriteString(dest[idx..], AmbientTextureMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.DiffuseTextureMap);
            idx += BinaryHelper.WriteString(dest[idx..], DiffuseTextureMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.SpecularTextureMap);
            idx += BinaryHelper.WriteString(dest[idx..], SpecularTextureMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.SpecularHighlightTextureMap);
            idx += BinaryHelper.WriteString(dest[idx..], SpecularHighlightTextureMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.BumpMap);
            idx += BinaryHelper.WriteString(dest[idx..], BumpMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.DisplacementMap);
            idx += BinaryHelper.WriteString(dest[idx..], DisplacementMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.StencilDecalMap);
            idx += BinaryHelper.WriteString(dest[idx..], StencilDecalMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.AlphaTextureMap);
            idx += BinaryHelper.WriteString(dest[idx..], AlphaTextureMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.RoughnessTextureMap);
            idx += BinaryHelper.WriteString(dest[idx..], RoughnessTextureMap);

            idx += BinaryHelper.WriteEnumU32(dest[idx..], MaterialValueType.MetallicTextureMap);
            idx += BinaryHelper.WriteString(dest[idx..], MetallicTextureMap);

            return idx;
        }

        public static implicit operator Material(MeshMaterial material)
        {
            return new()
            {
                NormalTextureMap = material.BumpMap,
                Color = material.DiffuseColor,
                AlbedoTextureMap = material.DiffuseTextureMap,
                DisplacementTextureMap = material.DisplacementMap,
                MetalnessTextureMap = material.MetallicTextureMap,
                Name = material.Name,
                RoughnessTextureMap = material.RoughnessTextureMap,
                Opacity = material.Transparency,
                Emissivness = Vector3.Zero,
                Roughness = 0,
                Metalness = 0,
                Ao = 1,
            };
        }
    }
}