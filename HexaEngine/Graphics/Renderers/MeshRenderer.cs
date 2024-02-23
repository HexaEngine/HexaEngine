namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using System.Numerics;

    public class MeshRenderer : IDisposable, IRenderer
    {
        private bool initialized;

        private Matrix4x4[] globals;
        private Matrix4x4[] locals;
        private PlainNode[] plainNodes;
        private readonly ConstantBuffer<UPoint4> offsetBuffer;
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private StructuredBuffer<Matrix4x4> transformNoBuffer;
        private StructuredBuffer<uint> transformNoOffsetBuffer;
        private StructuredUavBuffer<Matrix4x4> transformBuffer;
        private StructuredUavBuffer<uint> transformOffsetBuffer;
        private MeshDrawType[] drawTypes;

        private Mesh[] meshes;
        private Material[] materials;

        private bool disposedValue;

        public MeshRenderer(IGraphicsDevice device)
        {
            offsetBuffer = new(device, CpuAccessFlags.Write);
        }

        public void Initialize(Model model)
        {
            globals = model.Globals;
            locals = model.Locals;
            plainNodes = model.PlainNodes;
            drawTypes = model.DrawTypes;
            meshes = model.Meshes;
            materials = model.Materials;
            initialized = true;
        }

        public void Uninitialize()
        {
            initialized = false;
            globals = null;
            locals = null;
            plainNodes = null;
            drawTypes = null;
            meshes = null;
            materials = null;
        }

        public void Update(IGraphicsContext context, Matrix4x4 transform)
        {
            if (!initialized)
                return;

            globals[0] = locals[0] * transform;

            for (int i = 1; i < plainNodes.Length; i++)
            {
                var node = plainNodes[i];
                globals[i] = locals[node.Id] * globals[node.ParentId];
            }

            for (int i = 0; i < drawTypes.Length; i++)
            {
                MeshDrawType drawType = drawTypes[i];
                BoundingBox mesh = meshes[drawType.MeshId].BoundingBox;
                for (int j = 0; j < drawType.Instances.Length; j++)
                {
                    MeshDrawInstance instance = drawType.Instances[j];
                    Matrix4x4 global = globals[instance.NodeId];
                    instance.BoundingBox = mesh;
                    instance.Transform = Matrix4x4.Transpose(global);
                    drawType.Instances[j] = instance;
                }
            }
        }

        public void VisibilityTest(CullingContext context)
        {
            if (!initialized)
                return;

            drawIndirectArgs = context.DrawIndirectArgs;
            transformNoBuffer = context.InstanceDataNoCull;
            transformNoOffsetBuffer = context.InstanceOffsetsNoCull;
            transformBuffer = context.InstanceDataOutBuffer;
            transformOffsetBuffer = context.InstanceOffsets;

            for (int i = 0; i < drawTypes.Length; i++)
            {
                MeshDrawType drawType = drawTypes[i];
                Mesh mesh = meshes[drawType.MeshId];
                context.AppendType(mesh.IndexCount);
                drawTypes[i].TypeId = context.CurrentType;
                drawTypes[i].DrawIndirectOffset = context.GetDrawArgsOffset();
                for (int j = 0; j < drawType.Instances.Length; j++)
                {
                    MeshDrawInstance instance = drawType.Instances[j];
                    context.AppendInstance(instance.Transform, instance.BoundingBox);
                }
            }
        }

        public void DrawDeferred(IGraphicsContext context)
        {
            if (!initialized || drawIndirectArgs == null)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer?.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer?.SRV);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                MeshDrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawIndexedInstancedIndirect(context, "Deferred", drawIndirectArgs, drawType.DrawIndirectOffset);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawForward(IGraphicsContext context)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer?.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer?.SRV);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                MeshDrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawIndexedInstancedIndirect(context, "Forward", drawIndirectArgs, drawType.DrawIndirectOffset);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer?.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer?.SRV);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                MeshDrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, "DepthOnly", mesh.IndexCount, (uint)drawType.Instances.Length);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer?.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer?.SRV);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                MeshDrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, "DepthOnly", mesh.IndexCount, (uint)drawType.Instances.Length);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer.SRV);
            context.VSSetConstantBuffer(1, light);
            context.GSSetConstantBuffer(0, light);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                MeshDrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, type.ToString(), mesh.IndexCount, (uint)drawType.Instances.Length);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(1, null);
            context.GSSetConstantBuffer(0, null);
            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public void Bake(IGraphicsContext context)
        {
            if (!initialized)
                return;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                offsetBuffer.Dispose();
                Uninitialize();
                disposedValue = true;
            }
        }

        ~MeshRenderer()
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