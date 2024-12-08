namespace HexaEngine.Lights.Types
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using System;
    using System.Numerics;

    [EditorCategory("Lights")]
    [EditorGameObject<Spotlight>("Spotlight")]
    public class Spotlight : Light
    {
        private float coneAngle;
        private float blend;
        private Matrix4x4 view;
        private ShadowAtlasHandle? atlasHandle;
        private readonly List<SpatialCacheHandle> cacheHandles = [];

        [JsonIgnore]
        public readonly BoundingFrustum ShadowFrustum = new();

        [JsonIgnore]
        public Matrix4x4 View => view;

        [JsonIgnore]
        public override LightType LightType => LightType.Spot;

        [JsonIgnore]
        public override int ShadowMapSize => GraphicsSettings.GetSMSizeSpotlight(ShadowMapResolution);

        [EditorProperty("Cone Angle", 1f, 180f, EditorPropertyMode.Slider)]
        public float ConeAngle { get => coneAngle; set => SetAndNotifyWithEqualsTest(ref coneAngle, value); }

        [EditorProperty("Blend", 0f, 1f, EditorPropertyMode.Slider)]
        public float Blend { get => blend; set => SetAndNotifyWithEqualsTest(ref blend, value); }

        [JsonIgnore]
        public override bool HasShadowMap => atlasHandle?.IsValid ?? false;

        public override IShaderResourceView? GetShadowMap()
        {
            return null;
        }

        protected override void OnCreateShadowMap(ShadowAtlas atlas)
        {
            if (HasShadowMap)
            {
                return;
            }

            atlasHandle = atlas.Alloc(ShadowMapSize);
            last = ShadowMapResolution;
        }

        protected override void OnDestroyShadowMap()
        {
            if (!HasShadowMap)
            {
                return;
            }

            atlasHandle?.Dispose();
        }

        public unsafe void UpdateShadowBuffer(StructuredUavBuffer<ShadowData> buffer)
        {
            if (!HasShadowMap)
            {
                return;
            }

            var data = buffer.Local + QueueIndex;
            data->Size = ShadowMapSize;
            data->Softness = ShadowMapLightBleedingReduction;
            var views = ShadowData.GetViews(data);
            var coords = ShadowData.GetAtlasCoords(data);

            float texel = 1.0f / atlasHandle!.Atlas.Size;

            var vp = atlasHandle.Handle.Viewport;
            coords[0] = new Vector4(vp.X, vp.Y, vp.Width, vp.Height) * texel;

            views[0] = PSMHelper.GetLightSpaceMatrix(Transform, ConeAngle.ToRad(), Range, ShadowFrustum);
        }

        public unsafe Viewport UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer, ConstantBuffer<PSMShadowParams> psmBuffer)
        {
            if (!HasShadowMap)
            {
                return default;
            }
#nullable disable

            var viewport = atlasHandle.Handle.Viewport;
            Matrix4x4 view;
            Matrix4x4 viewProjection;
            PSMHelper.GetLightSpaceMatrix(Transform, ConeAngle.ToRad(), Range, ShadowFrustum, &view, &viewProjection);
            psmBuffer.Update(context, new(view, viewProjection, Transform.GlobalPosition, Range, 0));

            return viewport;
#nullable enable
        }

        private ShadowResolution last;

        public override bool UpdateShadowMapSize(Camera camera, ShadowAtlas atlas)
        {
#if false
            var distance = camera.DistanceTo(this);

            var distanceScaled = distance / Range;

            ShadowResolution resolution = (ShadowResolution)MathUtil.Lerp((float)ShadowMapResolution, (float)ShadowResolution.Low, distanceScaled);

            resolution = (ShadowResolution)MathUtil.Clamp((int)resolution, (int)ShadowResolution.Low, (int)ShadowMapResolution);

            if (last != resolution)
            {
                last = resolution;
            }
            else
            {
                return false;
            }

            cacheHandles.Add(atlas.Cache(atlasHandle));

            var size = new Vector2(GraphicsSettings.GetSMSizeSpotlight(resolution));

            var index = cacheHandles.FindIndex(x => x.Handle.Size == size);

            SpatialCacheHandle? cacheHandle = null;
            if (index != -1)
            {
                cacheHandle = cacheHandles[index];
                cacheHandles.RemoveAt(index);
            }

            atlasHandle = atlas.Alloc(size, cacheHandle);
            return true;
#endif
            return false;
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
            major = Vector3.Transform(major, Transform.GlobalOrientation);
            minor = Vector3.Transform(minor, Transform.GlobalOrientation);
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
            major = Vector3.Transform(major, Transform.GlobalOrientation);
            minor = Vector3.Transform(minor, Transform.GlobalOrientation);
            return (major, minor);
        }

        public override unsafe bool IntersectFrustum(BoundingBox box)
        {
            return ShadowFrustum.Intersects(box);
        }
    }
}