using HexaEngine.Core.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace HexaEngine.D3D12
{
    internal class D3D12Buffer : DeviceChildBase, IBuffer
    {
        private ComPtr<ID3D12Resource2> buffer;
        private ComPtr<ID3D12Device10> device;

        public D3D12Buffer(ComPtr<ID3D12Resource2> buffer, ComPtr<ID3D12Device10> device)
        {
            this.buffer = buffer;
            this.device = device;
        }

        public BufferDescription Description { get; }

        public int Length { get; }

        public Core.Graphics.ResourceDimension Dimension { get; }

        protected override void DisposeCore()
        {
            throw new NotImplementedException();
        }
    }
}