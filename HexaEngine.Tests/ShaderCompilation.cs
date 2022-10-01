namespace HexaEngine.Tests
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.D3D11;
    using System;

    public class ShaderCompilation
    {
        private IGraphicsDevice device;

        [SetUp]
        public void Setup()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Fail("Only windows is supported");
                return;
            }
            device = new D3D11GraphicsDevice(new DXGIAdapter(), null);
        }

        [Test]
        public void VertexShader()
        {
            device.CompileFromFile("deferred/prepass/vs.hlsl", "main", "vs_5_0", out Blob? vsb, out Blob? e);
            if (vsb == null)
            {
                Assert.Fail(e?.AsString());
                return;
            }
            var vs = device.CreateVertexShader(vsb);
            var layout = device.CreateInputLayout(vsb);
            vs.Dispose();
            layout.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void HullShader()
        {
            device.CompileFromFile("deferred/prepass/hs.hlsl", "main", "hs_5_0", out Blob? vsb, out Blob? e);
            if (vsb == null)
            {
                Assert.Fail(e?.AsString());
                return;
            }
            var hs = device.CreateHullShader(vsb);
            hs.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void DomainShader()
        {
            device.CompileFromFile("deferred/prepass/ds.hlsl", "main", "ds_5_0", out Blob? vsb, out Blob? e);
            if (vsb == null)
            {
                Assert.Fail(e?.AsString());
                return;
            }
            var s = device.CreateDomainShader(vsb);
            s.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void PixelShader()
        {
            device.CompileFromFile("deferred/prepass/ps.hlsl", "main", "ps_5_0", out Blob? vsb, out Blob? e);
            if (vsb == null)
            {
                Assert.Fail(e?.AsString());
                return;
            }
            var s = device.CreatePixelShader(vsb);
            s.Dispose();
            LeakTracer.ReportLiveInstances();
        }
    }
}