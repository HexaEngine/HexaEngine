namespace HexaEngine.Tests
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.D3D11;

    public class DeviceCreationTests
    {
        private IGraphicsDevice device;
        private IGraphicsContext context;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [Platform(Include = "Win")]
        public void DeviceCreationWithoutWindow()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Fail("Only windows is supported");
                return;
            }
            try
            {
                device = new D3D11GraphicsDevice(new DXGIAdapter(), null);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass();
            device.Dispose();
        }

        [Test]
        [Platform(Include = "Win")]
        public void DeviceCreationWithWindow()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Fail("Only windows is supported");
                return;
            }
            SdlWindow window = new();
            window.ShowHidden();
            try
            {
                device = new D3D11GraphicsDevice(new DXGIAdapter(), null);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            Assert.Pass();
            device.Dispose();
            window.Close();
        }
    }
}