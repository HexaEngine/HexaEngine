namespace HexaEngine.Graphics
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;

    public struct TextureDescription
    {
        public TextureDimension Dimension;
        public int Width;
        public int Height;
        public int Depth;
        public int ArraySize;
        public int MipLevels;
        public Format Format;
        public SampleDescription SampleDescription;
        public Usage Usage;
        public BindFlags BindFlags;
        public CpuAccessFlags CPUAccessFlags;
        public ResourceMiscFlag MiscFlags;

        public TextureDescription(
            TextureDimension dimension,
            int width,
            int height,
            int depth,
            int arraySize,
            int mipLevels,
            SampleDescription sampleDescription,
            Format format = Format.RGBA32Float,
            Usage usage = Usage.Default,
            BindFlags bindFlags = BindFlags.ShaderResource,
            CpuAccessFlags cPUAccessFlags = CpuAccessFlags.None,
            ResourceMiscFlag miscFlags = ResourceMiscFlag.None
            )
        {
            Dimension = dimension;
            Width = width;
            Height = height;
            Depth = depth;
            ArraySize = arraySize;
            MipLevels = mipLevels;
            Format = format;
            SampleDescription = sampleDescription;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cPUAccessFlags;
            MiscFlags = miscFlags;
        }

        public TextureDescription(
            TextureDimension dimension,
            int width,
            int height,
            int depth,
            int arraySize,
            int mipLevels,
            Format format = Format.RGBA32Float,
            Usage usage = Usage.Default,
            BindFlags bindFlags = BindFlags.ShaderResource,
            CpuAccessFlags cPUAccessFlags = CpuAccessFlags.None,
            ResourceMiscFlag miscFlags = ResourceMiscFlag.None
            )
        {
            Dimension = dimension;
            Width = width;
            Height = height;
            Depth = depth;
            ArraySize = arraySize;
            MipLevels = mipLevels;
            Format = format;
            SampleDescription = SampleDescription.Default;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cPUAccessFlags;
            MiscFlags = miscFlags;
        }

        public TextureDescription(
            TextureDimension dimension,
            int width,
            int height,
            int depth,
            int mipLevels,
            Format format = Format.RGBA32Float,
            Usage usage = Usage.Default,
            BindFlags bindFlags = BindFlags.ShaderResource,
            CpuAccessFlags cPUAccessFlags = CpuAccessFlags.None,
            ResourceMiscFlag miscFlags = ResourceMiscFlag.None
            )
        {
            Dimension = dimension;
            Width = width;
            Height = height;
            Depth = depth;
            ArraySize = 1;
            MipLevels = mipLevels;
            Format = format;
            SampleDescription = SampleDescription.Default;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cPUAccessFlags;
            MiscFlags = miscFlags;
        }

        public TextureDescription(
            TextureDimension dimension,
            int width,
            int height,
            Format format = Format.RGBA32Float,
            Usage usage = Usage.Default,
            BindFlags bindFlags = BindFlags.ShaderResource,
            CpuAccessFlags cPUAccessFlags = CpuAccessFlags.None,
            ResourceMiscFlag miscFlags = ResourceMiscFlag.None
            )
        {
            Dimension = dimension;
            Width = width;
            Height = height;
            Depth = 0;
            ArraySize = 1;
            MipLevels = 1;
            Format = format;
            SampleDescription = SampleDescription.Default;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cPUAccessFlags;
            MiscFlags = miscFlags;
        }

        public TextureDescription(IResource resource) : this()
        {
            switch (resource.Dimension)
            {
                case ResourceDimension.Unknown:
                    throw new ArgumentException("Unknown ressource type", nameof(resource));

                case ResourceDimension.Buffer:
                    throw new ArgumentException("Buffers cannot be used as a texture", nameof(resource));

                case ResourceDimension.Texture1D:
                    {
                        Dimension = TextureDimension.Texture1D;
                        ITexture1D tex = (ITexture1D)resource;
                        Texture1DDescription desc = tex.Description;
                        Width = desc.Width;
                        MipLevels = desc.MipLevels;
                        ArraySize = desc.ArraySize;
                        Format = desc.Format;
                        Usage = desc.Usage;
                        BindFlags = desc.BindFlags;
                        CPUAccessFlags = desc.CPUAccessFlags;
                        MiscFlags = desc.MiscFlags;
                    }
                    break;

                case ResourceDimension.Texture2D:
                    {
                        Dimension = TextureDimension.Texture2D;
                        ITexture2D tex = (ITexture2D)resource;
                        Texture2DDescription desc = tex.Description;
                        Width = desc.Width;
                        Height = desc.Height;
                        MipLevels = desc.MipLevels;
                        ArraySize = desc.ArraySize;
                        Format = desc.Format;
                        SampleDescription = desc.SampleDescription;
                        Usage = desc.Usage;
                        BindFlags = desc.BindFlags;
                        CPUAccessFlags = desc.CPUAccessFlags;
                        MiscFlags = desc.MiscFlags;
                    }
                    break;

                case ResourceDimension.Texture3D:
                    {
                        Dimension = TextureDimension.Texture3D;
                        ITexture3D tex = (ITexture3D)resource;
                        Texture3DDescription desc = tex.Description;
                        Width = desc.Width;
                        Height = desc.Height;
                        Depth = desc.Depth;
                        MipLevels = desc.MipLevels;
                        Format = desc.Format;
                        Usage = desc.Usage;
                        BindFlags = desc.BindFlags;
                        CPUAccessFlags = desc.CPUAccessFlags;
                        MiscFlags = desc.MiscFlags;
                    }
                    break;
            }
        }

        public static TextureDescription CreateTexture1D(int width, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture1D, width, 0, 0, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture1DWithRTV(int width, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture1D, width, 0, 0, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture1DArray(int width, int arraySize, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture1D, width, 0, 0, arraySize, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture1DArrayWithRTV(int width, int arraySize, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture1D, width, 0, 0, arraySize, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture2D(int width, int height, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, height, 0, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture2DWithRTV(int width, int height, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, height, 0, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture2DArray(int width, int height, int arraySize, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, height, 0, arraySize, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture2DArrayWithRTV(int width, int height, int arraySize, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, height, 0, arraySize, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTextureCube(int width, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.TextureCube)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, width, 0, 6, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTextureCubeWithRTV(int width, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.TextureCube)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, width, 0, 6, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture3D(int width, int height, int depth, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture3D, width, height, depth, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public static TextureDescription CreateTexture3DWithRTV(int width, int height, int depth, int mips, Format format = Format.RGBA32Float, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture3D, width, height, depth, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        public DepthStencilViewDimension GetDepthStencilViewDimension()
        {
            return Dimension switch
            {
                TextureDimension.Texture1D => ArraySize > 1 ? DepthStencilViewDimension.Texture1DArray : DepthStencilViewDimension.Texture1D,
                TextureDimension.Texture2D => ArraySize > 1 ? SampleDescription.Count > 1 ? DepthStencilViewDimension.Texture2DMultisampledArray : DepthStencilViewDimension.Texture2DArray : SampleDescription.Count > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                TextureDimension.Texture3D => SampleDescription.Count > 1 ? DepthStencilViewDimension.Texture2DMultisampledArray : DepthStencilViewDimension.Texture2DArray,
                TextureDimension.TextureCube => SampleDescription.Count > 1 ? DepthStencilViewDimension.Texture2DMultisampledArray : DepthStencilViewDimension.Texture2DArray,
                _ => DepthStencilViewDimension.Unknown,
            };
        }

        public Viewport GetViewport()
        {
            return new(Width, Height);
        }
    }

    public struct TextureFileDescription
    {
        public string Path;
        public TextureDimension Dimension;
        public int MipLevels;
        public Usage Usage;
        public BindFlags BindFlags;
        public CpuAccessFlags CPUAccessFlags;
        public bool ForceSRGB;

        public TextureFileDescription(
            string path,
            TextureDimension dimension = TextureDimension.Texture2D,
            int mipLevels = 0,
            Usage usage = Usage.Default,
            BindFlags bindFlags = BindFlags.ShaderResource,
            CpuAccessFlags cPUAccessFlags = CpuAccessFlags.None,
            bool forceSRGB = false
            )
        {
            Path = path;
            Dimension = dimension;
            MipLevels = mipLevels;
            Usage = usage;
            BindFlags = bindFlags;
            CPUAccessFlags = cPUAccessFlags;
            ForceSRGB = forceSRGB;
        }
    }

    public struct DepthStencilDesc
    {
        public Format Format;
        public BindFlags BindFlags;
        public Usage Usage;
        public CpuAccessFlags CPUAccessFlags;
        public DepthStencilViewFlags ViewFlags;
        public SampleDescription SampleDescription;

        public static DepthStencilDesc Default => new(Format.Depth24UNormStencil8, BindFlags.DepthStencil, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, SampleDescription.Default);
        public static DepthStencilDesc DefaultMS => new(Format.Depth32FloatStencil8, BindFlags.DepthStencil, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, new(Nucleus.Settings.MSAASampleCount, Nucleus.Settings.MSAASampleQuality));

        public DepthStencilDesc(Format format, BindFlags bindFlags, Usage usage, CpuAccessFlags cPUAccessFlags, DepthStencilViewFlags viewFlags, SampleDescription sampleDescription)
        {
            Format = format;
            BindFlags = bindFlags;
            Usage = usage;
            CPUAccessFlags = cPUAccessFlags;
            ViewFlags = viewFlags;
            SampleDescription = sampleDescription;
        }
    }
}