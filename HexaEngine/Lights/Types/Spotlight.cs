namespace HexaEngine.Lights.Types
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;
    using System;
    using System.Numerics;

    [EditorGameObject<Spotlight>("Spotlight")]
    public class Spotlight : Light
    {
        public new CameraTransform Transform;

        private static ulong instances;
        private static ConstantBuffer<Matrix4x4>? psmBuffer;

        private float coneAngle;
        private float blend;
        private Matrix4x4 view;
        private ShadowAtlasHandle atlasHandle;

        public const int ShadowMapSize = 2048;

        [JsonIgnore]
        public readonly BoundingFrustum ShadowFrustum = new();

        [JsonIgnore]
        public Matrix4x4 View => view;

        [JsonIgnore]
        public override LightType LightType => LightType.Spot;

        public Spotlight()
        {
            base.Transform = Transform = new();
            OverwriteTransform(Transform);
        }

        [EditorProperty("Cone Angle", 1f, 180f, EditorPropertyMode.Slider)]
        public float ConeAngle { get => coneAngle; set => SetAndNotifyWithEqualsTest(ref coneAngle, value); }

        [EditorProperty("Blend", 0f, 1f, EditorPropertyMode.Slider)]
        public float Blend { get => blend; set => SetAndNotifyWithEqualsTest(ref blend, value); }

        [JsonIgnore]
        public override bool HasShadowMap => atlasHandle.IsValid;

        [JsonIgnore]
        internal static IBuffer PSMBuffer => psmBuffer;

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

            atlasHandle = atlas.Alloc(ShadowMapSize);

            if (Interlocked.Increment(ref instances) == 1)
            {
                psmBuffer = new(device, CpuAccessFlags.Write);
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
                psmBuffer?.Dispose();
                psmBuffer = null;
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

            var vp = atlasHandle.Allocation.GetViewport();
            coords[0] = new Vector4(vp.X, vp.Y, vp.X + vp.Width, vp.Y + vp.Height) * texel;

            var mapping = ShadowAtlasAllocation.GetTextureCoordsMapping((int)atlasHandle.Atlas.Size, vp);
        }

        public unsafe ShadowData GetShadowData()
        {
            if (!ShadowMapEnable)
            {
                return default;
            }
            ShadowData data = default;
            data.Size = ShadowMapSize;
            data.Softness = 1;
            var views = ShadowData.GetViews(&data);
            var coords = ShadowData.GetAtlasCoords(&data);

            views[0] = PSMHelper.GetLightSpaceMatrix(Transform, ConeAngle.ToRad(), Range, ShadowFrustum);

            float texel = 1.0f / atlasHandle.Atlas.Size;

            var vp = atlasHandle.Allocation.GetViewport();
            coords[0] = new Vector4(vp.X, vp.Y, vp.X + vp.Width, vp.Y + vp.Height) * texel;

            var mapping = ShadowAtlasAllocation.GetTextureCoordsMapping((int)atlasHandle.Atlas.Size, vp);
            return data;
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer)
        {
            if (!atlasHandle.IsValid)
            {
                return;
            }
#nullable disable

            var viewport = atlasHandle.Allocation.GetViewport();
            view = PSMHelper.GetLightSpaceMatrix(Transform, ConeAngle.ToRad(), Range, ShadowFrustum);
            context.Write(psmBuffer, view);
            context.ClearView(atlasHandle.Atlas.DSV, Vector4.One, viewport.Rect);
            context.SetRenderTarget(null, atlasHandle.Atlas.DSV);
            context.SetViewport(viewport);

#nullable enable
        }

        public float GetConeRadius(float z)
        {
            return MathF.Tan((coneAngle / 2).ToRad()) * z;
        }

        public (Vector3, Vector3) GetConeEllipse(float z)
        {
            float r = MathF.Tan((coneAngle / 2).ToRad()) * z;
            Vector3 major = new(r, 0, 0);
            Vector3 minor = new(0, r, 0);
            major = Vector3.Transform(major, Transform.Orientation);
            minor = Vector3.Transform(minor, Transform.Orientation);
            return (major, minor);
        }

        public float GetInnerConeRadius(float z)
        {
            return MathF.Tan((MathUtil.Lerp(0, coneAngle, 1 - blend) / 2).ToRad()) * z;
        }

        public (Vector3, Vector3) GetInnerConeEllipse(float z)
        {
            float r = MathF.Tan((MathUtil.Lerp(0, coneAngle, 1 - blend) / 2).ToRad()) * z;
            Vector3 major = new(r, 0, 0);
            Vector3 minor = new(0, r, 0);
            major = Vector3.Transform(major, Transform.Orientation);
            minor = Vector3.Transform(minor, Transform.Orientation);
            return (major, minor);
        }

        public override unsafe bool IntersectFrustum(BoundingBox box)
        {
            return ShadowFrustum.Intersects(box);
        }
    }
}