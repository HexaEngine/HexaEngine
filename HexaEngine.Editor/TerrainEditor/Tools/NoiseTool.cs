namespace HexaEngine.Editor.TerrainEditor.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Mathematics;
    using HexaEngine.Mathematics.Noise;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public enum NoiseMode
    {
        Add,
        Subtract,
        Both,
    }

    public class NoiseTool : TerrainTool
    {
        private readonly PerlinNoise noise = new();
        private float scale = 128;
        private NoiseMode mode;
        private Vector3 origin;
        private Vector3 noisePosition;

        public override string Name { get; } = "Noise";

        public override bool DrawSettings(TerrainToolContext toolContext)
        {
            ImGui.InputFloat("Scale", ref scale);
            ComboEnumHelper<NoiseMode>.Combo("Mode", ref mode);
            return false;
        }

        public override void OnMouseDown(Vector3 position)
        {
            origin = position;
        }

        public override void OnMouseMove(Vector3 position)
        {
            noisePosition = position - origin;
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

                Vector3 vertexWS = Vector3.Transform(vertex.Position, toolContext.Cell.Transform);

                Vector3 offset = toolContext.Position - vertexWS;

                float noiseValue = noise.Noise((noisePosition.X + offset.X) * scale, (noisePosition.Z + offset.Z) * scale);

                switch (mode)
                {
                    case NoiseMode.Add:
                        noiseValue = noiseValue.MapN1P1To01();
                        break;

                    case NoiseMode.Subtract:
                        noiseValue = 1 - noiseValue.MapN1P1To01();
                        break;
                }

                float value = Strength * noiseValue * Time.Delta;

                uint index = toolContext.GetHeightMapIndex(vertex, out var heightMap);

                heightMap[index] += value;

                hasAffected = true;
            }

            return hasAffected;
        }
    }
}