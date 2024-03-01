namespace HexaEngine.Lights.Structs
{
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public struct CSMShadowParams
    {
        public Matrix4x4 View0;
        public Matrix4x4 View1;
        public Matrix4x4 View2;
        public Matrix4x4 View3;
        public Matrix4x4 View4;
        public Matrix4x4 View5;
        public Matrix4x4 View6;
        public Matrix4x4 View7;
        public uint CascadeCount;
        public UPoint3 Padding;

        public CSMShadowParams(Matrix4x4 view0, Matrix4x4 view1, Matrix4x4 view2, Matrix4x4 view3, Matrix4x4 view4, Matrix4x4 view5, Matrix4x4 view6, Matrix4x4 view7, uint cascadesCount)
        {
            View0 = view0;
            View1 = view1;
            View2 = view2;
            View3 = view3;
            View4 = view4;
            View5 = view5;
            View6 = view6;
            View7 = view7;
            CascadeCount = cascadesCount;
        }

        public unsafe Matrix4x4 this[uint index]
        {
            get => ((Matrix4x4*)Unsafe.AsPointer(ref this))[index];
            set => ((Matrix4x4*)Unsafe.AsPointer(ref this))[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Matrix4x4* GetViews(CSMShadowParams* data)
        {
            return (Matrix4x4*)data;
        }
    }

    public struct PSMShadowParams
    {
        public Matrix4x4 View;

        public PSMShadowParams(Matrix4x4 view)
        {
            View = view;
        }
    }

    public struct DPSMShadowParams
    {
        public Matrix4x4 View;
        public float Near;
        public float Far;
        public float HemiDir;
        public float _padd;

        public DPSMShadowParams(Matrix4x4 view, float near, float far, float hemiDir)
        {
            View = view;
            Near = near;
            Far = far;
            HemiDir = hemiDir;
            _padd = 0;
        }
    }

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
        public uint CascadeCount;

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
            CascadeCount = (uint)light.CascadeCount;
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