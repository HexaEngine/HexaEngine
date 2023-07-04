namespace HexaEngine.Core.Lights.Types
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Structs;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;
    using System.Numerics;

    [EditorGameObject<PointLight>("Point Light")]
    public class PointLight : Light
    {
        private static ulong instances;
        private static ConstantBuffer<OmniShadowData>? osmBuffer;

        public const int ShadowMapSize = 512;

        private struct OmniShadowData
        {
            public Matrix4x4 View;
            public Vector3 Position;
            public float Far;
        }

        private DepthStencil? osmDepthBuffer;
        private ShadowAtlasAllocation[] allocations = new ShadowAtlasAllocation[6];

        [JsonIgnore]
        public BoundingBox ShadowBox = new();

        [JsonIgnore]
        public override LightType LightType => LightType.Point;

        [JsonIgnore]
        public override bool HasShadowMap => osmDepthBuffer != null;

        [JsonIgnore]
        public static IBuffer OSMBuffer => osmBuffer;

        public override IShaderResourceView? GetShadowMap()
        {
            return osmDepthBuffer?.SRV;
        }

        public ShadowAtlasAllocation[] GetShadows()
        {
            return allocations;
        }

        public override void CreateShadowMap(IGraphicsDevice device)
        {
            if (osmDepthBuffer != null)
            {
                return;
            }

            osmDepthBuffer = new(device, ShadowMapSize, ShadowMapSize, 6, Format.D32Float, ResourceMiscFlag.TextureCube);

            LightManager.Current.ShadowPool.AllocRange(ShadowMapSize, allocations);

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

            LightManager.Current.ShadowPool.FreeRange(allocations);

            osmDepthBuffer?.Dispose();
            osmDepthBuffer = null;
            if (Interlocked.Decrement(ref instances) == 0)
            {
                osmBuffer?.Dispose();
                osmBuffer = null;
                osmBuffer = null;
            }
        }

        public unsafe void UpdateShadowBuffer(StructuredUavBuffer<ShadowData> buffer)
        {
            var data = buffer.Local + QueueIndex;
            data->Size = ShadowMapSize;
            data->Softness = 1;
            var views = ShadowData.GetViews(data);
            var coords = ShadowData.GetAtlasCoords(data);
            for (int i = 0; i < 6; i++)
            {
                coords[i] = allocations[i].Offset * allocations[i].Size;
            }

            OSMHelper.GetLightSpaceMatrices(Transform, Range, views, ref ShadowBox);
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer, int pass)
        {
            if (osmDepthBuffer == null)
            {
                return;
            }
#nullable disable

            var data = buffer.Local + QueueIndex;
            data->Size = ShadowMapSize;
            data->Softness = 1;
            var views = ShadowData.GetViews(data);
            var coords = ShadowData.GetAtlasCoords(data);
            for (int i = 0; i < 6; i++)
            {
                coords[i] = allocations[i].Offset * allocations[i].Size;
            }

            OSMHelper.GetLightSpaceMatrices(Transform, Range, views, ref ShadowBox);

            var viewport = allocations[pass].GetViewport();
            osmBuffer.Local->View = views[pass];
            osmBuffer.Local->Position = Transform.GlobalPosition;
            osmBuffer.Local->Far = Range;
            osmBuffer.Update(context);
            context.ClearView(LightManager.Current.ShadowPool.DSV, Vector4.One, viewport.Rect);
            context.SetRenderTarget(null, LightManager.Current.ShadowPool.DSV);
            context.SetViewport(viewport);
#nullable enable
        }

        public override unsafe bool IntersectFrustum(BoundingBox box)
        {
            return ShadowBox.Intersects(box);
        }
    }
}