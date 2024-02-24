namespace HexaEngine.Editor.TerrainEditor.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Editor.TerrainEditor;
    using HexaEngine.Mathematics;

    public class SmoothTool : TerrainTool
    {
        private int radius = 5;

        public int Radius { get => radius; set => radius = value; }

        public override string Name { get; } = "Smooth";

        public override bool DrawSettings(TerrainToolContext toolContext)
        {
            ImGui.InputInt("Smooth Radius", ref radius);
            return false;
        }

        public override bool Modify(IGraphicsContext context, TerrainToolContext toolContext)
        {
            bool hasAffected = false;
            for (int j = 0; j < toolContext.VertexCount; j++)
            {
                if (!toolContext.TestVertex(this, j, out var vertex, out var distance))
                {
                    continue;
                }

                float edgeFade = ComputeEdgeFade(distance);
                float value = Strength * edgeFade * Time.Delta;

                UPoint2 point = toolContext.GetHeightMapPosition(vertex, out var heightMap);
                uint index = heightMap.GetIndexFor(point.X, point.Y);

                // Get average height of neighboring vertices
                float avgHeight = ComputeAverageHeight(heightMap, point);
                // Smooth the current vertex towards the average height
                heightMap[index] = MathUtil.Lerp(heightMap[index], avgHeight, value);

                hasAffected = true;
            }

            return hasAffected;
        }

        private float ComputeAverageHeight(HeightMap map, UPoint2 center)
        {
            float sum = 0;
            uint samples = 0;

            // box blur kernel
            for (int x = -radius; x <= radius; ++x)
            {
                for (int y = -radius; y <= radius; ++y)
                {
                    Point2 offset = new(x, y);
                    Point2 newPos = (Point2)center + offset;
                    if (newPos.X < 0 || newPos.Y < 0 || newPos.X >= map.Width || newPos.Y >= map.Height)
                    {
                        continue;
                    }
                    sum += map[(uint)newPos.X, (uint)newPos.Y];
                    samples++;
                }
            }

            return sum / samples;
        }
    }
}