namespace HexaEngine.Lights.Structs
{
    using HexaEngine.Lights.Types;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public struct ShadowData
    {
        public static readonly unsafe int CascadePointerOffset = sizeof(Matrix4x4) * 8;
        public static readonly unsafe int AtlasCoordPointerOffset = sizeof(Matrix4x4) * 8 + sizeof(float) * 8 + sizeof(float) * 3;

        public Matrix4x4 View0;
        public Matrix4x4 View1;
        public Matrix4x4 View2;
        public Matrix4x4 View3;
        public Matrix4x4 View4;
        public Matrix4x4 View5;
        public Matrix4x4 View6;
        public Matrix4x4 View7;
        public float Cascade0;
        public float Cascade1;
        public float Cascade2;
        public float Cascade3;
        public float Cascade4;
        public float Cascade5;
        public float Cascade6;
        public float Cascade7;
        public float Size;
        public float Softness;
        public float CascadeCount;

        public Vector4 AtlasCoord0;
        public Vector4 AtlasCoord1;
        public Vector4 AtlasCoord2;
        public Vector4 AtlasCoord3;
        public Vector4 AtlasCoord4;
        public Vector4 AtlasCoord5;
        public Vector4 AtlasCoord6;
        public Vector4 AtlasCoord7;

        public float NormalBias;
        public float SlopeScaleDepthBias;

        public ShadowData(PointLight light, float size)
        {
            View0 = light.Transform.View;
            Size = size;
            Softness = 1;
            CascadeCount = 0;
            NormalBias = light.ShadowMapNormalBias;
            SlopeScaleDepthBias = light.ShadowMapSlopeScaleDepthBias;
        }

        public ShadowData(Spotlight light, float size)
        {
            View0 = light.View;
            Size = size;
            Softness = 1;
            CascadeCount = 0;
            NormalBias = light.ShadowMapNormalBias;
            SlopeScaleDepthBias = light.ShadowMapSlopeScaleDepthBias;
        }

        public ShadowData(DirectionalLight light, float size)
        {
            View0 = light.Transform.View;
            Size = size;
            Softness = 1;
            CascadeCount = light.CascadeCount;
            NormalBias = light.ShadowMapNormalBias;
            SlopeScaleDepthBias = light.ShadowMapSlopeScaleDepthBias;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Matrix4x4* GetViews(ShadowData* data)
        {
            return (Matrix4x4*)data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float* GetCascades(ShadowData* data)
        {
            return (float*)((byte*)data + CascadePointerOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Vector4* GetAtlasCoords(ShadowData* data)
        {
            return (Vector4*)((byte*)data + AtlasCoordPointerOffset);
        }
    };
}