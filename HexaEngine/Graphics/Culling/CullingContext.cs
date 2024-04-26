namespace HexaEngine.Graphics.Culling
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class CullingContext
    {
        private readonly StructuredBuffer<uint> instanceOffsetsNoCull;
        private readonly StructuredBuffer<Matrix4x4> instanceDataNoCull;
        private readonly StructuredUavBuffer<uint> instanceOffsets;
        private readonly StructuredUavBuffer<Matrix4x4> instanceDataOutBuffer;
        private readonly StructuredBuffer<TypeData> typeDataBuffer;
        private readonly StructuredBuffer<GPUInstance> instanceDataBuffer;
        private readonly StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer;
        private readonly DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private readonly StructuredUavBuffer<uint> visibleListBuffer;
        private uint currentType;
        private int count;
        private int typeCount;

        public CullingContext(StructuredBuffer<uint> instanceOffsetsNoCull, StructuredBuffer<Matrix4x4> instanceDataNoCull, StructuredUavBuffer<uint> instanceOffsets, StructuredUavBuffer<Matrix4x4> instanceDataOutBuffer, StructuredBuffer<TypeData> typeDataBuffer, StructuredBuffer<GPUInstance> instanceDataBuffer, StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer, DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs, StructuredUavBuffer<uint> visibleListBuffer)
        {
            this.instanceOffsetsNoCull = instanceOffsetsNoCull;
            this.instanceDataNoCull = instanceDataNoCull;
            this.instanceOffsets = instanceOffsets;
            this.instanceDataOutBuffer = instanceDataOutBuffer;
            this.typeDataBuffer = typeDataBuffer;
            this.instanceDataBuffer = instanceDataBuffer;
            this.swapBuffer = swapBuffer;
            this.drawIndirectArgs = drawIndirectArgs;
            this.visibleListBuffer = visibleListBuffer;
        }

        public StructuredBuffer<uint> InstanceOffsetsNoCull => instanceOffsetsNoCull;

        public StructuredBuffer<Matrix4x4> InstanceDataNoCull => instanceDataNoCull;

        public StructuredUavBuffer<uint> InstanceOffsets => instanceOffsets;

        public StructuredUavBuffer<Matrix4x4> InstanceDataOutBuffer => instanceDataOutBuffer;

        public DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> DrawIndirectArgs => drawIndirectArgs;
        public StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> SwapBuffer => swapBuffer;
        public StructuredUavBuffer<uint> VisibleList => visibleListBuffer;

        public uint CurrentType => currentType;

        public int TypeCount => typeCount;

        public int Count => count;

        public void Reset()
        {
            typeDataBuffer.ResetCounter();
            instanceDataBuffer.ResetCounter();
            instanceDataNoCull.ResetCounter();
            instanceOffsetsNoCull.ResetCounter();
            swapBuffer.Clear();
            currentType = 0;
        }

        public void Flush(IGraphicsContext context)
        {
            swapBuffer.Update(context);

            drawIndirectArgs.Capacity = instanceOffsets.Capacity = swapBuffer.Capacity;
            visibleListBuffer.Capacity = instanceDataOutBuffer.Capacity = instanceDataBuffer.Capacity;
            instanceDataBuffer.Update(context);
            typeCount = (int)typeDataBuffer.Count;
            count = (int)instanceDataBuffer.Count;

            typeDataBuffer.Update(context);

            instanceDataNoCull.Update(context);
            instanceOffsetsNoCull.Update(context);
        }

        public unsafe uint GetDrawArgsOffset()
        {
            return currentType * (uint)sizeof(DrawIndexedInstancedIndirectArgs);
        }

        public uint AppendType(TypeData type)
        {
            swapBuffer.Add(new(type.IndexCountPerInstance, 0, type.StartIndexLocation, type.BaseVertexLocation, type.StartInstanceLocation));
            instanceOffsetsNoCull.Add(instanceDataNoCull.Count);
            currentType = typeDataBuffer.Count;
            typeDataBuffer.Add(type);
            return currentType;
        }

        public uint AppendType(uint indexCountPerInstance, uint startIndexLocation = 0, int baseVertexLocation = 0, uint startInstanceLocation = 0)
        {
            return AppendType(new(indexCountPerInstance, startIndexLocation, baseVertexLocation, startInstanceLocation));
        }

        public uint AppendInstance(GPUInstance instance)
        {
            uint id = instanceDataBuffer.Count;
            instanceDataNoCull.Add(instance.World);
            instanceDataBuffer.Add(instance);
            return id;
        }

        public uint AppendInstance(Matrix4x4 world, Vector3 center, float radius)
        {
            return AppendInstance(new(currentType, world, center, radius));
        }

        public uint AppendInstance(Matrix4x4 world, BoundingBox boundingBox)
        {
            return AppendInstance(new(currentType, world, boundingBox));
        }

        public uint AppendInstance(Matrix4x4 world, BoundingSphere boundingSphere)
        {
            return AppendInstance(new(currentType, world, boundingSphere));
        }
    }
}