namespace HexaEngine.Core.Lights.Types
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Structs;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;
    using System;
    using System.Numerics;

    [EditorGameObject<Spotlight>("Spotlight")]
    public class Spotlight : Light
    {
        public new CameraTransform Transform;

        private static ulong instances;
        private static IBuffer? psmBuffer;

        private float coneAngle;
        private float blend;
        private Matrix4x4 view;
        private ShadowAtlasAllocation allocation;

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
        public override bool HasShadowMap => allocation.IsValid;

        [JsonIgnore]
        internal static IBuffer PSMBuffer => psmBuffer;

        public override void Initialize(IGraphicsDevice device)
        {
            base.Initialize(device);
        }

        public override IShaderResourceView? GetShadowMap()
        {
            return null;
        }

        public override void CreateShadowMap(IGraphicsDevice device)
        {
            if (allocation.IsValid)
            {
                return;
            }

            allocation = LightManager.Current.ShadowPool.Alloc(ShadowMapSize);

            if (Interlocked.Increment(ref instances) == 1)
            {
                psmBuffer = device.CreateBuffer(new Matrix4x4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            }
        }

        public override void DestroyShadowMap()
        {
            if (!allocation.IsValid)
            {
                return;
            }

            LightManager.Current.ShadowPool.Free(ref allocation);

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

            coords[0] = allocation.Offset * allocation.Size;
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer)
        {
            if (!allocation.IsValid)
            {
                return;
            }
#nullable disable

            var viewport = allocation.GetViewport();
            view = PSMHelper.GetLightSpaceMatrix(Transform, ConeAngle.ToRad(), Range, ShadowFrustum);
            context.Write(psmBuffer, view);
            context.ClearView(LightManager.Current.ShadowPool.DSV, Vector4.One, viewport.Rect);
            context.SetRenderTarget(null, LightManager.Current.ShadowPool.DSV);
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