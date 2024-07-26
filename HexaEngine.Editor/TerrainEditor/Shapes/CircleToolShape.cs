namespace HexaEngine.Editor.TerrainEditor.Shapes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.TerrainEditor;
    using Hexa.NET.Mathematics;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class CircleToolShape : TerrainToolShape
    {
        public override bool TestCell(TerrainToolContext context, TerrainTool tool, TerrainCell cell)
        {
            BoundingBox global = BoundingBox.Transform(cell.BoundingBox, cell.Transform);
            // we ignore the y axis.
            global.Max.Y = float.MaxValue;
            global.Min.Y = float.MinValue;
            BoundingSphere sphere = new(context.Position, tool.Size);
            return global.Intersects(sphere);
        }

        public override bool TestVertex(TerrainToolContext context, TerrainTool tool, TerrainVertex vertex, out float distance)
        {
            Vector3 vertexWS = Vector3.Transform(vertex.Position, context.Transform);

            Vector2 p1 = new(vertexWS.X, vertexWS.Z);
            Vector2 p2 = new(context.Position.X, context.Position.Z);

            distance = Vector2.Distance(p2, p1);

            return distance <= tool.Size;
        }
    }
}