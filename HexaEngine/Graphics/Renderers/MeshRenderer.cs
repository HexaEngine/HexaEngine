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
    using HexaEngine.Resources;
    using System.Numerics;

    public sealed class MeshRenderer : BaseRenderer<Model>
    {
#nullable disable
        private ConstantBuffer<UPoint4> offsetBuffer;
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private StructuredBuffer<Matrix4x4> transformNoBuffer;
        private StructuredBuffer<uint> transformNoOffsetBuffer;
        private StructuredUavBuffer<Matrix4x4> transformBuffer;
        private StructuredUavBuffer<uint> transformOffsetBuffer;
#nullable restore

        protected override void Initialize(IGraphicsDevice device, CullingContext cullingContext)
        {
            offsetBuffer = new(CpuAccessFlags.Write);
            drawIndirectArgs = cullingContext.DrawIndirectArgs;
            transformNoBuffer = cullingContext.InstanceDataNoCull;
            transformNoOffsetBuffer = cullingContext.InstanceOffsetsNoCull;
            transformBuffer = cullingContext.InstanceDataOutBuffer;
            transformOffsetBuffer = cullingContext.InstanceOffsets;
        }

        public override void Update(IGraphicsContext context, Matrix4x4 transform, Model model)
        {
            DrawType[] drawTypes = model.DrawTypes;
            Matrix4x4[] globals = model.Globals;
            Matrix4x4[] locals = model.Locals;
            Core.IO.Binary.Meshes.PlainNode[] plainNodes = model.PlainNodes;
            Mesh[] meshes = model.Meshes;

            globals[0] = locals[0] * transform;

            for (int i = 1; i < plainNodes.Length; i++)
            {
                Core.IO.Binary.Meshes.PlainNode node = plainNodes[i];
                globals[i] = locals[node.Id] * globals[node.ParentId];
            }

            for (int i = 0; i < drawTypes.Length; i++)
            {
                DrawType drawType = drawTypes[i];
                BoundingSphere sphere = meshes[drawType.MeshId]?.BoundingSphere ?? default;
                for (int j = 0; j < drawType.Instances.Count; j++)
                {
                    DrawInstance instance = drawType.Instances[j];
                    Matrix4x4 global = globals[instance.NodeId];
                    instance.BoundingSphere = sphere;
                    instance.Transform = Matrix4x4.Transpose(global);
                }
            }
        }

        public override void VisibilityTest(CullingContext context, Model model)
        {
            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;

            for (int i = 0; i < drawTypes.Length; i++)
            {
                DrawType drawType = drawTypes[i];
                Mesh mesh = meshes[drawType.MeshId];
                if (mesh == null)
                {
                    continue;
                }

                context.AppendType(mesh.IndexCount);
                drawType.TypeId = context.CurrentType;
                drawType.DrawIndirectOffset = context.GetDrawArgsOffset();
                for (int j = 0; j < drawType.Instances.Count; j++)
                {
                    DrawInstance instance = drawType.Instances[j];
                    context.AppendInstance(instance.Transform, instance.BoundingSphere);
                }
            }
        }

        public override void DrawDeferred(IGraphicsContext context, Model model)
        {
            if (drawIndirectArgs == null)
            {
                return;
            }

            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer?.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer?.SRV);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                DrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstancedIndirect(context, "Deferred", drawIndirectArgs, drawType.DrawIndirectOffset);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawForward(IGraphicsContext context, Model model)
        {
            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer?.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer?.SRV);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                DrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstancedIndirect(context, "Forward", drawIndirectArgs, drawType.DrawIndirectOffset);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawDepth(IGraphicsContext context, Model model)
        {
            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer?.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer?.SRV);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                DrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, "DepthOnly", mesh.IndexCount, (uint)drawType.Instances.Count);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawDepth(IGraphicsContext context, Model model, IBuffer camera)
        {
            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer?.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer?.SRV);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                DrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, "DepthOnly", mesh.IndexCount, (uint)drawType.Instances.Count);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawShadowMap(IGraphicsContext context, Model model, IBuffer light, ShadowType type)
        {
            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

            string name = EnumHelper<ShadowType>.GetName(type);

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer.SRV);
            context.VSSetConstantBuffer(1, light);
            context.GSSetConstantBuffer(0, light);
            context.PSSetConstantBuffer(0, light);

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                DrawType drawType = drawTypes[i];
                offsetBuffer.Update(context, new(drawType.TypeId));

                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, name, mesh.IndexCount, (uint)drawType.Instances.Count);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(1, null);
            context.GSSetConstantBuffer(0, null);
            context.PSSetConstantBuffer(0, null);
            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void Bake(IGraphicsContext context, Model model)
        {
        }

        protected override void DisposeCore()
        {
            offsetBuffer.Dispose();
            offsetBuffer = null;
            drawIndirectArgs = null;
            transformNoBuffer = null;
            transformNoOffsetBuffer = null;
            transformBuffer = null;
            transformOffsetBuffer = null;
        }
    }
}