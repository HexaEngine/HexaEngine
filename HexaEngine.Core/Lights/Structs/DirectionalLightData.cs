namespace HexaEngine.Core.Lights.Structs
{
    using System.Numerics;
    using HexaEngine.Core.Lights.Types;

    public struct DirectionalLightData
    {
        public Vector4 Color;
        public Vector3 Direction;

        public DirectionalLightData(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
        }

        public void Update(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}