﻿namespace HexaEngine.Lights.Types
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
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

        private ShadowAtlasRangeHandle atlasHandle;

        [JsonIgnore]
        public BoundingBox ShadowBox = new();

        private float seamOffset;
        private float seamSize;

        [JsonIgnore]
        public override LightType LightType => LightType.Point;

        [JsonIgnore]
        public override bool HasShadowMap => atlasHandle.IsValid;

        [JsonIgnore]
        public static IBuffer OSMBuffer => osmBuffer;

        [EditorProperty("SeamOffset")]
        public float SeamOffset { get => seamOffset; set => seamOffset = value; }

        [EditorProperty("SeamSize")]
        public float SeamSize { get => seamSize; set => seamSize = value; }

        public override IShaderResourceView? GetShadowMap()
        {
            return null;
        }

        public override void CreateShadowMap(IGraphicsDevice device, ShadowAtlas atlas)
        {
            if (atlasHandle.IsValid)
            {
                return;
            }

            atlasHandle = atlas.AllocRange(ShadowMapSize, 6);

            if (Interlocked.Increment(ref instances) == 1)
            {
                osmBuffer = new(device, CpuAccessFlags.Write);
            }
        }

        public override void DestroyShadowMap()
        {
            if (!atlasHandle.IsValid)
            {
                return;
            }

            atlasHandle.Release();
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

            float texel = 1.0f / atlasHandle.Atlas.Size;

            for (int i = 0; i < 6; i++)
            {
                var vp = atlasHandle.Allocations[i].GetViewport();
                coords[i] = new Vector4(vp.X + 1, vp.Y + 1, vp.X + vp.Width - 1, vp.Y + vp.Height - 1) * texel;
            }

            OSMHelper.GetLightSpaceMatrices(Transform, Range, views, ref ShadowBox);
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer, int pass)
        {
            if (!atlasHandle.IsValid)
            {
                return;
            }
#nullable disable

            var data = buffer.Local + QueueIndex;
            data->Size = ShadowMapSize;
            data->Softness = 1;
            var views = ShadowData.GetViews(data);
            var coords = ShadowData.GetAtlasCoords(data);

            float texel = 1.0f / atlasHandle.Atlas.Size;

            for (int i = 0; i < 6; i++)
            {
                var vp = atlasHandle.Allocations[i].GetViewport();
                coords[i] = new Vector4(vp.X + 1, vp.Y + 1, vp.X + vp.Width - 1, vp.Y + vp.Height - 1) * texel;
            }

            OSMHelper.GetLightSpaceMatrices(Transform, Range, views, ref ShadowBox);

            var viewport = atlasHandle.Allocations[pass].GetViewport();
            osmBuffer.Local->View = views[pass];
            osmBuffer.Local->Position = Transform.GlobalPosition;
            osmBuffer.Local->Far = Range;
            osmBuffer.Update(context);

            context.ClearView(atlasHandle.Atlas.DSV, Vector4.One, viewport.Rect);
            context.SetRenderTarget(null, atlasHandle.Atlas.DSV);
            context.SetViewport(viewport);
#nullable enable
        }

        public override unsafe bool IntersectFrustum(BoundingBox box)
        {
            return ShadowBox.Intersects(box);
        }
    }
}