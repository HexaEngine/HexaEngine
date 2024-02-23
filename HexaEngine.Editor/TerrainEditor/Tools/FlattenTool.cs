namespace HexaEngine.Editor.TerrainEditor.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Editor.TerrainEditor;
    using System.Numerics;

    public class FlattenTool : TerrainTool
    {
        private bool autoHeight = true;
        private float targetHeight;
        private float autoHeightValue;

        public bool AutoHeight { get => autoHeight; set => autoHeight = value; }

        public float TargetHeight { get => targetHeight; set => targetHeight = value; }
        public override string Name { get; } = "Flatten";

        public override void DrawSettings()
        {
            ImGui.Checkbox("Auto Height", ref autoHeight);
            if (!autoHeight)
            {
                ImGui.InputFloat("Target Height", ref targetHeight);
            }
        }

        public override void OnMouseDown(Vector3 position)
        {
            autoHeightValue = position.Y;
        }

        public override bool Modify(TerrainToolContext context)
        {
            bool hasAffected = false;
            for (int j = 0; j < context.VertexCount; j++)
            {
                if (!context.TestVertex(this, j, out var vertex, out float distance))
                {
                    continue;
                }

                uint index = context.GetHeightMapIndex(vertex, out var heightMap);

                float edgeFade = ComputeEdgeFade(distance);
                float value = edgeFade * autoHeightValue;

                if (autoHeight)
                {
                    heightMap[index] = value;
                }
                else
                {
                    heightMap[index] = value;
                }

                hasAffected = true;
            }

            return hasAffected;
        }
    }
}