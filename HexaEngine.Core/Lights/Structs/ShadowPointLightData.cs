using HexaEngine.Core.Lights.Types;

namespace HexaEngine.Core.Lights.Structs
{
    using System.Numerics;

    public struct ShadowPointLightData
    {
        public Vector4 Color;
        public Vector3 Position;
        public float Far;

        public ShadowPointLightData(PointLight point) : this()
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
            Far = point.ShadowRange;
        }

        public void Update(PointLight point)
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
            Far = point.ShadowRange;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}