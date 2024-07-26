namespace HexaEngine.Core.IO
{
    using Hexa.NET.Mathematics;
    using System;
    using System.IO;
    using System.Numerics;

    /// <summary>
    /// Represents bone weight for a <see cref="SkinnedMeshVertex"/>.
    /// </summary>
    public struct BoneWeight
    {
        /// <summary>
        /// The array of bone IDs influencing the vertex.
        /// </summary>
        public Point4 BoneIds;

        /// <summary>
        /// The array of bone weights influencing the vertex.
        /// </summary>
        public Vector4 Weights;

        public BoneWeight((Point4 boneIds, Vector4 weights) value)
        {
            BoneIds = value.boneIds;
            Weights = value.weights;
        }

        public BoneWeight(Point4 boneIds, Vector4 weights)
        {
            BoneIds = boneIds;
            Weights = weights;
        }

        public BoneWeight(int boneIndex0, int boneIndex1, int boneIndex2, int boneIndex3, float boneWeight0, float boneWeight1, float boneWeight2, float boneWeight3)
        {
            BoneIds = new(boneIndex0, boneIndex1, boneIndex2, boneIndex3);
            Weights = new(boneWeight0, boneWeight1, boneWeight2, boneWeight3);
        }

        /// <summary>
        /// Reads a bone weight from a stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="endianness">The endianness.</param>
        public static BoneWeight Read(Stream stream, Endianness endianness)
        {
            BoneWeight boneWeight;
            boneWeight.BoneIds = Point4.Read(stream, endianness);
            boneWeight.Weights = stream.ReadVector4(endianness);
            return boneWeight;
        }

        /// <summary>
        /// Writes a bone weight to a stream.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        /// <param name="endianness">The endianness.</param>
        public readonly void Write(Stream stream, Endianness endianness)
        {
            BoneIds.Write(stream, endianness);
            stream.WriteVector4(Weights, endianness);
        }
    }
}