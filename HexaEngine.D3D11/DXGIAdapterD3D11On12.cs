namespace HexaEngine.D3D11
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Contexts;
    using System.Runtime.Versioning;

    public unsafe class DXGIAdapterD3D11On12 : DXGIAdapterD3D11
    {
        public DXGIAdapterD3D11On12(INativeWindowSource windowSource, bool debug) : base(windowSource, debug)
        {
        }

        public new static void Init(INativeWindowSource windowSource, bool debug)
        {
            if (OperatingSystem.IsWindows())
            {
                GraphicsAdapter.Adapters.Add(new DXGIAdapterD3D11On12(windowSource, debug));
            }
        }

        public override GraphicsBackend Backend => GraphicsBackend.D3D11On12;

        public override int PlatformScore => 150;

        [SupportedOSPlatform("windows")]
        public override IGraphicsDevice CreateGraphicsDevice(bool debug)
        {
            AdapterDesc1 desc;
            IDXGIAdapter.GetDesc1(&desc);
            string name = new(&desc.Description_0);

            LoggerFactory.General.Info("Backend: Using Graphics API: D3D11On12");
            LoggerFactory.General.Info($"Backend: Using Graphics Device: {name}");
            return new D3D11On12GraphicsDevice(this, debug);
        }
    }
}