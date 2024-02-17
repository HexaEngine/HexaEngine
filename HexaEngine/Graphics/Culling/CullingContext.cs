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
        private readonly StructuredBuffer<InstanceData> instanceDataBuffer;
        private readonly StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer;
        private readonly DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs;
        private uint currentType;
        private int count;
        private int typeCount;

        public CullingContext(StructuredBuffer<uint> instanceOffsetsNoCull, StructuredBuffer<Matrix4x4> instanceDataNoCull, StructuredUavBuffer<uint> instanceOffsets, StructuredUavBuffer<Matrix4x4> instanceDataOutBuffer, StructuredBuffer<TypeData> typeDataBuffer, StructuredBuffer<InstanceData> instanceDataBuffer, StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> swapBuffer, DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> drawIndirectArgs)
        {
            this.instanceOffsetsNoCull = instanceOffsetsNoCull;
            this.instanceDataNoCull = instanceDataNoCull;
            this.instanceOffsets = instanceOffsets;
            this.instanceDataOutBuffer = instanceDataOutBuffer;
            this.typeDataBuffer = typeDataBuffer;
            this.instanceDataBuffer = instanceDataBuffer;
            this.swapBuffer = swapBuffer;
            this.drawIndirectArgs = drawIndirectArgs;
        }

        public StructuredBuffer<uint> InstanceOffsetsNoCull => instanceOffsetsNoCull;

        public StructuredBuffer<Matrix4x4> InstanceDataNoCull => instanceDataNoCull;

        public StructuredUavBuffer<uint> InstanceOffsets => instanceOffsets;

        public StructuredUavBuffer<Matrix4x4> InstanceDataOutBuffer => instanceDataOutBuffer;

        public DrawIndirectArgsBuffer<DrawIndexedInstancedIndirectArgs> DrawIndirectArgs => drawIndirectArgs;

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
            instanceDataOutBuffer.Capacity = instanceDataBuffer.Capacity;
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

        public void AppendType(TypeData type)
        {
            swapBuffer.Add(new(type.IndexCountPerInstance, 0, type.StartIndexLocation, type.BaseVertexLocation, type.StartInstanceLocation));
            instanceOffsetsNoCull.Add(instanceDataNoCull.Count);
            currentType = typeDataBuffer.Count;
            typeDataBuffer.Add(type);
        }

        public void AppendType(uint indexCountPerInstance, uint startIndexLocation = 0, int baseVertexLocation = 0, uint startInstanceLocation = 0)
        {
            AppendType(new(indexCountPerInstance, startIndexLocation, baseVertexLocation, startInstanceLocation));
        }

        public void AppendInstance(InstanceData instance)
        {
            instanceDataNoCull.Add(instance.World);
            instanceDataBuffer.Add(instance);
        }

        public void AppendInstance(Matrix4x4 world, Vector3 min, Vector3 max, Vector3 center, float radius)
        {
            AppendInstance(new(currentType, world, min, max, center, radius));
        }

        public void AppendInstance(Matrix4x4 world, BoundingBox boundingBox, Vector3 center, float radius)
        {
            AppendInstance(new(currentType, world, boundingBox, center, radius));
        }

        public void AppendInstance(Matrix4x4 world, BoundingBox boundingBox, BoundingSphere boundingSphere)
        {
            AppendInstance(new(currentType, world, boundingBox, boundingSphere));
        }

        public void AppendInstance(Matrix4x4 world, BoundingBox boundingBox)
        {
            AppendInstance(new(currentType, world, boundingBox));
        }

        public void AppendInstance(Matrix4x4 world, BoundingSphere boundingSphere)
        {
            AppendInstance(new(currentType, world, boundingSphere));
        }
    }
}