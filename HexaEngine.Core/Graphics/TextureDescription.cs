﻿namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;

    /// <summary>
    /// Describes a texture.
    /// </summary>
    public struct TextureDescription : IEquatable<TextureDescription>
    {
        /// <summary>
        /// The dimension of the texture.
        /// </summary>
        public TextureDimension Dimension;

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height;

        /// <summary>
        /// The depth of the texture.
        /// </summary>
        public int Depth;

        /// <summary>
        /// The size of the array.
        /// </summary>
        public int ArraySize;

        /// <summary>
        /// The number of mip levels.
        /// </summary>
        public int MipLevels;

        /// <summary>
        /// The format of the texture.
        /// </summary>
        public Format Format;

        /// <summary>
        /// The sample description of the texture.
        /// </summary>
        public SampleDescription SampleDescription;

        /// <summary>
        /// The usage of the texture.
        /// </summary>
        public Usage Usage;

        /// <summary>
        /// The bind flags of the texture.
        /// </summary>
        public BindFlags BindFlags;

        /// <summary>
        /// The CPU access flags of the texture.
        /// </summary>
        public CpuAccessFlags CPUAccessFlags;

        /// <summary>
        /// The miscellaneous flags of the texture.
        /// </summary>
        public ResourceMiscFlag MiscFlags;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDescription"/> struct.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="mipLevels">The mip levels.</param>
        /// <param name="sampleDescription">The sample description.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cPUAccessFlags">The c pu access flags.</param>
        /// <param name="miscFlags">The misc flags.</param>
        public TextureDescription(
            TextureDimension dimension,
            int width,
            int height,
            int depth,
            int arraySize,
            int mipLevels,
            SampleDescription sampleDescription,
            Format format,
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDescription"/> struct.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="mipLevels">The mip levels.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cPUAccessFlags">The c pu access flags.</param>
        /// <param name="miscFlags">The misc flags.</param>
        public TextureDescription(
            TextureDimension dimension,
            int width,
            int height,
            int depth,
            int arraySize,
            int mipLevels,
            Format format,
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDescription"/> struct.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="mipLevels">The mip levels.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cPUAccessFlags">The c pu access flags.</param>
        /// <param name="miscFlags">The misc flags.</param>
        public TextureDescription(
            TextureDimension dimension,
            int width,
            int height,
            int depth,
            int mipLevels,
            Format format,
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDescription"/> struct.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cPUAccessFlags">The c pu access flags.</param>
        /// <param name="miscFlags">The misc flags.</param>
        public TextureDescription(
            TextureDimension dimension,
            int width,
            int height,
            Format format,
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDescription"/> struct.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <exception cref="ArgumentException">
        /// Unknown ressource type - resource
        /// or
        /// Buffers cannot be used as a texture - resource
        /// </exception>
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

        /// <summary>
        /// Creates an Texture1D description.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture1D(int width, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture1D, width, 0, 0, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture1D description with an RTV.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture1DWithRTV(int width, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture1D, width, 0, 0, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture1DArray description.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture1DArray(int width, int arraySize, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture1D, width, 0, 0, arraySize, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture1DArray description with an RTV.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture1DArrayWithRTV(int width, int arraySize, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture1D, width, 0, 0, arraySize, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture2D description.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture2D(int width, int height, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, height, 0, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture2D description with an RTV.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture2DWithRTV(int width, int height, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, height, 0, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture2DArray description.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture2DArray(int width, int height, int arraySize, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, height, 0, arraySize, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture2DArray description with an RTV.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture2DArrayWithRTV(int width, int height, int arraySize, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture2D, width, height, 0, arraySize, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an TextureCube description.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTextureCube(int width, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.TextureCube)
        {
            return new TextureDescription(TextureDimension.TextureCube, width, width, 0, 6, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an TextureCube description with an RTV.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTextureCubeWithRTV(int width, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.TextureCube)
        {
            return new TextureDescription(TextureDimension.TextureCube, width, width, 0, 6, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an TextureCubeArray description with an RTV.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="size">The size.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTextureCubeArrayWithRTV(int width, int size, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.TextureCube)
        {
            return new TextureDescription(TextureDimension.TextureCube, width, width, 0, 6 * size, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture3D description.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture3D(int width, int height, int depth, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture3D, width, height, depth, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Creates an Texture3D description with an RTV.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="mips">The mips.</param>
        /// <param name="format">The format.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="bindFlags">The bind flags.</param>
        /// <param name="cpuAccessFlags">The cpu access flags.</param>
        /// <param name="resourceOptionFlags">The resource option flags.</param>
        /// <returns></returns>
        public static TextureDescription CreateTexture3DWithRTV(int width, int height, int depth, int mips, Format format, Usage usage = Usage.Default, BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceOptionFlags = ResourceMiscFlag.None)
        {
            return new TextureDescription(TextureDimension.Texture3D, width, height, depth, mips, format, usage, bindFlags, cpuAccessFlags, resourceOptionFlags);
        }

        /// <summary>
        /// Gets the depth stencil view dimension.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the viewport.
        /// </summary>
        /// <returns></returns>
        public Viewport GetViewport()
        {
            return new(Width, Height);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref dbgName="obj" /> and this instance are the same type and represent the same _value; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj is TextureDescription description && Equals(description);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref dbgName="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(TextureDescription other)
        {
            return Dimension == other.Dimension &&
                   Width == other.Width &&
                   Height == other.Height &&
                   Depth == other.Depth &&
                   ArraySize == other.ArraySize &&
                   MipLevels == other.MipLevels &&
                   Format == other.Format &&
                   SampleDescription == other.SampleDescription &&
                   Usage == other.Usage &&
                   BindFlags == other.BindFlags &&
                   CPUAccessFlags == other.CPUAccessFlags &&
                   MiscFlags == other.MiscFlags;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Dimension);
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(Depth);
            hash.Add(ArraySize);
            hash.Add(MipLevels);
            hash.Add(Format);
            hash.Add(SampleDescription);
            hash.Add(Usage);
            hash.Add(BindFlags);
            hash.Add(CPUAccessFlags);
            hash.Add(MiscFlags);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(TextureDescription left, TextureDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(TextureDescription left, TextureDescription right)
        {
            return !(left == right);
        }
    }
}