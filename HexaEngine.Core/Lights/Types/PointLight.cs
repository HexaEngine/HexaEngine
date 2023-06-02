namespace HexaEngine.Core.Lights.Types
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Structs;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;
    using System.Numerics;

    [EditorNode<PointLight>("Point Light")]
    public class PointLight : Light
    {
        private static ulong instances;
        private static ConstantBuffer<OmniShadowData>? osmBuffer;

        private struct OmniShadowData
        {
            public Matrix4x4 M1;
            public Matrix4x4 M2;
            public Matrix4x4 M3;
            public Matrix4x4 M4;
            public Matrix4x4 M5;
            public Matrix4x4 M6;
            public Vector3 Position;
            public float Far;
        }

        private DepthStencil? osmDepthBuffer;
        private float shadowRange = 100;
        private float strength = 1;
        private float falloff = 100;

        [JsonIgnore]
        public BoundingBox ShadowBox = new();

        [EditorProperty("Shadow Range")]
        public float ShadowRange { get => shadowRange; set => SetAndNotifyWithEqualsTest(ref shadowRange, value); }

        [EditorProperty("Strength")]
        public float Strength { get => strength; set => SetAndNotifyWithEqualsTest(ref strength, value); }

        [EditorProperty("Falloff")]
        public float Falloff { get => falloff; set => SetAndNotifyWithEqualsTest(ref falloff, value); }

        [JsonIgnore]
        public override LightType LightType => LightType.Point;

        [JsonIgnore]
        public override bool HasShadowMap => osmDepthBuffer != null;

        public override IShaderResourceView? GetShadowMap()
        {
            return osmDepthBuffer?.SRV;
        }

        public override void CreateShadowMap(IGraphicsDevice device)
        {
            if (osmDepthBuffer != null)
            {
                return;
            }

            osmDepthBuffer = new(device, 2048, 2048, 6, Format.D32Float, ResourceMiscFlag.TextureCube);

            if (Interlocked.Increment(ref instances) == 1)
            {
                osmBuffer = new(device, CpuAccessFlags.Write);
            }
        }

        public override void DestroyShadowMap()
        {
            if (osmDepthBuffer == null)
            {
                return;
            }

            osmDepthBuffer?.Dispose();
            osmDepthBuffer = null;
            if (Interlocked.Decrement(ref instances) == 0)
            {
                osmBuffer?.Dispose();
                osmBuffer = null;
                osmBuffer = null;
            }
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowPointLightData> buffer)
        {
            if (osmDepthBuffer == null)
            {
                return;
            }
#nullable disable

            OSMHelper.GetLightSpaceMatrices(Transform, ShadowRange, (Matrix4x4*)osmBuffer.Local, ref ShadowBox);
            osmBuffer.Local->Position = Transform.GlobalPosition;
            osmBuffer.Local->Far = ShadowRange;
            osmBuffer.Update(context);
            context.ClearDepthStencilView(osmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);
            context.SetRenderTarget(null, osmDepthBuffer.DSV);
            context.SetViewport(osmDepthBuffer.Viewport);

#nullable enable
        }

        public override unsafe bool IntersectFrustum(BoundingBox box)
        {
            return ShadowBox.Intersects(box);
        }
    }
}