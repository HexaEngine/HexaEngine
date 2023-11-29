namespace HexaEngine.Graphics.Culling
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;

    public class CullingContext
    {
        private readonly StructuredBuffer<TypeData> typeDataBuffer;
        private readonly StructuredBuffer<InstanceData> instanceDataBuffer;
        private uint currentType;

        public CullingContext(StructuredBuffer<TypeData> typeDataBuffer, StructuredBuffer<InstanceData> instanceDataBuffer)
        {
            this.typeDataBuffer = typeDataBuffer;
            this.instanceDataBuffer = instanceDataBuffer;
        }

        public void Reset()
        {
            typeDataBuffer.ResetCounter();
            instanceDataBuffer.ResetCounter();
            currentType = 0;
        }

        public void AppendType(TypeData type)
        {
            currentType = typeDataBuffer.Count;
            typeDataBuffer.Add(type);
        }

        public unsafe uint GetDrawArgsOffset()
        {
            return currentType * (uint)sizeof(DrawIndexedInstancedIndirectArgs);
        }

        public void AppendInstance(InstanceData instance)
        {
            instance.Type = currentType;
            instanceDataBuffer.Add(instance);
        }
    }
}