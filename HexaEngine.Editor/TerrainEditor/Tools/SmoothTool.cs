namespace HexaEngine.Editor.TerrainEditor.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Editor.TerrainEditor;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;

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
            var cell = toolContext.Cell;
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
                float avgHeight = ComputeAverageHeight(cell, heightMap, point);

                // Smooth the current vertex towards the average height
                float newHeight = MathUtil.Lerp(heightMap[index], avgHeight, value);

                if (point.X == 0 && cell.Left != null)
                {
                    cell.Left.CellData.HeightMap[heightMap.Width - 1, point.Y] = newHeight;
                }

                if (point.Y == 0 && cell.Top != null)
                {
                    cell.Top.CellData.HeightMap[point.X, heightMap.Height - 1] = newHeight;
                }

                if (point.X == heightMap.Width - 1 && cell.Right != null)
                {
                    cell.Right.CellData.HeightMap[0, point.Y] = newHeight;
                }

                if (point.Y == heightMap.Height - 1 && cell.Bottom != null)
                {
                    cell.Bottom.CellData.HeightMap[point.X, 0] = newHeight;
                }

                heightMap[index] = newHeight;

                hasAffected = true;
            }

            return hasAffected;
        }

        private float ComputeAverageHeight(TerrainCell cell, HeightMap map, UPoint2 center)
        {
            float sum = 0;
            uint samples = 0;

            int width = (int)map.Width;
            int height = (int)map.Height;

            // box blur kernel
            for (int x = -radius; x <= radius; ++x)
            {
                for (int y = -radius; y <= radius; ++y)
                {
                    Point2 offset = new(x, y);
                    Point2 newPos = (Point2)center + offset;
                    var hm = map;

                    if (newPos.X < 0)
                    {
                        if ((newPos.Y < 0))
                        {
                            hm = (cell.Left?.Bottom?.CellData.HeightMap);
                            newPos.Y += height;
                        }
                        else if ((newPos.Y >= height))
                        {
                            hm = (cell.Left?.Top?.CellData.HeightMap);
                            newPos.Y %= height;
                        }
                        else
                        {
                            hm = (cell.Left?.CellData.HeightMap);
                        }
                        newPos.X += width;
                    }
                    else if (newPos.X >= width)
                    {
                        if (newPos.Y < 0)
                        {
                            hm = (cell.Right?.Bottom?.CellData.HeightMap);
                            newPos.Y += height;
                        }
                        else if (newPos.Y >= height)
                        {
                            hm = (cell.Right?.Top?.CellData.HeightMap);
                            newPos.Y %= height;
                        }
                        else
                        {
                            hm = (cell.Right?.CellData.HeightMap);
                        }
                        newPos.X %= width;
                    }
                    else if (newPos.Y < 0)
                    {
                        hm = cell.Bottom?.CellData.HeightMap;
                        newPos.Y += height;
                    }
                    else if (newPos.Y >= height)
                    {
                        hm = cell.Top?.CellData.HeightMap;
                        newPos.Y %= height;
                    }

                    if (hm == null)
                    {
                        continue;
                    }

                    sum += hm[(uint)newPos.X, (uint)newPos.Y];
                    samples++;
                }
            }

            return sum / samples;
        }
    }
}