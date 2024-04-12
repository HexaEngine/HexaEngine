namespace HexaEngine.Core.Graphics
{
    public static class AccessHelper
    {
        public static void Convert(Usage usage, BindFlags bindFlags, out CpuAccessFlags cpuAccessFlags, out GpuAccessFlags gpuAccessFlags)
        {
            cpuAccessFlags = CpuAccessFlags.None;
            gpuAccessFlags = GpuAccessFlags.None;

            switch (usage)
            {
                case Usage.Default:
                    if ((bindFlags & BindFlags.ShaderResource) != 0)
                        gpuAccessFlags |= GpuAccessFlags.Read;
                    if ((bindFlags & BindFlags.RenderTarget) != 0)
                        gpuAccessFlags |= GpuAccessFlags.Write;
                    if ((bindFlags & BindFlags.UnorderedAccess) != 0)
                        gpuAccessFlags |= GpuAccessFlags.UA;
                    if ((bindFlags & BindFlags.DepthStencil) != 0)
                        gpuAccessFlags |= GpuAccessFlags.DepthStencil;
                    break;

                case Usage.Dynamic:
                    cpuAccessFlags |= CpuAccessFlags.Write;
                    if ((bindFlags & BindFlags.ShaderResource) != 0)
                        gpuAccessFlags |= GpuAccessFlags.Read;
                    break;

                case Usage.Staging:
                    cpuAccessFlags |= CpuAccessFlags.Read | CpuAccessFlags.Write;
                    break;
            }
        }

        public static void Convert(CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, out Usage usage, out BindFlags bindFlags)
        {
            usage = Usage.Default;
            bindFlags = BindFlags.None;
            if ((gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.ShaderResource;
            }

            if ((gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.RenderTarget;
            }

            if ((gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.UnorderedAccess;
            }

            if ((gpuAccessFlags & GpuAccessFlags.DepthStencil) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.DepthStencil;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                usage = Usage.Dynamic;
                bindFlags = BindFlags.ShaderResource;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                usage = Usage.Staging;
                bindFlags = BindFlags.None;
            }
        }
    }
}