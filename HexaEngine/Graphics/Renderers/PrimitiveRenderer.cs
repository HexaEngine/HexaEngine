namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Objects;
    using HexaEngine.Resources;
    using System.Numerics;

    public class PrimitiveRenderer : BaseRenderer<PrimitiveModel>
    {
#nullable disable
        private ConstantBuffer<UPoint4> offsetBuffer;
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private StructuredBuffer<Matrix4x4> transformNoBuffer;
        private StructuredBuffer<uint> transformNoOffsetBuffer;
        private StructuredUavBuffer<Matrix4x4> transformBuffer;
        private StructuredUavBuffer<uint> transformOffsetBuffer;
#nullable restore

        public PrimitiveRenderer()
        {
        }

        protected override void Initialize(IGraphicsDevice device, CullingContext cullingContext)
        {
            offsetBuffer = new(CpuAccessFlags.Write);
            drawIndirectArgs = cullingContext.DrawIndirectArgs;
            transformNoBuffer = cullingContext.InstanceDataNoCull;
            transformNoOffsetBuffer = cullingContext.InstanceOffsetsNoCull;
            transformBuffer = cullingContext.InstanceDataOutBuffer;
            transformOffsetBuffer = cullingContext.InstanceOffsets;
        }

        public void Prepare(Material material)
        {
            for (int i = 0; i < material.Shader.Passes.Count; i++)
            {
                MaterialShaderPass pass = material.Shader.Passes[i];
                var bindings = pass.Bindings;
                bindings.SetCBV("offsetBuffer", offsetBuffer);
                if (pass.Name == "Deferred" || pass.Name == "Forward")
                {
                    bindings.SetSRV("worldMatrices", transformBuffer.SRV!);
                    bindings.SetSRV("worldMatrixOffsets", transformOffsetBuffer.SRV!);
                }
                else
                {
                    bindings.SetSRV("worldMatrices", transformNoBuffer.SRV);
                    bindings.SetSRV("worldMatrixOffsets", transformNoOffsetBuffer.SRV);
                }
            }
        }

        public override void Update(IGraphicsContext context, Matrix4x4 transform, PrimitiveModel model)
        {
            DrawType drawType = model.DrawType;
            Matrix4x4[] transforms = model.InstanceTransforms;

            BoundingSphere sphere = BoundingSphere.CreateFromBoundingBox(model.BoundingBox);
            for (int j = 0; j < drawType.Instances.Count; j++)
            {
                DrawInstance instance = drawType.Instances[j];
                Matrix4x4 global = transforms[instance.NodeId] * transform;
                instance.BoundingSphere = sphere;
                instance.Transform = Matrix4x4.Transpose(global);
            }
        }

        public override void VisibilityTest(CullingContext context, PrimitiveModel model)
        {
            DrawType drawType = model.DrawType;

            context.AppendType(model.Prim.IndexCount);
            drawType.TypeId = context.CurrentType;
            drawType.DrawIndirectOffset = context.GetDrawArgsOffset();
            for (int j = 0; j < drawType.Instances.Count; j++)
            {
                DrawInstance instance = drawType.Instances[j];
                context.AppendInstance(instance.Transform, instance.BoundingSphere);
            }
        }

        public override void DrawDeferred(IGraphicsContext context, PrimitiveModel model)
        {
            var primitive = model.Prim;
            var material = model.Material;
            var drawType = model.DrawType;

            offsetBuffer.Update(context, new(drawType.TypeId));

            Prepare(material);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawIndexedInstancedIndirect(context, "Deferred", drawIndirectArgs, drawType.DrawIndirectOffset);
            primitive.EndDraw(context);
        }

        public override void DrawForward(IGraphicsContext context, PrimitiveModel model)
        {
            var primitive = model.Prim;
            var material = model.Material;
            var drawType = model.DrawType;

            offsetBuffer.Update(context, new(drawType.TypeId));

            Prepare(material);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawIndexedInstancedIndirect(context, "Forward", drawIndirectArgs, drawType.DrawIndirectOffset);
            primitive.EndDraw(context);
        }

        public override void DrawDepth(IGraphicsContext context, PrimitiveModel model)
        {
            var primitive = model.Prim;
            var material = model.Material;
            var drawType = model.DrawType;

            offsetBuffer.Update(context, new(drawType.TypeId));
            Prepare(material);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawIndexedInstanced(context, "DepthOnly", indexCount, 1);
            primitive.EndDraw(context);
        }

        public override void DrawDepth(IGraphicsContext context, PrimitiveModel model, IBuffer camera)
        {
            var primitive = model.Prim;
            var material = model.Material;
            var drawType = model.DrawType;

            offsetBuffer.Update(context, new(drawType.TypeId));
            Prepare(material);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            material.DrawIndexedInstanced(context, "DepthOnly", indexCount, 1);
            primitive.EndDraw(context);
        }

        public override void DrawShadowMap(IGraphicsContext context, PrimitiveModel model, IBuffer light, ShadowType type)
        {
            var primitive = model.Prim;
            var material = model.Material;
            var drawType = model.DrawType;

            offsetBuffer.Update(context, new(drawType.TypeId));
            Prepare(material);

            string name = EnumHelper<ShadowType>.GetName(type);

            if (!material.BeginDraw(name, out var pass))
            {
                return;
            }

            pass.Bindings.SetCBV("lightBuffer", light);

            primitive.BeginDraw(context, out _, out var indexCount, out _);
            if (pass.BeginDraw(context))
            {
                context.DrawIndexedInstanced(indexCount, 1, 0, 0, 0);
            }
            material.EndDraw(context);
            primitive.EndDraw(context);
        }

        protected override void DisposeCore()
        {
            transformBuffer.Dispose();
            transformOffsetBuffer.Dispose();
            offsetBuffer.Dispose();
        }

        public override void Bake(IGraphicsContext context, PrimitiveModel instance)
        {
            throw new NotSupportedException();
        }
    }
}