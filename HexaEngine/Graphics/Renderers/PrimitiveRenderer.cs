namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using System.Numerics;

    public class PrimitiveRenderer : IRenderer, IDisposable
    {
        private ulong dirtyFrame;

        private readonly ConstantBuffer<UPoint4> offsetBuffer;
        private readonly StructuredBuffer<Matrix4x4> transformBuffer;
        private readonly StructuredBuffer<uint> transformOffsetBuffer;
        private Material material;
        private IPrimitive primitive;
        private BoundingBox boundingBox;
        private bool initialized;

        private bool disposedValue;

        public PrimitiveRenderer()
        {
            transformBuffer = new(CpuAccessFlags.Write);
            transformOffsetBuffer = new(CpuAccessFlags.Write);
            offsetBuffer = new(CpuAccessFlags.Write);
        }

        public void Initialize(Primitive<MeshVertex, ushort> primitive, Material material)
        {
            this.primitive = primitive;
            this.material = material;

            transformBuffer.Resize += TransformBufferResize;

            initialized = true;
        }

        public void Initialize(Primitive<MeshVertex, uint> primitive, Material material)
        {
            this.primitive = primitive;
            this.material = material;

            transformBuffer.Resize += TransformBufferResize;

            initialized = true;
        }

        private void TransformBufferResize(object? sender, CapacityChangedEventArgs e)
        {
            dirtyFrame = Time.Frame;
        }

        public void Uninitialize()
        {
            primitive = null;
            material = null;
            initialized = false;
        }

        public void Prepare(Material material)
        {
            foreach (var pass in material.Shader.Passes)
            {
                var bindings = pass.Bindings;
                bindings.SetCBV("CameraBuffer", GraphResourceBuilder.Global.GetConstantBuffer<CBCamera>("CBCamera").Value!);
                bindings.SetCBV("offsetBuffer", offsetBuffer);
                if (pass.Name == "Deferred" || pass.Name == "Forward")
                {
                    bindings.SetSRV("worldMatrices", transformBuffer.SRV!);
                    bindings.SetSRV("worldMatrixOffsets", transformOffsetBuffer.SRV!);
                }
                else
                {
                    bindings.SetSRV("worldMatrices", transformBuffer.SRV!);
                    bindings.SetSRV("worldMatrixOffsets", transformOffsetBuffer.SRV!);
                    //bindings.SetSRV("worldMatrices", transformNoBuffer.SRV);
                    //bindings.SetSRV("worldMatrixOffsets", transformNoOffsetBuffer.SRV);
                }
            }
        }

        public void Update(IGraphicsContext context, Matrix4x4 transform)
        {
            if (!initialized)
            {
                return;
            }

            transformOffsetBuffer.ResetCounter();
            transformBuffer.ResetCounter();

            transformBuffer.Add(Matrix4x4.Transpose(transform));
            transformOffsetBuffer.Add(0);

            boundingBox = BoundingBox.Transform(BoundingBox.Empty, transform);

            transformOffsetBuffer.Update(context);
            transformBuffer.Update(context);
        }

        public void VisibilityTest(CullingContext context)
        {
            if (!initialized)
            {
                return;
            }
        }

        public void DrawDeferred(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            Prepare(material);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawIndexedInstanced(context, "Deferred", indexCount, 1);
            primitive.EndDraw(context);
        }

        public void DrawForward(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            Prepare(material);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawIndexedInstanced(context, "Forward", indexCount, 1);
            primitive.EndDraw(context);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            Prepare(material);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawIndexedInstanced(context, "DepthOnly", indexCount, 1);
            primitive.EndDraw(context);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
            {
                return;
            }

            Prepare(material);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawIndexedInstanced(context, "DepthOnly", indexCount, 1);
            primitive.EndDraw(context);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
            {
                return;
            }

            Prepare(material);

            string name = EnumHelper<ShadowType>.GetName(type);

            var pass = material.GetPass(name);

            if (pass == null)
            {
                return;
            }

            pass.Bindings.SetCBV("lightBuffer", light);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            if (pass.BeginDraw(context))
            {
                context.DrawIndexedInstanced(indexCount, 1, 0, 0, 0);
            }
            pass.EndDraw(context);
            primitive.EndDraw(context);
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}