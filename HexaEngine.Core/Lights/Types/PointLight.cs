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
    using System.Numerics;

    [EditorNode<PointLight>("Point Light")]
    public class PointLight : Light
    {
        private static ulong instances;
        private static IGraphicsPipeline? osmPipeline;
        private static ConstantBuffer<Matrix4x4>? osmBuffer;
        private static IBuffer? osmParamBuffer;

        private DepthStencil? osmDepthBuffer;
        private float shadowRange = 100;
        private float strength = 1;
        private float falloff = 100;

        [JsonIgnore]
        public BoundingBox ShadowBox = new();

        [EditorProperty("Shadow Range")]
        public float ShadowRange { get => shadowRange; set => SetAndNotifyWithEqualsTest(ref shadowRange, value); }

        [EditorProperty("Strength")]
        public float Strength { get => strength; set => SetAndNotifyWithEqualsTest(ref strength, value); }

        [EditorProperty("Falloff")]
        public float Falloff { get => falloff; set => SetAndNotifyWithEqualsTest(ref falloff, value); }

        [JsonIgnore]
        public override LightType LightType => LightType.Point;

        [JsonIgnore]
        public override bool HasShadowMap => osmDepthBuffer != null;

        public override IShaderResourceView? GetShadowMap()
        {
            return osmDepthBuffer?.SRV;
        }

        public override void CreateShadowMap(IGraphicsDevice device)
        {
            if (osmDepthBuffer != null) return;
            osmDepthBuffer = new(device, 2048, 2048, 6, Format.D32Float, ResourceMiscFlag.TextureCube);

            if (Interlocked.Increment(ref instances) == 1)
            {
                osmBuffer = new(device, 6, CpuAccessFlags.Write);
                osmParamBuffer = device.CreateBuffer(new Vector4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                osmPipeline = device.CreateGraphicsPipeline(new()
                {
                    VertexShader = "forward/osm/vs.hlsl",
                    HullShader = "forward/osm/hs.hlsl",
                    DomainShader = "forward/osm/ds.hlsl",
                    GeometryShader = "forward/osm/gs.hlsl",
                    PixelShader = "forward/osm/ps.hlsl",
                },
                new GraphicsPipelineState()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.PatchListWith3ControlPoints,
                });
            }
        }

        public override void DestroyShadowMap()
        {
            if (osmDepthBuffer == null) return;
            osmDepthBuffer?.Dispose();
            osmDepthBuffer = null;
            if (Interlocked.Decrement(ref instances) == 0)
            {
                osmBuffer?.Dispose();
                osmParamBuffer?.Dispose();
                osmPipeline?.Dispose();
                osmBuffer = null;
                osmParamBuffer = null;
                osmBuffer = null;
            }
        }

        public unsafe void UpdateShadowMap(IGraphicsContext context, StructuredUavBuffer<ShadowPointLightData> buffer, IInstanceManager manager)
        {
            if (osmDepthBuffer == null) return;
#nullable disable

            OSMHelper.GetLightSpaceMatrices(Transform, ShadowRange, osmBuffer.Local, ref ShadowBox);
            osmBuffer.Update(context);
            context.Write(osmParamBuffer, new Vector4(Transform.GlobalPosition, ShadowRange));
            context.ClearDepthStencilView(osmDepthBuffer.DSV, DepthStencilClearFlags.All, 1, 0);
            context.SetRenderTarget(null, osmDepthBuffer.DSV);
            context.SetViewport(osmDepthBuffer.Viewport);
            context.SetGraphicsPipeline(osmPipeline);
            context.GSSetConstantBuffer(osmBuffer.Buffer, 0);
            context.PSSetConstantBuffer(osmParamBuffer, 0);

            var types = manager.Types;
            for (int j = 0; j < types.Count; j++)
            {
                var type = types[j];
                type.UpdateFrustumInstanceBuffer(ShadowBox);
                if (type.BeginDrawNoOcculusion(context))
                {
                    context.DrawIndexedInstanced((uint)type.IndexCount, (uint)type.Visible, 0, 0, 0);
                }
            }
            context.ClearState();
#nullable enable
        }

        public override unsafe bool IntersectFrustum(BoundingBox box)
        {
            return ShadowBox.Intersects(box);
        }
    }
}