namespace HexaEngine.Daxa
{
    using Hexa.NET.Daxa;
    using HexaEngine.Core.Graphics;

    public class DaxaBuffer : DeviceChildBase, IBuffer
    {
        private readonly DaxaDevice device;
        private readonly DaxaBufferId bufferId;

        public DaxaBuffer(DaxaDevice device, DaxaBufferId bufferId)
        {
            this.device = device;
            this.bufferId = bufferId;

            device.DvcDestroyBuffer(bufferId);
            nativePointer = (nint)bufferId.Value;
        }

        public BufferDescription Description { get; }

        public int Length { get; }

        public ResourceDimension Dimension => ResourceDimension.Buffer;

        protected override void DisposeCore()
        {
            device.DvcDestroyBuffer(bufferId);
        }
    }
}