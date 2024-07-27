namespace HexaEngine.Core.IO.Binary.Meshes
{
    using Hexa.NET.Mathematics;
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

    /// <summary>
    /// Represents mesh data including vertex and index information, bounding volumes, and vertex flags.
    /// </summary>
    public unsafe class MeshData : IMeshData
    {
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
        public MeshData(string name, Guid guid, Guid materialName, VertexFlags flags, BoneData[]? bones)
        {
            this.name = name;
            this.guid = guid;
            MaterialId = materialName;
            Flags = flags;
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
        public static MeshData Read(Stream stream, Encoding encoding, Endianness endianness, Compression compression, MeshLoadMode loadMode)
        {
            MeshData data = new();
            data.endianness = endianness;
            data.compression = compression;
            data.name = stream.ReadString(encoding, endianness) ?? string.Empty;
            data.guid = stream.ReadGuid(endianness);
            data.MaterialId = stream.ReadGuid(endianness);
            data.Flags = (VertexFlags)stream.ReadInt32(endianness);
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

                    MeshLODData lod = MeshLODData.Read(decompressor, data.Flags, endianness);
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
            MeshLODData data = MeshLODData.Read(decompressor, Flags, endianness);

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

        public InputElementDescription[] InputElements
        {
            get
            {
                if ((Flags & VertexFlags.Skinned) != 0)
                {
                    return skinnedInputElements;
                }
                return inputElements;
            }
        }

        /// <summary>
        /// Describes the input elements for a non-skinned mesh.
        /// </summary>
        private static readonly InputElementDescription[] inputElements =
        [
            new("POSITION", 0, Format.R32G32B32Float, 0, 0),
            new("TEXCOORD", 0, Format.R32G32B32Float, 12, 0),
            new("NORMAL", 0, Format.R32G32B32Float, 24, 0),
            new("TANGENT", 0, Format.R32G32B32Float, 36, 0),
        ];

        /// <summary>
        /// Describes the input elements for a skinned mesh.
        /// </summary>
        private static readonly InputElementDescription[] skinnedInputElements =
        [
            new("POSITION", 0, Format.R32G32B32Float, 0, 0),
            new("TEXCOORD", 0, Format.R32G32B32Float, 12, 0),
            new("NORMAL", 0, Format.R32G32B32Float, 24, 0),
            new("TANGENT", 0, Format.R32G32B32Float, 36, 0),
            new("BLENDINDICES", 0, Format.R32G32B32A32UInt, 48, 0),
            new("BLENDWEIGHT", 0, Format.R32G32B32A32Float, 64, 0),
        ];

        /// <summary>
        /// Removes bone-related information from the mesh.
        /// </summary>
        public void Debone()
        {
            BoneCount = 0;
            Bones = null;
            Flags ^= VertexFlags.Skinned;
        }
    }
}