namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using System.Numerics;

    public class TerrainRenderer : BaseRenderer<TerrainGrid>
    {
        private ulong dirtyFrame;
#nullable disable
        private ISamplerState linearClampSampler;
        private ConstantBuffer<UPoint4> offsetBuffer;
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private StructuredBuffer<Matrix4x4> transformNoBuffer;
        private StructuredBuffer<uint> transformNoOffsetBuffer;
        private StructuredUavBuffer<Matrix4x4> transformBuffer;
        private StructuredUavBuffer<uint> transformOffsetBuffer;
#nullable restore

        protected override void Initialize(IGraphicsDevice device, CullingContext cullingContext)
        {
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
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

        public override void Prepare(TerrainGrid instance)
        {
            if (dirtyFrame == 0 || dirtyFrame == Time.Frame - 1)
            {
                foreach (var cell in instance)
                {
                    foreach (var layer in cell.DrawLayers)
                    {
                        Prepare(layer.Material);
                    }
                }
            }
        }

        public void Prepare(TerrainMaterial material)
        {
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
        }

        public override void Update(IGraphicsContext context, Matrix4x4 transform, TerrainGrid instance)
        {
            for (int i = 0; i < instance.Count; i++)
            {
                TerrainCell cell = instance[i];
                Matrix4x4 global = transform * Matrix4x4.CreateTranslation(cell.Offset);
                Matrix4x4.Invert(global, out cell.TransformInv);
                cell.Transform = global;
                cell.BoundingSphere = cell.Mesh.BoundingSphere;
            }
        }

        public override void VisibilityTest(CullingContext context, TerrainGrid instance)
        {
            for (int i = 0; i < instance.Count; i++)
            {
                TerrainCell cell = instance[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                Mesh mesh = cell.Mesh;

                context.AppendType(mesh.IndexCount);
                cell.TypeId = context.CurrentType;
                cell.DrawIndirectOffset = context.GetDrawArgsOffset();
                context.AppendInstance(Matrix4x4.Transpose(cell.Transform), cell.BoundingSphere);
            }
        }

        public override void Bake(IGraphicsContext context, TerrainGrid instance)
        {
            throw new NotSupportedException();
        }

        public override void DrawDepth(IGraphicsContext context, TerrainGrid instance)
        {
            Prepare(instance);
            for (int i = 0; i < instance.Count; i++)
            {
                var cell = instance[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new(cell.TypeId));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    var pass = material.GetPass("DepthOnly");

                    if (pass == null)
                    {
                        continue;
                    }

                    pass.Bindings.SetSRV("maskTex", layer.Mask.SRV);

                    if (pass.BeginDraw(context))
                    {
                        context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    }
                    material.EndDraw(context);
                }
                cell.Unbind(context);
            }
        }

        public override void DrawForward(IGraphicsContext context, TerrainGrid instance)
        {
            for (int i = 0; i < instance.Count; i++)
            {
                var cell = instance[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new(cell.TypeId));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    var pass = material.GetPass("Forward");

                    if (pass == null)
                    {
                        continue;
                    }

                    pass.Bindings.SetSRV("maskTex", layer.Mask.SRV);

                    if (pass.BeginDraw(context))
                    {
                        context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    }
                    material.EndDraw(context);
                }
                cell.Unbind(context);
            }
        }

        public override void DrawDeferred(IGraphicsContext context, TerrainGrid instance)
        {
            for (int i = 0; i < instance.Count; i++)
            {
                var cell = instance[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new(cell.TypeId));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    var pass = material.GetPass("Deferred");

                    if (pass == null)
                    {
                        continue;
                    }

                    pass.Bindings.SetSRV("maskTex", layer.Mask.SRV);

                    if (pass.BeginDraw(context))
                    {
                        context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    }
                    material.EndDraw(context);
                }
                cell.Unbind(context);
            }
        }

        public override void DrawDepth(IGraphicsContext context, TerrainGrid instance, IBuffer camera)
        {
            for (int i = 0; i < instance.Count; i++)
            {
                var cell = instance[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new(cell.TypeId));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    var pass = material.GetPass("DepthOnly");

                    if (pass == null)
                    {
                        continue;
                    }

                    pass.Bindings.SetSRV("maskTex", layer.Mask.SRV);

                    if (pass.BeginDraw(context))
                    {
                        context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    }
                    material.EndDraw(context);
                }
                cell.Unbind(context);
            }
        }

        public override void DrawShadowMap(IGraphicsContext context, TerrainGrid instance, IBuffer light, ShadowType type)
        {
            var name = EnumHelper<ShadowType>.GetName(type);

            for (int i = 0; i < instance.Count; i++)
            {
                var cell = instance[i];

                // Skip draw when no layers are present
                if (cell.DrawLayers.Count == 0)
                {
                    continue;
                }

                unsafe
                {
                    offsetBuffer.Update(context, new(cell.TypeId));
                }

                cell.Bind(context);
                for (int j = 0; j < cell.DrawLayers.Count; j++)
                {
                    TerrainDrawLayer layer = cell.DrawLayers[j];
                    TerrainMaterial material = layer.Material;

                    var pass = material.GetPass(name);

                    if (pass == null)
                    {
                        continue;
                    }

                    pass.Bindings.SetCBV("lightBuffer", light);

                    if (pass.BeginDraw(context))
                    {
                        context.DrawIndexedInstanced(cell.IndexCount, 1, 0, 0, 0);
                    }
                    material.EndDraw(context);

                    break;
                }
                cell.Unbind(context);
            }
        }

        protected override void DisposeCore()
        {
            linearClampSampler?.Dispose();
            offsetBuffer?.Dispose();
        }
    }
}