namespace HexaEngine.Lights.Types
{
    using HexaEngine.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using Hexa.NET.Mathematics;
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using System.Numerics;

    [EditorCategory("Lights")]
    [EditorGameObject<DirectionalLight>("Directional Light")]
    public class DirectionalLight : Light
    {
        private DepthStencil? csmDepthBuffer;
        private Texture2D? csmBuffer;

        private readonly BoundingFrustum[] shadowFrustra = new BoundingFrustum[8];

        private int cascadeCount = 4;
        private CascadesSplitMode splitMode = CascadesSplitMode.Log;
        private float splitLambda = 0.85f;
        public const int MaxCascadeCount = 8;

        public DirectionalLight()
        {
            for (int i = 0; i < 8; i++)
            {
                shadowFrustra[i] = new();
            }
        }

        [JsonIgnore]
        public IReadOnlyList<BoundingFrustum> ShadowFrustra => shadowFrustra;

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Cascade Count")]
        public int CascadeCount
        {
            get => cascadeCount;
            set
            {
                cascadeCount = MathUtil.Clamp(value, 1, MaxCascadeCount);
                DestroyShadowMap();
            }
        }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty<CascadesSplitMode>("Split Mode")]
        public CascadesSplitMode SplitMode { get => splitMode; set => splitMode = value; }

        [EditorCategory("Shadow Map", "Shadows")]
        [EditorProperty("Split Lambda")]
        public float SplitLambda { get => splitLambda; set => splitLambda = value; }

        [JsonIgnore]
        public override int ShadowMapSize => GraphicsSettings.GetSMSizeDirectionalLight(ShadowMapResolution);

        /// <inheritdoc/>
        [JsonIgnore]
        public override LightType LightType => LightType.Directional;

        /// <inheritdoc/>
        [JsonIgnore]
        public override bool HasShadowMap => csmBuffer != null;

        /// <inheritdoc/>
        public override IShaderResourceView? GetShadowMap()
        {
            return csmBuffer?.SRV;
        }

        public Texture2D? GetMap() => csmBuffer;

        /// <inheritdoc/>
        public override void CreateShadowMap(IGraphicsDevice device, ShadowAtlas atlas)
        {
            if (csmBuffer != null)
            {
                return;
            }

            csmBuffer = new(GraphicsSettings.ShadowMapFormat, ShadowMapSize, ShadowMapSize, cascadeCount - 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            csmBuffer.CreateArraySlices();
            csmDepthBuffer = new(Format.D32Float, ShadowMapSize, ShadowMapSize, cascadeCount - 1);
        }

        /// <inheritdoc/>
        public override void DestroyShadowMap()
        {
            if (csmBuffer != null)
            {
                csmBuffer?.Dispose();
                csmBuffer = null;
            }

            if (csmDepthBuffer != null)
            {
                csmDepthBuffer?.Dispose();
                csmDepthBuffer = null;
            }
        }

        public unsafe void UpdateShadowBuffer(StructuredUavBuffer<ShadowData> buffer, Camera camera)
        {
            ShadowData* data = buffer.Local + QueueIndex;
            data->Softness = ShadowMapLightBleedingReduction;

            Matrix4x4* views = ShadowData.GetViews(data);
            float* cascades = ShadowData.GetCascades(data);

            CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, shadowFrustra, ShadowMapSize, cascadeCount);
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer, ConstantBuffer<CSMShadowParams> csmConstantBuffer, Camera camera)
        {
            if (csmBuffer == null)
            {
                return;
            }
#nullable disable
            var data = buffer.Local + QueueIndex;

            Matrix4x4* views = ShadowData.GetViews(data);
            float* cascades = ShadowData.GetCascades(data);

            var matrices = CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, shadowFrustra, ShadowMapSize, cascadeCount);
            CSMShadowParams shadowParams = default;
            for (uint i = 0; i < cascadeCount - 1; i++)
            {
                shadowParams[i] = matrices[i];
            }
            shadowParams.CascadeCount = (uint)(cascadeCount - 1);

            context.Write(csmConstantBuffer.Buffer, shadowParams);

            context.ClearRenderTargetView(csmBuffer.RTV, Vector4.One);
            context.ClearDepthStencilView(csmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);
            context.SetRenderTarget(csmBuffer.RTV, csmDepthBuffer.DSV);
            context.SetViewport(csmBuffer.Viewport);

#nullable enable
        }

        public unsafe void BeginUpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer, ConstantBuffer<CSMShadowParams> csmConstantBuffer, Camera camera)
        {
            if (csmBuffer == null)
            {
                return;
            }
#nullable disable
            var data = buffer.Local + QueueIndex;

            Matrix4x4* views = ShadowData.GetViews(data);
            float* cascades = ShadowData.GetCascades(data);

            var matrices = CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, shadowFrustra, ShadowMapSize, cascadeCount);
            CSMShadowParams shadowParams = default;
            for (uint i = 0; i < cascadeCount - 1; i++)
            {
                shadowParams[i] = matrices[i];
            }
            shadowParams.CascadeCount = (uint)(cascadeCount - 1);

            context.Write(csmConstantBuffer.Buffer, shadowParams);

#nullable enable
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, int cascadeIndex, StructuredUavBuffer<ShadowData> buffer, ConstantBuffer<CSMShadowParams> csmConstantBuffer, Camera camera)
        {
            if (csmBuffer == null)
            {
                return;
            }
#nullable disable
            var data = buffer.Local + QueueIndex;

            Matrix4x4* views = ShadowData.GetViews(data);
            float* cascades = ShadowData.GetCascades(data);

            var matrices = CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, shadowFrustra, ShadowMapSize, cascadeCount);
            CSMShadowParams shadowParams = default;
            for (uint i = 0; i < cascadeCount - 1; i++)
            {
                shadowParams[i] = matrices[i];
            }
            shadowParams.CascadeCount = (uint)(cascadeCount - 1);

            context.Write(csmConstantBuffer.Buffer, shadowParams);
            context.ClearRenderTargetView(csmBuffer.RTVArraySlices[cascadeIndex], Vector4.One);
            context.ClearDepthStencilView(csmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);
            context.SetRenderTarget(csmBuffer.RTV, csmDepthBuffer.DSV);
            context.SetViewport(csmBuffer.Viewport);
#nullable enable
        }

        /// <inheritdoc/>
        public override bool IntersectFrustum(BoundingBox box)
        {
            return true;
        }

        public override bool UpdateShadowMapSize(Camera camera, ShadowAtlas atlas)
        {
            return false;
        }
    }
}