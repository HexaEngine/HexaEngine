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
        private ulong dirtyFrame;
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
            CullingManager.Current.BuffersResized += TransformBufferResize;
        }

        private void TransformBufferResize(object? sender, CapacityChangedEventArgs e)
        {
            dirtyFrame = Time.Frame;
        }

        public override void Prepare(Model instance)
        {
            foreach (Material material in instance.Materials)
            {
                Prepare(material);
            }
        }

        public void Prepare(Material material)
        {
            if (!material.BeginUse())
            {
                return;
            }

            foreach (var pass in material.Shader.Passes)
            {
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
            material.EndUse();
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

            Prepare(model);

            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

            for (uint i = 0; i < drawTypes.Length; i++)
            {
                DrawType drawType = drawTypes[i];
                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                offsetBuffer.Update(context, new(drawType.TypeId));

                mesh.BeginDraw(context);
                material.DrawIndexedInstancedIndirect(context, "Deferred", drawIndirectArgs, drawType.DrawIndirectOffset);
                mesh.EndDraw(context);
            }
        }

        public override void DrawForward(IGraphicsContext context, Model model)
        {
            Prepare(model);

            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

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
        }

        public override void DrawDepth(IGraphicsContext context, Model model)
        {
            Prepare(model);

            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

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
        }

        public override void DrawDepth(IGraphicsContext context, Model model, IBuffer camera)
        {
            Prepare(model);

            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

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
        }

        public override void DrawShadowMap(IGraphicsContext context, Model model, IBuffer light, ShadowType type)
        {
            Prepare(model);

            DrawType[] drawTypes = model.DrawTypes;
            Mesh[] meshes = model.Meshes;
            Material[] materials = model.Materials;

            string name = EnumHelper<ShadowType>.GetName(type);

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

                if (!material.BeginDraw(name, out var pass))
                {
                    continue;
                }

                pass.Bindings.SetCBV("lightBuffer", light);

                mesh.BeginDraw(context);
                if (pass.BeginDraw(context))
                {
                    context.DrawIndexedInstanced(mesh.IndexCount, (uint)drawType.Instances.Count, 0, 0, 0);
                }
                material.EndDraw(context);
                mesh.EndDraw(context);
            }
        }

        public override void Bake(IGraphicsContext context, Model model)
        {
        }

        protected override void DisposeCore()
        {
            transformBuffer.Resize -= TransformBufferResize;
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