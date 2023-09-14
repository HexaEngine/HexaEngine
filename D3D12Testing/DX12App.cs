namespace D3D12Testing
{
    using D3D12Testing.Windows;
    using HexaEngine.D3D12;

    public class DX12App
    {
        private readonly DXGIAdapterD3D12 adapter;
        private D3D12GraphicsDevice device;

        public DX12App(IWindow window)
        {
            adapter = new(window, true);
            device = adapter.CreateGraphicsDevice(true);
        }

        public void Render()
        {
        }

        public void Dispose()
        {
        }
    }
}