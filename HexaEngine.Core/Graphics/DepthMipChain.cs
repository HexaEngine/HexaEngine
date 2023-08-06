namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using Silk.NET.SDL;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public unsafe class DepthMipChain
    {
        private readonly string dbgName;
        public int Height;
        public int Width;
        private Texture2D texture;
        public IRenderTargetView RTV;
        public IShaderResourceView SRV;
        private IShaderResourceView[] srvs;
        private IUnorderedAccessView[] uavs;

        private Viewport[] viewports;
        public int Mips;

        private void** pUavs;

        public IShaderResourceView? Input;

        public DepthMipChain(IGraphicsDevice device, DepthStencilBufferDescription desc, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"DepthMipChain: {Path.GetFileName(filename)}, Line: {lineNumber}";
            Width = desc.Width;
            Height = desc.Height;

            Mips = GetNumMipLevels(desc.Width, desc.Height);

            texture = new(device, Format.R32Float, desc.Width, desc.Height, 1, Mips, CpuAccessFlags.None, GpuAccessFlags.All);
            texture.DebugName = dbgName;
            SRV = device.CreateShaderResourceView(texture);
            SRV.DebugName = dbgName + ".SRV";
            RTV = device.CreateRenderTargetView(texture, new(desc.Width, desc.Height));
            RTV.DebugName = dbgName + ".RTV";

            srvs = new IShaderResourceView[Mips];
            uavs = new IUnorderedAccessView[Mips];
            pUavs = AllocArray((uint)Mips);
            viewports = new Viewport[Mips];
            int mipWidth = desc.Width;
            int mipHeight = desc.Height;

            for (int i = 0; i < Mips; i++)
            {
                srvs[i] = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture2D, Format.R32Float, i, 1, 0, 1));
                uavs[i] = device.CreateUnorderedAccessView(texture, new(UnorderedAccessViewDimension.Texture2D, Format.R32Float, i, 0, 1, BufferUnorderedAccessViewFlags.None));
                pUavs[i] = (void*)uavs[i].NativePointer;
                viewports[i] = new(mipWidth, mipHeight);
                mipWidth /= 2;
                mipHeight /= 2;
            }
        }

        public DepthMipChain(IGraphicsDevice device, int width, int height, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"DepthMipChain: {Path.GetFileName(filename)}, Line: {lineNumber}";
            Width = width;
            Height = height;

            Mips = GetNumMipLevels(width, height);

            texture = new(device, Format.R32Float, width, height, 1, Mips, CpuAccessFlags.None, GpuAccessFlags.All);
            texture.DebugName = dbgName;
            SRV = device.CreateShaderResourceView(texture);
            SRV.DebugName = dbgName + ".SRV";
            RTV = device.CreateRenderTargetView(texture, new(width, height));
            RTV.DebugName = dbgName + ".RTV";

            srvs = new IShaderResourceView[Mips];
            uavs = new IUnorderedAccessView[Mips];
            pUavs = AllocArray((uint)Mips);
            viewports = new Viewport[Mips];
            int mipWidth = width;
            int mipHeight = height;

            for (int i = 0; i < Mips; i++)
            {
                srvs[i] = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture2D, Format.R32Float, i, 1, 0, 1));
                uavs[i] = device.CreateUnorderedAccessView(texture, new(UnorderedAccessViewDimension.Texture2D, Format.R32Float, i, 0, 1, BufferUnorderedAccessViewFlags.None));
                pUavs[i] = (void*)uavs[i].NativePointer;
                viewports[i] = new(mipWidth, mipHeight);
                mipWidth /= 2;
                mipHeight /= 2;
            }
        }

        public Viewport[] Viewports => viewports;

        public IUnorderedAccessView[] UAVs => uavs;

        public IShaderResourceView[] SRVs => srvs;

        public void Debug()
        {
            for (int i = 0; i < Mips; i++)
            {
                var view = viewports[i];
                ImGui.Image(srvs[i].NativePointer, new(view.Width, view.Height), Vector2.Zero, Vector2.One, new(new Vector3(0.2f), 1));
            }
        }

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            Width = width;
            Height = height;
            for (int i = 0; i < Mips; i++)
            {
                srvs[i].Dispose();
                uavs[i].Dispose();
            }

            Free(pUavs);

            RTV.Dispose();
            SRV.Dispose();
            texture.Dispose();

            Mips = GetNumMipLevels(width, height);

            texture.Resize(device, Format.R32Float, width, height, 1, Mips, CpuAccessFlags.None, GpuAccessFlags.All);
            SRV = device.CreateShaderResourceView(texture);
            RTV = device.CreateRenderTargetView(texture, new(width, height));

            srvs = new IShaderResourceView[Mips];
            uavs = new IUnorderedAccessView[Mips];
            pUavs = AllocArray((uint)Mips);
            viewports = new Viewport[Mips];
            int mipWidth = width;
            int mipHeight = height;

            for (int i = 0; i < Mips; i++)
            {
                srvs[i] = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture2D, Format.R32Float, i, 1, 0, 1));
                uavs[i] = device.CreateUnorderedAccessView(texture, new(UnorderedAccessViewDimension.Texture2D, Format.R32Float, i, 0, 1, BufferUnorderedAccessViewFlags.None));
                pUavs[i] = (void*)uavs[i].NativePointer;
                viewports[i] = new(mipWidth, mipHeight);
                mipWidth /= 2;
                mipHeight /= 2;
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < Mips; i++)
            {
                srvs[i].Dispose();
                uavs[i].Dispose();
            }

            Free(pUavs);

            RTV.Dispose();
            SRV.Dispose();
            texture.Dispose();
        }

        public static int GetNumMipLevels(int width, int height)
        {
            int numLevels = 1;
            while (width > 1 && height > 1)
            {
                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);
                ++numLevels;
            }

            return numLevels;
        }
    }
}