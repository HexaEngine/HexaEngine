namespace HexaEngine.Core.Lights.Types
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Structs;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;
    using System.Numerics;

    [EditorNode<DirectionalLight>("Directional Light")]
    public class DirectionalLight : Light
    {
        private static ulong instances;
        private static ConstantBuffer<Matrix4x4>? csmBuffer;

        private DepthStencil? csmDepthBuffer;
        public new CameraTransform Transform = new();

        [JsonIgnore]
        public BoundingFrustum[] ShadowFrustra = new BoundingFrustum[16];

        public DirectionalLight()
        {
            for (int i = 0; i < 16; i++)
            {
                ShadowFrustra[i] = new();
            }
            base.Transform = Transform;
            OverwriteTransform(Transform);
        }

        [JsonIgnore]
        public override LightType LightType => LightType.Directional;

        [JsonIgnore]
        public override bool HasShadowMap => csmDepthBuffer != null;

        public override IShaderResourceView? GetShadowMap()
        {
            return csmDepthBuffer?.SRV;
        }

        public override void CreateShadowMap(IGraphicsDevice device)
        {
            if (csmDepthBuffer != null)
            {
                return;
            }

            csmDepthBuffer = new(device, 4096, 4096, 3, Format.D32Float);
            if (Interlocked.Increment(ref instances) == 1)
            {
                csmBuffer = new(device, 16, CpuAccessFlags.Write);
            }
        }

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

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowDirectionalLightData> buffer, Camera camera)
        {
            if (csmDepthBuffer == null)
            {
                return;
            }
#nullable disable
            var data = buffer.Local + QueueIndex;

            data->Data.Color = color;
            data->Data.Direction = Transform.Forward;
            Matrix4x4* views = data->GetViews();
            float* cascades = data->GetCascades();

            var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, ShadowFrustra);
            context.Write(csmBuffer.Buffer, mtxs, sizeof(Matrix4x4) * 16);

            context.ClearDepthStencilView(csmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);
            context.SetRenderTarget(null, csmDepthBuffer.DSV);
            context.SetViewport(csmDepthBuffer.Viewport);
#nullable enable
        }

        public override bool IntersectFrustum(BoundingBox box)
        {
            return true;
        }
    }
}