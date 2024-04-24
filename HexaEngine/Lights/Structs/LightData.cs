namespace HexaEngine.Lights.Structs
{
    using HexaEngine.Lights.Types;
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

        public LightData(Light light)
        {
            if (light is PointLight pointLight)
            {
                Type = (uint)pointLight.LightType;
                Color = pointLight.Color * pointLight.Intensity;
                Position = new Vector4(pointLight.Transform.GlobalPosition, 1);
                Range = pointLight.Range;
                CastsShadows = 0;
                SetBit(ref CastsShadows, 0, pointLight.ShadowMapEnable);
                SetBit(ref CastsShadows, 1, pointLight.ContactShadowsEnable);
                ShadowMapIndex = pointLight.QueueIndex;
            }
            else if (light is Spotlight spotlight)
            {
                Type = (uint)spotlight.LightType;
                Color = spotlight.Color * spotlight.Intensity;
                Position = new Vector4(spotlight.Transform.GlobalPosition, 1);
                Direction = new Vector4(spotlight.Transform.Forward, 1);
                OuterCosine = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
                InnerCosine = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
                Range = spotlight.Range;
                CastsShadows = 0;
                SetBit(ref CastsShadows, 0, spotlight.ShadowMapEnable);
                SetBit(ref CastsShadows, 1, spotlight.ContactShadowsEnable);
                ShadowMapIndex = spotlight.QueueIndex;
            }
            else if (light is DirectionalLight directionalLight)
            {
                Type = (uint)directionalLight.LightType;
                Color = directionalLight.Color * directionalLight.Intensity;
                Direction = new Vector4(directionalLight.Transform.Forward, 1);
                Range = directionalLight.Range;
                CastsShadows = 0;
                SetBit(ref CastsShadows, 0, directionalLight.ShadowMapEnable);
                SetBit(ref CastsShadows, 1, directionalLight.ContactShadowsEnable);
                ShadowMapIndex = directionalLight.QueueIndex;
            }
        }

        public LightData(PointLight light)
        {
            Type = (uint)light.LightType;
            Color = light.Color * light.Intensity;
            Position = new Vector4(light.Transform.GlobalPosition, 1);
            Range = light.Range;
            CastsShadows = 0;
            SetBit(ref CastsShadows, 0, light.ShadowMapEnable);
            SetBit(ref CastsShadows, 1, light.ContactShadowsEnable);
            ShadowMapIndex = light.QueueIndex;
        }

        public LightData(Spotlight light)
        {
            Type = (uint)light.LightType;
            Color = light.Color * light.Intensity;
            Position = new Vector4(light.Transform.GlobalPosition, 1);
            Direction = new Vector4(light.Transform.Forward, 1);
            OuterCosine = MathF.Cos((light.ConeAngle / 2).ToRad());
            InnerCosine = MathF.Cos((MathUtil.Lerp(0, light.ConeAngle, 1 - light.Blend) / 2).ToRad());
            Range = light.Range;
            CastsShadows = 0;
            SetBit(ref CastsShadows, 0, light.ShadowMapEnable);
            SetBit(ref CastsShadows, 1, light.ContactShadowsEnable);
            ShadowMapIndex = light.QueueIndex;
        }

        public LightData(DirectionalLight light)
        {
            Type = (uint)light.LightType;
            Color = light.Color * light.Intensity;
            Direction = new Vector4(light.Transform.Forward, 1);
            Range = light.Range;
            CastsShadows = 0;
            SetBit(ref CastsShadows, 0, light.ShadowMapEnable);
            SetBit(ref CastsShadows, 1, light.ContactShadowsEnable);
            ShadowMapIndex = light.QueueIndex;
        }

        public static void SetBit(ref int target, int bit, bool value)
        {
            if (value)
            {
                target |= 1 << bit;
            }
            else
            {
                target &= ~(1 << bit);
            }
        }

        public static bool GetBit(int target, int bit)
        {
            int mask = 1 << bit;
            target &= mask;
            return target != 0;
        }
    }
}