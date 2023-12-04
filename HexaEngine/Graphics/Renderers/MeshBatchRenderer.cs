namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Batching;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class MeshBatchRenderer : BaseBatchRenderer<MeshBatchInstance>
    {
        private readonly ConstantBuffer<UPoint4> offsetBuffer;
        private readonly StructuredBuffer<Matrix4x4> transformBuffer;
        private readonly StructuredBuffer<uint> transformOffsetBuffer;

        public MeshBatchRenderer(IGraphicsDevice device)
        {
            transformBuffer = new(device, CpuAccessFlags.Write);
            transformOffsetBuffer = new(device, CpuAccessFlags.Write);
            offsetBuffer = new(device, CpuAccessFlags.Write);
        }

        public override void BeginUpdate(IGraphicsContext context)
        {
            transformOffsetBuffer.ResetCounter();
            transformBuffer.ResetCounter();
        }

        public override void Update(IGraphicsContext context, IBatch<MeshBatchInstance> batch)
        {
            batch[0].BufferOffset = transformOffsetBuffer.Count;

            for (int i = 0; i < batch.Count; i++)
            {
                var instance = batch[i];

                if (!instance.Visible)
                {
                    continue;
                }

                transformOffsetBuffer.Add(transformBuffer.Count);
                transformBuffer.Add(Matrix4x4.Transpose(instance.Transform));
            }
        }

        public override void EndUpdate(IGraphicsContext context)
        {
            transformOffsetBuffer.Update(context);
            transformBuffer.Update(context);
        }

        public override void DrawDeferred(IGraphicsContext context, IBatch<MeshBatchInstance> batch)
        {
            var instance = batch[0];

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            offsetBuffer.Update(context, new(instance.BufferOffset));

            Mesh mesh = instance.Mesh;
            Material material = instance.Material;

            mesh.BeginDraw(context);
            material.DrawDeferred(context, mesh.IndexCount, (uint)batch.Count);
            mesh.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawForward(IGraphicsContext context, IBatch<MeshBatchInstance> batch)
        {
            var instance = batch[0];

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            offsetBuffer.Update(context, new(instance.BufferOffset));

            Mesh mesh = instance.Mesh;
            Material material = instance.Material;

            mesh.BeginDraw(context);
            material.DrawForward(context, mesh.IndexCount, (uint)batch.Count);
            mesh.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawDepth(IGraphicsContext context, IBatch<MeshBatchInstance> batch)
        {
            var instance = batch[0];

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            offsetBuffer.Update(context, new(instance.BufferOffset));

            Mesh mesh = instance.Mesh;
            Material material = instance.Material;

            mesh.BeginDraw(context);
            material.DrawDepth(context, mesh.IndexCount, (uint)batch.Count);
            mesh.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawDepth(IGraphicsContext context, IBatch<MeshBatchInstance> batch, IBuffer camera)
        {
            var instance = batch[0];

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            offsetBuffer.Update(context, new(instance.BufferOffset));

            Mesh mesh = instance.Mesh;
            Material material = instance.Material;

            mesh.BeginDraw(context);
            material.DrawDepth(context, mesh.IndexCount, (uint)batch.Count);
            mesh.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawShadowMap(IGraphicsContext context, IBatch<MeshBatchInstance> batch, IBuffer light, ShadowType type)
        {
            var instance = batch[0];

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            offsetBuffer.Update(context, new(instance.BufferOffset));

            Mesh mesh = instance.Mesh;
            Material material = instance.Material;

            mesh.BeginDraw(context);
            material.DrawShadow(context, light, type, mesh.IndexCount, (uint)batch.Count);
            mesh.EndDraw(context);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void VisibilityTest(IGraphicsContext context, IBatch<MeshBatchInstance> batch, Camera camera)
        {
            for (int i = 0; i < batch.Count; i++)
            {
                var instance = batch[i];
                var aabb = BoundingBox.Transform(instance.Mesh.BoundingBox, instance.Transform);
                instance.Visible &= camera.Transform.Frustum.Intersects(aabb);
            }
        }

        protected override void DisposeCore()
        {
            transformBuffer.Dispose();
            transformOffsetBuffer.Dispose();
            offsetBuffer.Dispose();
        }
    }
}