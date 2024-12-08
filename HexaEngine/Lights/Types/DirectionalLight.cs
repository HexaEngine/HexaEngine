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

        public DepthStencil? GetDepthStencil() => csmDepthBuffer;

        /// <inheritdoc/>
        protected override void OnCreateShadowMap(ShadowAtlas atlas)
        {
            if (csmBuffer != null)
            {
                return;
            }

            csmBuffer = new(GraphicsSettings.ShadowMapFormat, ShadowMapSize, ShadowMapSize, cascadeCount - 1, 1, CpuAccessFlags.None, GpuAccessFlags.All);
            csmBuffer.CreateArraySlices();
            csmDepthBuffer = new(Format.D32Float, ShadowMapSize, ShadowMapSize, cascadeCount - 1);
        }

        /// <inheritdoc/>
        protected override void OnDestroyShadowMap()
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

        private Vector3 camOldPos;
        private Vector3 camOldRot;

        private uint dirtyCascades;
        private ShadowData shadowDataLast;

        public unsafe bool UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowData> buffer, ConstantBuffer<CSMShadowParams> csmConstantBuffer, Camera camera, out uint updateMask, out bool reproject)
        {
            if (csmBuffer == null)
            {
                updateMask = 0;
                reproject = false;
                return false;
            }

            var camPos = camera.Transform.GlobalPosition;
            var camRot = camera.Transform.GlobalOrientation.ToYawPitchRoll();

            var camPosDelta = camPos - camOldPos;
            var camRotDelta = camRot - camOldRot;

            camOldPos = camPos;
            camOldRot = camRot;

            const float motionEpsilon = 0.00001f;
            // Determine if we need to update based on camera movement
            bool positionChanged = camPosDelta.LengthSquared() > motionEpsilon;
            bool rotationChanged = camRotDelta.LengthSquared() > motionEpsilon;

            // Check if we need to update the cascade shadow maps
            if (!positionChanged && !rotationChanged)
            {
                reproject = false;
                if (dirtyCascades == 0)
                {
                    updateMask = 0;
                    return false; // No significant changes, skip update
                }
            }
            else
            {
                dirtyCascades = (1u << (cascadeCount - 1)) - 1;  // set cascadeCount - 1 bits only
                reproject = true; // signal caller to reproject/reuse depth values.
            }

            var frame = Time.Frame;
            updateMask = 0;

            for (int i = 0; i < cascadeCount - 1; i++)
            {
                var frequency = 1u << i; // equivalent to pow(2, i), this might get changed.
                var flag = 1u << i;
                if (frame % frequency == 0 && (dirtyCascades & flag) != 0)
                {
                    updateMask |= flag;
                    dirtyCascades &= ~flag; // clear dirty flag.
                }
            }

            ShadowData* data = buffer.Local + QueueIndex;

            Matrix4x4* views = ShadowData.GetViews(data);
            float* cascades = ShadowData.GetCascades(data);

            CSMShadowParams shadowParams = default;

            if (reproject) // only update matrices if needed if not use the last, because updating everytime would cause numerical instability and performance penalties.
            {
                var matrices = CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, shadowFrustra, ShadowMapSize, cascadeCount);
                MemcpyT(matrices, &shadowParams.View0, cascadeCount - 1);
                shadowDataLast = *data;
            }
            else
            {
                *data = shadowDataLast;
                MemcpyT(views, &shadowParams.View0, cascadeCount - 1);
            }

            shadowParams.CascadeCount = (uint)(cascadeCount - 1);
            shadowParams.ActiveCascades = updateMask;

            *csmConstantBuffer.Local = shadowParams;
            csmConstantBuffer.Update(context);

            return true;
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