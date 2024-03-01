namespace HexaEngine.Editor.TerrainEditor
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System;
    using System.Numerics;

    public class TerrainToolContext
    {
        public Ray Ray { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 UV { get; set; }

        public TerrainGrid Grid { get; set; }

        public TerrainCell Cell { get; set; }

        public Matrix4x4 Transform => Cell.Transform;

        public Matrix4x4 TransformInverse => Cell.TransformInv;

        public HeightMap HeightMap => Cell.CellData.HeightMap;

        public uint VertexCount => Cell.VertexCount;

        public uint IndexCount => Cell.IndexCount;

        public Vector3[] Vertices => Cell.LODData.Positions;

        public Vector2[] UVs => Cell.LODData.UVs;

        public Vector3[] Normals => Cell.LODData.Normals;

        public Vector3[] Tangents => Cell.LODData.Tangents;

        public Vector2 GridDimensions { get; set; } = new(32);

        public TerrainToolShape Shape { get; set; }

        public bool TestVertex(TerrainTool tool, int i, out TerrainVertex vertex, out float distance)
        {
            vertex = new(Vertices[i], UVs[i], Normals[i], Tangents[i]);

            return Shape.TestVertex(this, tool, vertex, out distance);
        }

        public uint GetHeightMapIndex(TerrainVertex vertex, out HeightMap heightMap)
        {
            heightMap = HeightMap;
            Vector2 cTex = new Vector2(vertex.Position.X, vertex.Position.Z) / new Vector2(32);
            Vector2 pos = cTex * heightMap.Size;

            return heightMap.GetIndexFor((uint)pos.X, (uint)pos.Y);
        }

        public UPoint2 GetHeightMapPosition(TerrainVertex vertex, out HeightMap heightMap)
        {
            heightMap = HeightMap;
            Vector2 cTex = new Vector2(vertex.Position.X, vertex.Position.Z) / new Vector2(32);
            Vector2 pos = cTex * heightMap.Size;

            return new((uint)pos.X, (uint)pos.Y);
        }
    }
}