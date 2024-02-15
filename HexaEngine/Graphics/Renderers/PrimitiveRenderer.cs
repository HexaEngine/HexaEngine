namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class PrimitiveRenderer : IRenderer, IDisposable
    {
        private readonly ConstantBuffer<UPoint4> offsetBuffer;
        private readonly StructuredBuffer<Matrix4x4> transformBuffer;
        private readonly StructuredBuffer<uint> transformOffsetBuffer;
        private Material material;
        private IPrimitive primitive;
        private BoundingBox boundingBox;
        private bool initialized;

        private bool disposedValue;

        public PrimitiveRenderer(IGraphicsDevice device)
        {
            transformBuffer = new(device, CpuAccessFlags.Write);
            transformOffsetBuffer = new(device, CpuAccessFlags.Write);
            offsetBuffer = new(device, CpuAccessFlags.Write);
        }

        public void Initialize(Primitive<MeshVertex, ushort> primitive, Material material)
        {
            this.primitive = primitive;
            this.material = material;
            initialized = true;
        }

        public void Initialize(Primitive<MeshVertex, uint> primitive, Material material)
        {
            this.primitive = primitive;
            this.material = material;
            initialized = true;
        }

        public void Uninitialize()
        {
            primitive = null;
            material = null;
            initialized = false;
        }

        public void Update(IGraphicsContext context, Matrix4x4 transform)
        {
            if (!initialized)
                return;

            transformOffsetBuffer.ResetCounter();
            transformBuffer.ResetCounter();

            transformBuffer.Add(Matrix4x4.Transpose(transform));
            transformOffsetBuffer.Add(0);

            boundingBox = BoundingBox.Transform(primitive.BoundingBox, transform);

            transformOffsetBuffer.Update(context);
            transformBuffer.Update(context);
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
            if (!initialized)
                return;
        }

        public void DrawDeferred(IGraphicsContext context)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawDeferred(context, indexCount, 1);
            primitive.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawForward(IGraphicsContext context)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawForward(context, indexCount, 1);
            primitive.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawDepth(context, indexCount, 1);
            primitive.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawDepth(context, indexCount, 1);
            primitive.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawShadow(context, light, type, indexCount, 1);
            primitive.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                transformBuffer.Dispose();
                transformOffsetBuffer.Dispose();

                offsetBuffer.Dispose();
                Uninitialize();
                disposedValue = true;
            }
        }

        ~PrimitiveRenderer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}