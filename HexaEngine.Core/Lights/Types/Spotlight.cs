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
    using System;
    using System.Numerics;

    [EditorNode<Spotlight>("Spotlight")]
    public class Spotlight : Light
    {
        public new CameraTransform Transform;

        private static ulong instances;
        private static IGraphicsPipeline? psmPipeline;
        private static IBuffer? psmBuffer;

        private DepthStencil? psmDepthBuffer;
        private float strength = 1;
        private float coneAngle;
        private float blend;
        private float falloff = 100;
        private Matrix4x4 view;

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

        [EditorProperty("Strength")]
        public float Strength { get => strength; set => SetAndNotifyWithEqualsTest(ref strength, value); }

        [EditorProperty("Shadow Range")]
        public float ShadowRange
        {
            get => Transform.Far;
            set
            {
                var target = Transform.Far;
                if (SetAndNotifyWithEqualsTest(ref target, value))
                    Transform.Far = target;
            }
        }

        [EditorProperty("Falloff")]
        public float Falloff { get => falloff; set => SetAndNotifyWithEqualsTest(ref falloff, value); }

        [EditorProperty("Cone Angle", 1f, 180f, EditorPropertyMode.Slider)]
        public float ConeAngle { get => coneAngle; set => SetAndNotifyWithEqualsTest(ref coneAngle, value); }

        [EditorProperty("Blend", 0f, 1f, EditorPropertyMode.Slider)]
        public float Blend { get => blend; set => SetAndNotifyWithEqualsTest(ref blend, value); }

        [JsonIgnore]
        public override bool HasShadowMap => psmDepthBuffer != null;

        public override void Initialize(IGraphicsDevice device)
        {
            base.Initialize(device);
        }

        public override IShaderResourceView? GetShadowMap()
        {
            return psmDepthBuffer?.SRV;
        }

        public override void CreateShadowMap(IGraphicsDevice device)
        {
            if (psmDepthBuffer != null) return;
            psmDepthBuffer = new(device, 2048, 2048, Format.D32Float); //new(device, TextureDescription.CreateTexture2DWithRTV(2048, 2048, 1, Format.R32Float), DepthStencilDesc.Default);

            if (Interlocked.Increment(ref instances) == 1)
            {
                psmBuffer = device.CreateBuffer(new Matrix4x4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

                psmPipeline = device.CreateGraphicsPipeline(new()
                {
                    VertexShader = "forward/psm/vs.hlsl",
                    HullShader = "forward/psm/hs.hlsl",
                    DomainShader = "forward/psm/ds.hlsl",
                    PixelShader = "forward/psm/ps.hlsl",
                },
                new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullFront,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.PatchListWith3ControlPoints,
                });
            }
        }

        public override void DestroyShadowMap()
        {
            if (psmDepthBuffer == null) return;
            psmDepthBuffer?.Dispose();
            psmDepthBuffer = null;
            if (Interlocked.Decrement(ref instances) == 0)
            {
                psmBuffer?.Dispose();
                psmPipeline?.Dispose();
            }
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowSpotlightData> buffer, IInstanceManager manager)
        {
            if (psmDepthBuffer == null) return;
#nullable disable

            view = PSMHelper.GetLightSpaceMatrix(Transform, ConeAngle.ToRad(), ShadowRange, ShadowFrustum);
            context.Write(psmBuffer, view);
            context.ClearDepthStencilView(psmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);
            context.SetRenderTarget(null, psmDepthBuffer.DSV);
            context.SetViewport(psmDepthBuffer.Viewport);
            context.SetGraphicsPipeline(psmPipeline);
            context.DSSetConstantBuffer(psmBuffer, 1);

            var types = manager.Types;
            for (int j = 0; j < types.Count; j++)
            {
                var type = types[j];
                if (type.BeginDrawNoOcculusion(context))
                {
                    context.DrawIndexedInstanced((uint)type.IndexCount, (uint)type.Visible, 0, 0, 0);
                }
            }
            context.ClearState();
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