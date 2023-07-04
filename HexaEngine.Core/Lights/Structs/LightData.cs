namespace HexaEngine.Core.Lights.Structs
{
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct LightData
    {
        public uint Type;
        public Vector4 Color;
        public Vector4 Direction;
        public Vector4 Position;
        public float Range;
        public float OuterCosine;
        public float InnerCosine;
        public int CastsShadows;
        public int CascadedShadows;
        public uint ShadowMapIndex;
        public uint padd;

        public LightData(PointLight light, Camera camera)
        {
            Type = (uint)light.LightType;
            Color = light.Color * light.Intensity;
            Position = new Vector4(light.Transform.GlobalPosition, 1);
            Range = light.Range;
            CastsShadows = light.ShadowMapEnable ? 1 : 0;
            ShadowMapIndex = 0;
        }

        public LightData(Spotlight light, Camera camera)
        {
            Type = (uint)light.LightType;
            Color = light.Color * light.Intensity;
            Position = new Vector4(light.Transform.GlobalPosition, 1);
            Direction = new Vector4(light.Transform.Forward, 1);
            OuterCosine = MathF.Cos((light.ConeAngle / 2).ToRad());
            InnerCosine = MathF.Cos((MathUtil.Lerp(0, light.ConeAngle, 1 - light.Blend) / 2).ToRad());
            Range = light.Range;
            CastsShadows = light.ShadowMapEnable ? 1 : 0;
            ShadowMapIndex = 0;
        }

        public LightData(DirectionalLight light, Camera camera)
        {
            Type = (uint)light.LightType;
            Color = light.Color * light.Intensity;
            Direction = new Vector4(light.Transform.Forward, 1);
            Range = light.Range;
            CastsShadows = light.ShadowMapEnable ? 1 : 0;
            ShadowMapIndex = 0;
        }
    }
}