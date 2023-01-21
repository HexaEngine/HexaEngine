namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.Pipelines.Forward;
    using Newtonsoft.Json;
    using System.Numerics;
    using Texture = Graphics.Texture;

    [EditorNode<PointLight>("Point Light")]
    public class PointLight : Light
    {
        private static ulong instances;
        private static OSMPipeline? osmPipeline;
        private static ConstantBuffer<Matrix4x4>? osmBuffer;
        private static IBuffer? osmParamBuffer;
        private Texture? osmDepthBuffer;
        private float shadowRange = 100;
        private float strength = 1;
        private float falloff = 100;

        [JsonIgnore]
        public unsafe BoundingBox* ShadowBox;

        [EditorProperty("Shadow Range")]
        public float ShadowRange { get => shadowRange; set => SetAndNotifyWithEqualsTest(ref shadowRange, value); }

        [EditorProperty("Strength")]
        public float Strength { get => strength; set => SetAndNotifyWithEqualsTest(ref strength, value); }

        [EditorProperty("Falloff")]
        public float Falloff { get => falloff; set => SetAndNotifyWithEqualsTest(ref falloff, value); }

        [JsonIgnore]
        public override LightType LightType => LightType.Point;

        public override IShaderResourceView? GetShadowMap()
        {
            return osmDepthBuffer?.ShaderResourceView;
        }

        public override void CreateShadowMap(IGraphicsDevice device)
        {
            if (osmDepthBuffer != null) return;
            osmDepthBuffer = new(device, TextureDescription.CreateTextureCubeWithRTV(2048, 1, Format.R32Float), DepthStencilDesc.Default);

            if (Interlocked.Increment(ref instances) == 1)
            {
                osmBuffer = new(device, 6, CpuAccessFlags.Write);
                osmParamBuffer = device.CreateBuffer(new Vector4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                osmPipeline = new(device);
                osmPipeline.View = osmBuffer;
                osmPipeline.Light = osmParamBuffer;
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
            OSMHelper.GetLightSpaceMatrices(Transform, ShadowRange, osmBuffer.Local, ShadowBox);
            osmBuffer.Update(context);
            context.Write(osmParamBuffer, new Vector4(Transform.GlobalPosition, ShadowRange));

            osmDepthBuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
            context.SetRenderTarget(osmDepthBuffer.RenderTargetView, osmDepthBuffer.DepthStencilView);
            osmPipeline.BeginDraw(context, osmDepthBuffer.Viewport);

            var types = manager.Types;
            for (int j = 0; j < types.Count; j++)
            {
                var type = types[j];
                type.UpdateFrustumInstanceBuffer(*ShadowBox);
                if (type.BeginDrawNoOcculusion(context))
                {
                    context.DrawIndexedInstanced((uint)type.IndexCount, (uint)type.Visible, 0, 0, 0);
                }
            }
            context.ClearState();
#nullable enable
        }

        public override unsafe void Initialize(IGraphicsDevice device)
        {
            Updated = true;
            ShadowBox = Alloc<BoundingBox>();
            base.Initialize(device);
        }

        public override unsafe bool IntersectFrustum(BoundingBox box)
        {
            return ShadowBox->Intersects(box);
        }

        public override unsafe void Uninitialize()
        {
            base.Uninitialize();
            Free(ShadowBox);
        }
    }
}