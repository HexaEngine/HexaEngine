namespace HexaEngine.Core.Lights.Types
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Instances;
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
        private static IGraphicsPipeline? csmPipeline;
        private static ConstantBuffer<Matrix4x4>? csmCB;

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
            if (csmDepthBuffer != null) return;
            csmDepthBuffer = new(device, 4096, 4096, 3, Format.D32Float);
            if (Interlocked.Increment(ref instances) == 1)
            {
                csmCB = new(device, 16, CpuAccessFlags.Write);
                csmPipeline = device.CreateGraphicsPipeline(new()
                {
                    VertexShader = "forward/csm/vs.hlsl",
                    HullShader = "forward/csm/hs.hlsl",
                    DomainShader = "forward/csm/ds.hlsl",
                    GeometryShader = "forward/csm/gs.hlsl",
                }, new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.PatchListWith3ControlPoints,
                });
            }
        }

        public override void DestroyShadowMap()
        {
            if (csmDepthBuffer == null) return;
            csmDepthBuffer?.Dispose();
            csmDepthBuffer = null;

            if (Interlocked.Decrement(ref instances) == 0)
            {
                csmCB?.Dispose();
                csmPipeline?.Dispose();
                csmCB = null;
                csmPipeline = null;
            }
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowDirectionalLightData> buffer, Camera camera, IInstanceManager manager)
        {
            if (csmDepthBuffer == null) return;
#nullable disable
            var data = buffer.Local + QueueIndex;

            data->Color = color;
            data->Direction = Transform.Forward;
            Matrix4x4* views = data->GetViews();
            float* cascades = data->GetCascades();

            var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, Transform, views, cascades, ShadowFrustra);
            context.Write(csmCB.Buffer, mtxs, sizeof(Matrix4x4) * 16);

            context.ClearDepthStencilView(csmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);
            context.SetRenderTarget(null, csmDepthBuffer.DSV);
            context.SetViewport(csmDepthBuffer.Viewport);
            context.SetGraphicsPipeline(csmPipeline);
            context.GSSetConstantBuffer(csmCB.Buffer, 0);
            var types = manager.Types;
            for (int j = 0; j < types.Count; j++)
            {
                var type = types[j];
                type.UpdateFrustumInstanceBuffer(ShadowFrustra);
                if (type.BeginDrawNoOcculusion(context))
                {
                    context.DrawIndexedInstanced((uint)type.IndexCount, (uint)type.Visible, 0, 0, 0);
                }
            }
            context.ClearState();
#nullable enable
        }

        public override bool IntersectFrustum(BoundingBox box)
        {
            return true;
        }
    }
}