namespace HexaEngine.Lights.Types
{
    using HexaEngine.Components;
    using HexaEngine.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using System.Numerics;

    [EditorGameObject<DirectionalLight>("Directional Light")]
    public class DirectionalLight : Light
    {
        private static ulong instances;
        private static ConstantBuffer<Matrix4x4>? csmBuffer;

        private DepthStencil? csmDepthBuffer;
        public new CameraTransform Transform = new();

        [JsonIgnore]
        public BoundingFrustum[] ShadowFrustra = new BoundingFrustum[8];

        private int cascadeCount = 4;

        public DirectionalLight()
        {
            for (int i = 0; i < 8; i++)
            {
                ShadowFrustra[i] = new();
            }
            base.Transform = Transform;
            OverwriteTransform(Transform);
        }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Cascade Count")]
        public int CascadeCount
        {
            get => cascadeCount;
            set
            {
                cascadeCount = value % 8;
                DestroyShadowMap();
            }
        }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Split Mode")]
        public CascadesSplitMode SplitMode { get; set; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Split Lambda")]
        [EditorPropertyCondition<DirectionalLight>(nameof(SplitLambdaCheck))]
        public float SplitLambda { get; set; }

        [JsonIgnore]
        public override int ShadowMapSize => GraphicsSettings.GetSMSizeDirectionalLight(ShadowMapResolution);

        private static bool SplitLambdaCheck(DirectionalLight obj)
        {
            return obj.SplitMode == CascadesSplitMode.Log;
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public override LightType LightType => LightType.Directional;

        /// <inheritdoc/>
        [JsonIgnore]
        public override bool HasShadowMap => csmDepthBuffer != null;

        [JsonIgnore]
        public static IBuffer CSMBuffer => csmBuffer;

        /// <inheritdoc/>
        public override IShaderResourceView? GetShadowMap()
        {
            return csmDepthBuffer?.SRV;
        }

        /// <inheritdoc/>
        public override void CreateShadowMap(IGraphicsDevice device, ShadowAtlas atlas)
        {
            if (csmDepthBuffer != null)
            {
                return;
            }

            csmDepthBuffer = new(device, Format.D32Float, ShadowMapSize, ShadowMapSize, cascadeCount - 1);
            if (Interlocked.Increment(ref instances) == 1)
            {
                csmBuffer = new(device, 8, CpuAccessFlags.Write);
            }
        }

        /// <inheritdoc/>
        public override void DestroyShadowMap()
        {
            if (csmDepthBuffer == null)
            {
                return;
            }

            csmDepthBuffer?.Dispose();
            csmDepthBuffer = null;

            if (Interlocked.Decrement(ref instances) == 0)
            {
                csmBuffer?.Dispose();
                csmBuffer = null;
            }
        }

        public unsafe void UpdateShadowBuffer(StructuredUavBuffer<ShadowData> buffer, Camera camera)
        {
            var data = buffer.Local + QueueIndex;

            Matrix4x4* views = ShadowData.GetViews(data);
            float* cascades = ShadowData.GetCascades(data);

            CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, ShadowFrustra, ShadowMapSize, cascadeCount);
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer, Camera camera)
        {
            if (csmDepthBuffer == null)
            {
                return;
            }
#nullable disable
            var data = buffer.Local + QueueIndex;

            Matrix4x4* views = ShadowData.GetViews(data);
            float* cascades = ShadowData.GetCascades(data);

            var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, ShadowFrustra, ShadowMapSize, cascadeCount);
            context.Write(csmBuffer.Buffer, mtxs, sizeof(Matrix4x4) * cascadeCount);

            context.ClearDepthStencilView(csmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);
            context.SetRenderTarget(null, csmDepthBuffer.DSV);
            context.SetViewport(csmDepthBuffer.Viewport);
#nullable enable
        }

        /// <inheritdoc/>
        public override bool IntersectFrustum(BoundingBox box)
        {
            return true;
        }
    }
}