namespace HexaEngine.Core.IO.Binary.Meshes
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Numerics;
    using System.Text;
    using Version = Version;

    public struct UVChannelInfo
    {
        public UVType Channel0;
        public UVType Channel1;
        public UVType Channel2;
        public UVType Channel3;

        public const int MaxChannels = 4;
    }

    /// <summary>
    /// Represents mesh data including vertex and index information, bounding volumes, and vertex flags.
    /// </summary>
    public unsafe class MeshData : IMeshData
    {
        private Version subVersion;
        private Endianness endianness;
        private Compression compression;

        private string name;
        private Guid guid;

        /// <summary>
        /// The name of the mesh.
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// The GUID of the mesh.
        /// </summary>
        public Guid Guid { get => guid; set => guid = value; }

        /// <summary>
        /// The GUID of the material associated with the mesh.
        /// </summary>
        public Guid MaterialId;

        /// <summary>
        /// Flags indicating which vertex components are present in the mesh.
        /// </summary>
        public VertexFlags Flags;

        /// <summary>
        /// UV Channel info.
        /// </summary>
        public UVChannelInfo ChannelInfo;

        /// <summary>
        /// The number of bones in the mesh.
        /// </summary>
        public uint BoneCount;

        /// <summary>
        /// The array of bone data representing the skeleton structure of the mesh.
        /// </summary>
        public BoneData[]? Bones;

        public MeshLODDataSeekTable SeekTable = new();

        /// <summary>
        ///
        /// </summary>
        public List<MeshLODData> LODs = [];

#nullable disable

        /// <summary>
        /// Private default constructor for internal use.
        /// </summary>
        private MeshData()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshData"/> class with specified parameters.
        /// </summary>
        /// <param name="name">The name of the mesh.</param>
        /// <param name="guid">The GUID of the mesh.</param>
        /// <param name="materialName">The name of the material associated with the mesh.</param>
        /// <param name="flags"></param>
        /// <param name="channelInfo"></param>
        /// <param name="bones"></param>
        public MeshData(string name, Guid guid, Guid materialName, VertexFlags flags, UVChannelInfo channelInfo, BoneData[]? bones)
        {
            this.name = name;
            this.guid = guid;
            MaterialId = materialName;
            Flags = flags;
            ChannelInfo = channelInfo;
            BoneCount = (uint)(bones?.Length ?? 0);
            Bones = bones;
        }

        /// <summary>
        /// Reads mesh data from a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness used for reading numerical data.</param>
        /// <param name="compression"></param>
        /// <param name="loadMode"></param>
        /// <param name="version">The sub version.</param>
        public static MeshData Read(Stream stream, Encoding encoding, Endianness endianness, Compression compression, MeshLoadMode loadMode, Version version)
        {
            MeshData data = new();
            data.subVersion = version;
            data.endianness = endianness;
            data.compression = compression;
            data.name = stream.ReadString(encoding, endianness) ?? string.Empty;
            data.guid = stream.ReadGuid(endianness);
            data.MaterialId = stream.ReadGuid(endianness);
            data.Flags = (VertexFlags)stream.ReadInt32(endianness);
            if (version == new Version(1, 0, 0, 0))
            {
                data.ChannelInfo.Channel0 = (data.Flags & VertexFlags.UVs) != 0 ? UVType.UV2D : UVType.Empty;
            }
            else
            {
                UVChannelInfo info;
                var pType = (UVType*)&info;
                for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                {
                    *pType = (UVType)stream.ReadByte();
                    pType++;
                }
                data.ChannelInfo = info;
            }

            data.BoneCount = stream.ReadUInt32(endianness);
            if ((data.Flags & VertexFlags.Skinned) != 0)
            {
                data.Bones = new BoneData[data.BoneCount];
                for (int i = 0; i < data.BoneCount; i++)
                {
                    data.Bones[i].Read(stream, encoding, endianness);
                }
            }

            data.SeekTable.Read(stream, endianness);

            if (loadMode == MeshLoadMode.Immediate)
            {
                for (int i = 0; i < data.SeekTable.Entries.Count; i++)
                {
                    var entry = data.SeekTable.Entries[i];
                    stream.Position = entry.Offset;

                    Stream decompressor = CreateDecompressionStream(stream, compression, out var isCompressed);

                    MeshLODData lod = MeshLODData.Read(decompressor, data.Flags, endianness, version, data.ChannelInfo);
                    data.LODs.Add(lod);

                    if (isCompressed)
                    {
                        decompressor.Dispose();
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.SeekTable.Entries.Count; i++)
                {
                    var entry = data.SeekTable.Entries[i];
                    stream.Position += entry.Size;
                }
            }

            return data;
        }

        private static Stream CreateDecompressionStream(Stream stream, Compression compression, out bool isCompressed)
        {
            if (compression == Compression.Deflate)
            {
                isCompressed = true;
                return new DeflateStream(stream, CompressionMode.Decompress, true);
            }

            if (compression == Compression.LZ4)
            {
                isCompressed = true;
                return LZ4Stream.Decode(stream, 0, true);
            }

            isCompressed = false;
            return stream;
        }

        private static Stream CreateCompressionStream(Stream stream, Compression compression, out bool isCompressed)
        {
            if (compression == Compression.Deflate)
            {
                isCompressed = true;
                return new DeflateStream(stream, CompressionLevel.SmallestSize, true);
            }

            if (compression == Compression.LZ4)
            {
                isCompressed = true;
                return LZ4Stream.Encode(stream, LZ4Level.L12_MAX, 0, true);
            }

            isCompressed = false;
            return stream;
        }

        /// <summary>
        /// Writes the mesh data to a stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write the data to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness used for writing numerical data.</param>
        /// <param name="compression"></param>
        public void Write(Stream stream, Encoding encoding, Endianness endianness, Compression compression)
        {
            // Write basic information
            stream.WriteString(name, encoding, endianness);
            stream.WriteGuid(guid, endianness);
            stream.WriteGuid(MaterialId, endianness);
            stream.WriteInt32((int)Flags, endianness);
            UVChannelInfo info = ChannelInfo;
            var pType = (UVType*)&info;
            for (int i = 0; i < UVChannelInfo.MaxChannels; i++, pType++)
            {
                stream.WriteByte((byte)*pType);
            }
            stream.WriteUInt32(BoneCount, endianness);
            if ((Flags & VertexFlags.Skinned) != 0)
            {
                for (int i = 0; i < BoneCount; i++)
                {
                    Bones[i].Write(stream, encoding, endianness);
                }
            }

            SeekTable.Clear();

            long baseOffset = stream.Position;

            stream.Position += MeshLODDataSeekTable.GetSize(LODs.Count);

            long last = stream.Position;
            for (int i = 0; i < LODs.Count; i++)
            {
                Stream compressor = CreateCompressionStream(stream, compression, out var isCompressed);

                MeshLODData lod = LODs[i];
                lod.Write(compressor, Flags, endianness);

                if (isCompressed)
                {
                    compressor.Flush();
                    compressor.Dispose();
                }

                long now = stream.Position;
                long size = now - last;
                SeekTable.Entries.Add(new(lod.LODLevel, (uint)last, (uint)size));
                last = now;
            }

            long before = stream.Position;
            stream.Position = baseOffset;
            SeekTable.Write(stream, endianness);
            stream.Position = before;
        }

        public MeshLODData LoadLODData(int lodIndex, Stream stream)
        {
            long offset = SeekTable.GetOffsetFromIndex(lodIndex);
            stream.Position = offset;

            Stream decompressor = CreateDecompressionStream(stream, compression, out var isCompressed);
            MeshLODData data = MeshLODData.Read(decompressor, Flags, endianness, subVersion, ChannelInfo);

            if (isCompressed)
            {
                decompressor.Dispose();
            }

            return data;
        }

        /// <summary>
        /// Gathers bone data for the specified vertex ID.
        /// </summary>
        /// <param name="vertexId">The ID of the vertex.</param>
        /// <returns>A tuple containing bone IDs and weights for the specified vertex ID.</returns>
        public (Point4 boneIds, Vector4 weigths) GatherBoneData(int vertexId)
        {
            Point4 boneIds = default;
            Vector4 weigths = default;

#nullable disable
            int m = 0;
            for (int i = 0; i < Bones.Length; i++)
            {
                var bone = Bones[i];
                for (uint j = 0; j < bone.Weights.Length; j++)
                {
                    var weight = bone.Weights[j];
                    if (weight.VertexId == vertexId)
                    {
                        boneIds[m] = i;
                        weigths[m] = weight.Weight;
                        m++;
                        if (m == 4)
                        {
                            break;
                        }
                    }
                    else
                    {
                        boneIds[m] = -1;
                    }
                }
                if (m == 4)
                {
                    break;
                }
            }
#nullable restore

            return (boneIds, weigths);
        }

        public unsafe InputElementDescription[] InputElements
        {
            get
            {
                List<InputElementDescription> inputElements = [];
                int offset = 0;
                if ((Flags & VertexFlags.Colors) != 0)
                {
                    inputElements.Add(new("COLOR", 0, Format.R32G32B32A32Float, offset, 0));
                    offset += 16;
                }
                if ((Flags & VertexFlags.Positions) != 0)
                {
                    inputElements.Add(new("POSITION", 0, Format.R32G32B32Float, offset, 0));
                    offset += 12;
                }
                if ((Flags & VertexFlags.UVs) != 0)
                {
                    var info = ChannelInfo;
                    UVType* pType = (UVType*)&info;
                    for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                    {
                        switch (pType[i])
                        {
                            case UVType.Empty:
                                continue;

                            case UVType.UV2D:
                                inputElements.Add(new("TEXCOORD", 0, Format.R32G32Float, offset, i));
                                offset += 8;
                                break;

                            case UVType.UV3D:
                                inputElements.Add(new("TEXCOORD", 0, Format.R32G32B32Float, offset, i));
                                offset += 12;
                                break;

                            case UVType.UV4D:
                                inputElements.Add(new("TEXCOORD", 0, Format.R32G32B32A32Float, offset, i));
                                offset += 16;
                                break;
                        }
                    }
                }
                if ((Flags & VertexFlags.Normals) != 0)
                {
                    inputElements.Add(new("NORMAL", 0, Format.R32G32B32Float, offset, 0));
                    offset += 12;
                }
                if ((Flags & VertexFlags.Tangents) != 0)
                {
                    inputElements.Add(new("TANGENT", 0, Format.R32G32B32Float, offset, 0));
                    offset += 12;
                }
                if ((Flags & VertexFlags.Skinned) != 0)
                {
                    inputElements.Add(new("BLENDINDICES", 0, Format.R32G32B32A32UInt, offset, 0));
                    offset += 16;
                    inputElements.Add(new("BLENDWEIGHT", 0, Format.R32G32B32A32Float, offset, 0));
                    // offset += 16; // Commented out for potential future use
                }

                return [.. inputElements];
            }
        }

        public ShaderMacro[] GetShaderMacros()
        {
            List<ShaderMacro> macros = [];
            if ((Flags & VertexFlags.Colors) != 0)
            {
                macros.Add(new("VtxColors", "1"));
            }
            if ((Flags & VertexFlags.Positions) != 0)
            {
                macros.Add(new("VtxPos", "1"));
            }
            if ((Flags & VertexFlags.UVs) != 0)
            {
                macros.Add(new("VtxUVs", "1"));  // TODO: UV channels, not supported yet by the file format.
                var info = ChannelInfo;
                UVType* pType = (UVType*)&info;
                for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                {
                    var type = pType[i];

                    if (type == UVType.Empty)
                        continue;

                    macros.Add(new($"VtxUVs{i}", "1"));
                    macros.Add(new($"VtxUV{i}Type", type.ToHLSL()));
                }
            }
            if ((Flags & VertexFlags.Normals) != 0)
            {
                macros.Add(new("VtxNormals", "1"));
            }
            if ((Flags & VertexFlags.Tangents) != 0)
            {
                macros.Add(new("VtxTangents", "1"));
            }
            if ((Flags & VertexFlags.Skinned) != 0)
            {
                macros.Add(new("VtxSkinned", "1"));
            }
            return [.. macros];
        }

        /// <summary>
        /// Removes bone-related information from the mesh.
        /// </summary>
        public void Debone()
        {
            BoneCount = 0;
            Bones = null;
            Flags &= ~VertexFlags.Skinned;
        }
    }
}