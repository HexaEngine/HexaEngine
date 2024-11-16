using Hexa.NET.Mathematics;
using System.Numerics;
using System.Text;

namespace HexaEngine.Core.IO.Binary.Meshes
{
    public struct BlendShape
    {
        public string Name;
        public float Weight;
        public VertexFlags Flags;
        public uint VertexCount;
        public Vector4[] Colors;
        public Vector3[] Positions;
        public Vector3[] Normals;
        public Vector3[] Tangents;
        public UVChannel[] UVChannels;

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(Name, encoding, endianness);
            stream.WriteFloat(Weight, endianness);
            stream.WriteInt32((int)Flags, endianness);
            stream.WriteUInt32(VertexCount, endianness);

            if ((Flags & VertexFlags.Colors) != 0)
            {
                stream.WriteArrayVector4(Colors, endianness);
            }
            if ((Flags & VertexFlags.Positions) != 0)
            {
                stream.WriteArrayVector3(Positions, endianness);
            }

            if ((Flags & VertexFlags.UVs) != 0)
            {
                for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                {
                    UVChannel uvChannel = UVChannels[i];
                    switch (uvChannel.Type)
                    {
                        case UVType.Empty:
                            continue;

                        case UVType.UV2D:
                            stream.WriteArrayVector2(uvChannel.GetUV2D(), endianness);
                            break;

                        case UVType.UV3D:
                            stream.WriteArrayVector3(uvChannel.GetUV3D(), endianness);
                            break;

                        case UVType.UV4D:
                            stream.WriteArrayVector4(uvChannel.GetUV4D(), endianness);
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported UV type: {uvChannel.Type}");
                    }
                }
            }
            if ((Flags & VertexFlags.Normals) != 0)
            {
                stream.WriteArrayVector3(Normals, endianness);
            }
            if ((Flags & VertexFlags.Tangents) != 0)
            {
                stream.WriteArrayVector3(Tangents, endianness);
            }
        }

        public unsafe void Read(Stream stream, Encoding encoding, Endianness endianness, UVChannelInfo info)
        {
            Name = stream.ReadString(encoding, endianness)!;
            Weight = stream.ReadFloat(endianness);
            Flags = (VertexFlags)stream.ReadInt32(endianness);
            VertexCount = stream.ReadUInt32(endianness);

            if ((Flags & VertexFlags.Colors) != 0)
            {
                Colors = new Vector4[VertexCount];
                stream.ReadArrayVector4(Colors, endianness);
            }
            if ((Flags & VertexFlags.Positions) != 0)
            {
                Positions = new Vector3[VertexCount];
                stream.ReadArrayVector3(Positions, endianness);
            }

            if ((Flags & VertexFlags.UVs) != 0)
            {
                UVType* pType = (UVType*)&info;
                UVChannels = new UVChannel[UVChannelInfo.MaxChannels];
                for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                {
                    var type = pType[i];
                    UVChannel uvChannel = new(type, VertexCount);
                    switch (type)
                    {
                        case UVType.Empty:
                            continue;

                        case UVType.UV2D:
                            stream.ReadArrayVector2(uvChannel.GetUV2D(), endianness);
                            break;

                        case UVType.UV3D:
                            stream.ReadArrayVector3(uvChannel.GetUV3D(), endianness);
                            break;

                        case UVType.UV4D:
                            stream.ReadArrayVector4(uvChannel.GetUV4D(), endianness);
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported UV type: {type}");
                    }
                    UVChannels[i] = uvChannel;
                }
            }

            if ((Flags & VertexFlags.Normals) != 0)
            {
                Normals = new Vector3[VertexCount];
                stream.ReadArrayVector3(Normals, endianness);
            }
            if ((Flags & VertexFlags.Tangents) != 0)
            {
                Tangents = new Vector3[VertexCount];
                stream.ReadArrayVector3(Tangents, endianness);
            }
        }
    }
}