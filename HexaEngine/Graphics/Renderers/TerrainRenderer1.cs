namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using System.Numerics;

    public class TerrainRenderer1 : BaseRenderer<TerrainGrid>
    {
        private ISamplerState linearClampSampler;
        private ConstantBuffer<UPoint4> offsetBuffer;
        private DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private StructuredBuffer<Matrix4x4> transformNoBuffer;
        private StructuredBuffer<uint> transformNoOffsetBuffer;
        private StructuredUavBuffer<Matrix4x4> transformBuffer;
        private StructuredUavBuffer<uint> transformOffsetBuffer;

        protected override void Initialize(IGraphicsDevice device, CullingContext cullingContext)
        {
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            offsetBuffer = new(CpuAccessFlags.Write);
            drawIndirectArgs = cullingContext.DrawIndirectArgs;
            transformNoBuffer = cullingContext.InstanceDataNoCull;
            transformNoOffsetBuffer = cullingContext.InstanceOffsetsNoCull;
            transformBuffer = cullingContext.InstanceDataOutBuffer;
            transformOffsetBuffer = cullingContext.InstanceOffsets;
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
            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer.SRV);

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

                    context.PSSetSampler(0, linearClampSampler);
                    context.PSSetShaderResource(0, layer.Mask.SRV);
                    context.DSSetSampler(0, linearClampSampler);
                    context.DSSetShaderResource(0, layer.Mask.SRV);
                    material.DrawIndexedInstanced(context, "DepthOnly", cell.IndexCount, 1, 0, 0, 0);
                }
                cell.Unbind(context);
            }
            context.PSSetShaderResource(0, null);
            context.PSSetSampler(0, null);
            context.DSSetShaderResource(0, null);
            context.DSSetSampler(0, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawForward(IGraphicsContext context, TerrainGrid instance)
        {
            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

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

                    context.PSSetShaderResource(11, layer.Mask.SRV);
                    context.DSSetSampler(0, linearClampSampler);
                    context.DSSetShaderResource(0, layer.Mask.SRV);
                    material.DrawIndexedInstancedIndirect(context, "Forward", drawIndirectArgs, cell.DrawIndirectOffset);
                }
                cell.Unbind(context);
            }
            context.PSSetShaderResource(11, null);
            context.DSSetShaderResource(0, null);
            context.DSSetSampler(0, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawDeferred(IGraphicsContext context, TerrainGrid instance)
        {
            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

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

                    context.PSSetShaderResource(11, layer.Mask.SRV);
                    context.DSSetSampler(0, linearClampSampler);
                    context.DSSetShaderResource(0, layer.Mask.SRV);
                    material.DrawIndexedInstancedIndirect(context, "Deferred", drawIndirectArgs, cell.DrawIndirectOffset);
                }
                cell.Unbind(context);
            }

            context.PSSetShaderResource(11, null);
            context.DSSetShaderResource(0, null);
            context.DSSetSampler(0, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawDepth(IGraphicsContext context, TerrainGrid instance, IBuffer camera)
        {
            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer.SRV);

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

                    context.PSSetSampler(0, linearClampSampler);
                    context.PSSetShaderResource(0, layer.Mask.SRV);
                    context.DSSetSampler(0, linearClampSampler);
                    context.DSSetShaderResource(0, layer.Mask.SRV);
                    material.DrawIndexedInstanced(context, "DepthOnly", cell.IndexCount, 1, 0, 0, 0);
                }
                cell.Unbind(context);
            }
            context.PSSetShaderResource(0, null);
            context.PSSetSampler(0, null);
            context.DSSetShaderResource(0, null);
            context.DSSetSampler(0, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        public override void DrawShadowMap(IGraphicsContext context, TerrainGrid instance, IBuffer light, ShadowType type)
        {
            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformNoBuffer.SRV);
            context.VSSetShaderResource(1, transformNoOffsetBuffer.SRV);

            var name = EnumHelper<ShadowType>.GetName(type);

            context.VSSetConstantBuffer(1, light);
            context.GSSetConstantBuffer(0, light);
            context.PSSetConstantBuffer(0, light);
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

                    material.DrawIndexedInstanced(context, name, cell.IndexCount, 1, 0, 0, 0);
                    break;
                }
                cell.Unbind(context);
            }
            context.PSSetConstantBuffer(0, null);
            context.GSSetConstantBuffer(0, null);
            context.VSSetConstantBuffer(1, null);

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
        }

        protected override void DisposeCore()
        {
            linearClampSampler?.Dispose();
            offsetBuffer?.Dispose();
        }
    }
}