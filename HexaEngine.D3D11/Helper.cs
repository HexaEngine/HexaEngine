namespace HexaEngine.D3D11
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.Graphics.Textures;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static unsafe class Helper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DirectXTex.CPFlags Convert(CPFlags flags)
        {
            Hexa.NET.DirectXTex.CPFlags result = 0;
            if ((flags & CPFlags.None) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.None;
            }

            if ((flags & CPFlags.LegacyDWORD) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.LegacyDword;
            }

            if ((flags & CPFlags.Paragraph) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.Paragraph;
            }

            if ((flags & CPFlags.YMM) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.Ymm;
            }

            if ((flags & CPFlags.ZMM) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.Zmm;
            }

            if ((flags & CPFlags.Page4K) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.Page4K;
            }

            if ((flags & CPFlags.BadDXTNTails) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.BadDxtnTails;
            }

            if ((flags & CPFlags.BPP24) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.Flags24Bpp;
            }

            if ((flags & CPFlags.BPP16) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.Flags16Bpp;
            }

            if ((flags & CPFlags.BPP8) != 0)
            {
                result |= Hexa.NET.DirectXTex.CPFlags.Flags8Bpp;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DirectXTex.WICCodecs Convert(TexFileFormat format)
        {
            return format switch
            {
                TexFileFormat.DDS => throw new NotSupportedException(),
                TexFileFormat.TGA => throw new NotSupportedException(),
                TexFileFormat.HDR => throw new NotSupportedException(),
                TexFileFormat.BMP => Hexa.NET.DirectXTex.WICCodecs.CodecBmp,
                TexFileFormat.JPEG => Hexa.NET.DirectXTex.WICCodecs.CodecJpeg,
                TexFileFormat.PNG => Hexa.NET.DirectXTex.WICCodecs.CodecPng,
                TexFileFormat.TIFF => Hexa.NET.DirectXTex.WICCodecs.CodecTiff,
                TexFileFormat.GIF => Hexa.NET.DirectXTex.WICCodecs.CodecGif,
                TexFileFormat.WMP => Hexa.NET.DirectXTex.WICCodecs.CodecWmp,
                TexFileFormat.ICO => Hexa.NET.DirectXTex.WICCodecs.CodecIco,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DirectXTex.TexFilterFlags Convert(TexFilterFlags flags)
        {
            Hexa.NET.DirectXTex.TexFilterFlags result = 0;
            if ((flags & TexFilterFlags.Default) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Default;
            }

            if ((flags & TexFilterFlags.WrapU) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.WrapU;
            }

            if ((flags & TexFilterFlags.WrapV) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.WrapV;
            }

            if ((flags & TexFilterFlags.WrapW) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.WrapW;
            }

            if ((flags & TexFilterFlags.Wrap) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Wrap;
            }

            if ((flags & TexFilterFlags.MirrorU) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.MirrorU;
            }

            if ((flags & TexFilterFlags.MirrorV) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.MirrorV;
            }

            if ((flags & TexFilterFlags.MirrorW) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.MirrorW;
            }

            if ((flags & TexFilterFlags.Mirror) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Mirror;
            }

            if ((flags & TexFilterFlags.SeparateAlpha) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.SeparateAlpha;
            }

            if ((flags & TexFilterFlags.FloatX2Bias) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.FloatX2Bias;
            }

            if ((flags & TexFilterFlags.RGBCopyRed) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.RgbCopyRed;
            }

            if ((flags & TexFilterFlags.RGBCopyGreen) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.RgbCopyGreen;
            }

            if ((flags & TexFilterFlags.RGBCopyBlue) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.RgbCopyBlue;
            }

            if ((flags & TexFilterFlags.Dither) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Dither;
            }

            if ((flags & TexFilterFlags.DitherDiffusion) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.DitherDiffusion;
            }

            if ((flags & TexFilterFlags.Point) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Point;
            }

            if ((flags & TexFilterFlags.Linear) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Linear;
            }

            if ((flags & TexFilterFlags.Cubic) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Cubic;
            }

            if ((flags & TexFilterFlags.Box) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Box;
            }

            if ((flags & TexFilterFlags.Triangle) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Triangle;
            }

            if ((flags & TexFilterFlags.SRGBIn) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.SrgbIn;
            }

            if ((flags & TexFilterFlags.SRGBOut) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.SrgbOut;
            }

            if ((flags & TexFilterFlags.SRGB) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.Srgb;
            }

            if ((flags & TexFilterFlags.ForceNonWIC) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.ForceNonWic;
            }

            if ((flags & TexFilterFlags.ForceWIC) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexFilterFlags.ForceWic;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DirectXTex.TexCompressFlags Convert(TexCompressFlags flags)
        {
            Hexa.NET.DirectXTex.TexCompressFlags result = 0;
            if ((flags & TexCompressFlags.Default) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.Default;
            }

            if ((flags & TexCompressFlags.DitherRGB) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.RgbDither;
            }

            if ((flags & TexCompressFlags.DitherA) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.ADither;
            }

            if ((flags & TexCompressFlags.Dither) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.Dither;
            }

            if ((flags & TexCompressFlags.Uniform) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.Uniform;
            }

            if ((flags & TexCompressFlags.BC7Use3Sunsets) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.Bc7Use3Subsets;
            }

            if ((flags & TexCompressFlags.BC7Quick) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.Bc7Quick;
            }

            if ((flags & TexCompressFlags.SRGBIn) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.SrgbIn;
            }

            if ((flags & TexCompressFlags.SRGBOut) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.SrgbOut;
            }

            if ((flags & TexCompressFlags.Parallel) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexCompressFlags.Parallel;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Map Convert(MapMode map)
        {
            return map switch
            {
                MapMode.Read => Hexa.NET.D3D11.Map.Read,
                MapMode.Write => Hexa.NET.D3D11.Map.Write,
                MapMode.ReadWrite => Hexa.NET.D3D11.Map.ReadWrite,
                MapMode.WriteDiscard => Hexa.NET.D3D11.Map.WriteDiscard,
                MapMode.WriteNoOverwrite => Hexa.NET.D3D11.Map.WriteNoOverwrite,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Viewport Convert(Viewport viewport)
        {
            return new()
            {
                Height = viewport.Height,
                MaxDepth = viewport.MaxDepth,
                MinDepth = viewport.MinDepth,
                TopLeftX = viewport.X,
                TopLeftY = viewport.Y,
                Width = viewport.Width
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Convert(Viewport* srcViewports, Hexa.NET.D3D11.Viewport* dstViewports, uint count)
        {
            for (uint i = 0; i < count; i++)
            {
                dstViewports[i] = Convert(srcViewports[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ClampAndRound(float value, float min, float max)
        {
            if (float.IsNaN(value))
            {
                return 0.0f;
            }

            if (float.IsInfinity(value))
            {
                return float.IsNegativeInfinity(value) ? min : max;
            }

            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return MathF.Round(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackUNorm(float bitmask, float value)
        {
            value *= bitmask;
            return (uint)ClampAndRound(value, 0.0f, bitmask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackRGBA(float x, float y, float z, float w)
        {
            uint red = PackUNorm(255.0f, x);
            uint green = PackUNorm(255.0f, y) << 8;
            uint blue = PackUNorm(255.0f, z) << 16;
            uint alpha = PackUNorm(255.0f, w) << 24;
            return red | green | blue | alpha;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackRGBA(Vector4 color) => PackRGBA(color.X, color.Y, color.Z, color.W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3DCommon.PrimitiveTopology Convert(PrimitiveTopology topology)
        {
            return topology switch
            {
                PrimitiveTopology.Undefined => Hexa.NET.D3DCommon.PrimitiveTopology.Undefined,
                PrimitiveTopology.PointList => Hexa.NET.D3DCommon.PrimitiveTopology.Pointlist,
                PrimitiveTopology.LineList => Hexa.NET.D3DCommon.PrimitiveTopology.Linelist,
                PrimitiveTopology.LineStrip => Hexa.NET.D3DCommon.PrimitiveTopology.Linestrip,
                PrimitiveTopology.TriangleList => Hexa.NET.D3DCommon.PrimitiveTopology.Trianglelist,
                PrimitiveTopology.TriangleStrip => Hexa.NET.D3DCommon.PrimitiveTopology.Trianglestrip,
                PrimitiveTopology.LineListAdjacency => Hexa.NET.D3DCommon.PrimitiveTopology.LinelistAdj,
                PrimitiveTopology.LineStripAdjacency => Hexa.NET.D3DCommon.PrimitiveTopology.LinestripAdj,
                PrimitiveTopology.TriangleListAdjacency => Hexa.NET.D3DCommon.PrimitiveTopology.TrianglelistAdj,
                PrimitiveTopology.TriangleStripAdjacency => Hexa.NET.D3DCommon.PrimitiveTopology.TrianglestripAdj,
                PrimitiveTopology.PatchListWith1ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology1ControlPointPatchlist,
                PrimitiveTopology.PatchListWith2ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology2ControlPointPatchlist,
                PrimitiveTopology.PatchListWith3ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology3ControlPointPatchlist,
                PrimitiveTopology.PatchListWith4ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology4ControlPointPatchlist,
                PrimitiveTopology.PatchListWith5ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology5ControlPointPatchlist,
                PrimitiveTopology.PatchListWith6ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology6ControlPointPatchlist,
                PrimitiveTopology.PatchListWith7ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology7ControlPointPatchlist,
                PrimitiveTopology.PatchListWith8ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology8ControlPointPatchlist,
                PrimitiveTopology.PatchListWith9ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology9ControlPointPatchlist,
                PrimitiveTopology.PatchListWith10ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology10ControlPointPatchlist,
                PrimitiveTopology.PatchListWith11ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology11ControlPointPatchlist,
                PrimitiveTopology.PatchListWith12ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology12ControlPointPatchlist,
                PrimitiveTopology.PatchListWith13ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology13ControlPointPatchlist,
                PrimitiveTopology.PatchListWith14ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology14ControlPointPatchlist,
                PrimitiveTopology.PatchListWith15ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology15ControlPointPatchlist,
                PrimitiveTopology.PatchListWith16ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology16ControlPointPatchlist,
                PrimitiveTopology.PatchListWith17ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology17ControlPointPatchlist,
                PrimitiveTopology.PatchListWith18ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology18ControlPointPatchlist,
                PrimitiveTopology.PatchListWith19ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology19ControlPointPatchlist,
                PrimitiveTopology.PatchListWith20ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology20ControlPointPatchlist,
                PrimitiveTopology.PatchListWith21ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology21ControlPointPatchlist,
                PrimitiveTopology.PatchListWith22ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology22ControlPointPatchlist,
                PrimitiveTopology.PatchListWith23ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology23ControlPointPatchlist,
                PrimitiveTopology.PatchListWith24ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology24ControlPointPatchlist,
                PrimitiveTopology.PatchListWith25ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology25ControlPointPatchlist,
                PrimitiveTopology.PatchListWith26ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology26ControlPointPatchlist,
                PrimitiveTopology.PatchListWith27ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology27ControlPointPatchlist,
                PrimitiveTopology.PatchListWith28ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology28ControlPointPatchlist,
                PrimitiveTopology.PatchListWith29ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology29ControlPointPatchlist,
                PrimitiveTopology.PatchListWith30ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology30ControlPointPatchlist,
                PrimitiveTopology.PatchListWith31ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology31ControlPointPatchlist,
                PrimitiveTopology.PatchListWith32ControlPoints => Hexa.NET.D3DCommon.PrimitiveTopology.Topology32ControlPointPatchlist,
                _ => throw new ArgumentOutOfRangeException(nameof(topology)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.MapFlag Convert(MapFlags flags)
        {
            return flags switch
            {
                MapFlags.DoNotWait => Hexa.NET.D3D11.MapFlag.DoNotWait,
                MapFlags.None => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(flags)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.ClearFlag Convert(DepthStencilClearFlags flags)
        {
            return flags switch
            {
                DepthStencilClearFlags.None => 0,
                DepthStencilClearFlags.Depth => Hexa.NET.D3D11.ClearFlag.Depth,
                DepthStencilClearFlags.Stencil => Hexa.NET.D3D11.ClearFlag.Stencil,
                DepthStencilClearFlags.All => Hexa.NET.D3D11.ClearFlag.Depth | Hexa.NET.D3D11.ClearFlag.Stencil,
                _ => throw new ArgumentOutOfRangeException(nameof(flags)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture1DDescription ConvertBack(Hexa.NET.D3D11.Texture1DDesc desc)
        {
            return new()
            {
                Format = ConvertBack(desc.Format),
                ArraySize = (int)desc.ArraySize,
                BindFlags = ConvertBack((Hexa.NET.D3D11.BindFlag)desc.BindFlags),
                CPUAccessFlags = ConvertBack((Hexa.NET.D3D11.CpuAccessFlag)desc.CPUAccessFlags),
                MipLevels = (int)desc.MipLevels,
                MiscFlags = ConvertBack((Hexa.NET.D3D11.ResourceMiscFlag)desc.MiscFlags),
                Usage = ConvertBack(desc.Usage),
                Width = (int)desc.Width
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture2DDescription ConvertBack(Hexa.NET.D3D11.Texture2DDesc desc)
        {
            return new()
            {
                Format = ConvertBack(desc.Format),
                ArraySize = (int)desc.ArraySize,
                BindFlags = ConvertBack((Hexa.NET.D3D11.BindFlag)desc.BindFlags),
                CPUAccessFlags = ConvertBack((Hexa.NET.D3D11.CpuAccessFlag)desc.CPUAccessFlags),
                Height = (int)desc.Height,
                MipLevels = (int)desc.MipLevels,
                MiscFlags = ConvertBack((Hexa.NET.D3D11.ResourceMiscFlag)desc.MiscFlags),
                SampleDescription = ConvertBack(desc.SampleDesc),
                Usage = ConvertBack(desc.Usage),
                Width = (int)desc.Width
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture3DDescription ConvertBack(Hexa.NET.D3D11.Texture3DDesc desc)
        {
            return new()
            {
                Format = ConvertBack(desc.Format),
                Depth = (int)desc.Depth,
                BindFlags = ConvertBack((Hexa.NET.D3D11.BindFlag)desc.BindFlags),
                CPUAccessFlags = ConvertBack((Hexa.NET.D3D11.CpuAccessFlag)desc.CPUAccessFlags),
                Height = (int)desc.Height,
                MipLevels = (int)desc.MipLevels,
                MiscFlags = ConvertBack((Hexa.NET.D3D11.ResourceMiscFlag)desc.MiscFlags),
                Usage = ConvertBack(desc.Usage),
                Width = (int)desc.Width
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CpuAccessFlags ConvertBack(Hexa.NET.D3D11.CpuAccessFlag flags)
        {
            if (flags == Hexa.NET.D3D11.CpuAccessFlag.Write)
            {
                return CpuAccessFlags.Write;
            }

            if (flags == Hexa.NET.D3D11.CpuAccessFlag.Read)
            {
                return CpuAccessFlags.Read;
            }

            return CpuAccessFlags.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ResourceMiscFlag ConvertBack(Hexa.NET.D3D11.ResourceMiscFlag flags)
        {
            ResourceMiscFlag result = 0;
            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.GenerateMips) != 0)
            {
                result |= ResourceMiscFlag.GenerateMips;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.Shared) != 0)
            {
                result |= ResourceMiscFlag.Shared;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.Texturecube) != 0)
            {
                result |= ResourceMiscFlag.TextureCube;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.DrawindirectArgs) != 0)
            {
                result |= ResourceMiscFlag.DrawIndirectArguments;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.BufferAllowRawViews) != 0)
            {
                result |= ResourceMiscFlag.BufferAllowRawViews;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.BufferStructured) != 0)
            {
                result |= ResourceMiscFlag.BufferStructured;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.Clamp) != 0)
            {
                result |= ResourceMiscFlag.ResourceClamp;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.SharedKeyedmutex) != 0)
            {
                result |= ResourceMiscFlag.SharedKeyedMutex;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.GdiCompatible) != 0)
            {
                result |= ResourceMiscFlag.GdiCompatible;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.SharedNthandle) != 0)
            {
                result |= ResourceMiscFlag.SharedNTHandle;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.RestrictedContent) != 0)
            {
                result |= ResourceMiscFlag.RestrictedContent;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.RestrictSharedResource) != 0)
            {
                result |= ResourceMiscFlag.RestrictSharedResource;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.RestrictSharedResourceDriver) != 0)
            {
                result |= ResourceMiscFlag.RestrictSharedResourceDriver;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.Guarded) != 0)
            {
                result |= ResourceMiscFlag.Guarded;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.TilePool) != 0)
            {
                result |= ResourceMiscFlag.TilePool;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.Tiled) != 0)
            {
                result |= ResourceMiscFlag.Tiled;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.HwProtected) != 0)
            {
                result |= ResourceMiscFlag.HardwareProtected;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.SharedDisplayable) != 0)
            {
                result |= ResourceMiscFlag.SharedDisplayable;
            }

            if ((flags & Hexa.NET.D3D11.ResourceMiscFlag.SharedExclusiveWriter) != 0)
            {
                result |= ResourceMiscFlag.SharedExclusiveWriter;
            }

            return result;
        }

        private static SampleDescription ConvertBack(Hexa.NET.DXGI.SampleDesc sampleDesc)
        {
            return new()
            {
                Count = (int)sampleDesc.Count,
                Quality = (int)sampleDesc.Quality,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Usage ConvertBack(Hexa.NET.D3D11.Usage usage)
        {
            return usage switch
            {
                Hexa.NET.D3D11.Usage.Default => Usage.Default,
                Hexa.NET.D3D11.Usage.Immutable => Usage.Immutable,
                Hexa.NET.D3D11.Usage.Dynamic => Usage.Dynamic,
                Hexa.NET.D3D11.Usage.Staging => Usage.Staging,
                _ => throw new ArgumentOutOfRangeException(nameof(usage)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BindFlags ConvertBack(Hexa.NET.D3D11.BindFlag flags)
        {
            BindFlags result = 0;
            if ((flags & Hexa.NET.D3D11.BindFlag.VertexBuffer) != 0)
            {
                result |= BindFlags.VertexBuffer;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.IndexBuffer) != 0)
            {
                result |= BindFlags.IndexBuffer;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.ConstantBuffer) != 0)
            {
                result |= BindFlags.ConstantBuffer;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.ShaderResource) != 0)
            {
                result |= BindFlags.ShaderResource;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.StreamOutput) != 0)
            {
                result |= BindFlags.StreamOutput;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.RenderTarget) != 0)
            {
                result |= BindFlags.RenderTarget;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.UnorderedAccess) != 0)
            {
                result |= BindFlags.UnorderedAccess;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.DepthStencil) != 0)
            {
                result |= BindFlags.DepthStencil;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.Decoder) != 0)
            {
                result |= BindFlags.Decoder;
            }

            if ((flags & Hexa.NET.D3D11.BindFlag.VideoEncoder) != 0)
            {
                result |= BindFlags.VideoEncoder;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.SamplerDesc Convert(SamplerStateDescription description)
        {
            Hexa.NET.D3D11.SamplerDesc result = new()
            {
                AddressU = Convert(description.AddressU),
                AddressV = Convert(description.AddressV),
                AddressW = Convert(description.AddressW),
                ComparisonFunc = Convert(description.ComparisonFunction),
                Filter = Convert(description.Filter),
                MaxAnisotropy = (uint)description.MaxAnisotropy,
                MaxLOD = description.MaxLOD,
                MinLOD = description.MinLOD,
                MipLODBias = description.MipLODBias
            };

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Filter Convert(Filter filter)
        {
            return filter switch
            {
                Filter.MinMagMipPoint => Hexa.NET.D3D11.Filter.MinMagMipPoint,
                Filter.MinMagPointMipLinear => Hexa.NET.D3D11.Filter.MinMagPointMipLinear,
                Filter.MinPointMagLinearMipPoint => Hexa.NET.D3D11.Filter.MinPointMagLinearMipPoint,
                Filter.MinPointMagMipLinear => Hexa.NET.D3D11.Filter.MinPointMagMipLinear,
                Filter.MinLinearMagMipPoint => Hexa.NET.D3D11.Filter.MinLinearMagMipPoint,
                Filter.MinLinearMagPointMipLinear => Hexa.NET.D3D11.Filter.MinLinearMagPointMipLinear,
                Filter.MinMagLinearMipPoint => Hexa.NET.D3D11.Filter.MinMagLinearMipPoint,
                Filter.MinMagMipLinear => Hexa.NET.D3D11.Filter.MinMagMipLinear,
                Filter.Anisotropic => Hexa.NET.D3D11.Filter.Anisotropic,
                Filter.ComparisonMinMagMipPoint => Hexa.NET.D3D11.Filter.ComparisonMinMagMipPoint,
                Filter.ComparisonMinMagPointMipLinear => Hexa.NET.D3D11.Filter.ComparisonMinMagPointMipLinear,
                Filter.ComparisonMinPointMagLinearMipPoint => Hexa.NET.D3D11.Filter.ComparisonMinPointMagLinearMipPoint,
                Filter.ComparisonMinPointMagMipLinear => Hexa.NET.D3D11.Filter.ComparisonMinPointMagMipLinear,
                Filter.ComparisonMinLinearMagMipPoint => Hexa.NET.D3D11.Filter.ComparisonMinLinearMagMipPoint,
                Filter.ComparisonMinLinearMagPointMipLinear => Hexa.NET.D3D11.Filter.ComparisonMinLinearMagPointMipLinear,
                Filter.ComparisonMinMagLinearMipPoint => Hexa.NET.D3D11.Filter.ComparisonMinMagLinearMipPoint,
                Filter.ComparisonMinMagMipLinear => Hexa.NET.D3D11.Filter.ComparisonMinMagMipLinear,
                Filter.ComparisonAnisotropic => Hexa.NET.D3D11.Filter.ComparisonAnisotropic,
                Filter.MinimumMinMagMipPoint => Hexa.NET.D3D11.Filter.MinimumMinMagMipPoint,
                Filter.MinimumMinMagPointMipLinear => Hexa.NET.D3D11.Filter.MinimumMinMagPointMipLinear,
                Filter.MinimumMinPointMagLinearMipPoint => Hexa.NET.D3D11.Filter.MinimumMinPointMagLinearMipPoint,
                Filter.MinimumMinPointMagMipLinear => Hexa.NET.D3D11.Filter.MinimumMinPointMagMipLinear,
                Filter.MinimumMinLinearMagMipPoint => Hexa.NET.D3D11.Filter.MinimumMinLinearMagMipPoint,
                Filter.MinimumMinLinearMagPointMipLinear => Hexa.NET.D3D11.Filter.MinimumMinLinearMagPointMipLinear,
                Filter.MinimumMinMagLinearMipPoint => Hexa.NET.D3D11.Filter.MinimumMinMagLinearMipPoint,
                Filter.MinimumMinMagMipLinear => Hexa.NET.D3D11.Filter.MinimumMinMagMipLinear,
                Filter.MinimumAnisotropic => Hexa.NET.D3D11.Filter.MinimumAnisotropic,
                Filter.MaximumMinMagMipPoint => Hexa.NET.D3D11.Filter.MaximumMinMagMipPoint,
                Filter.MaximumMinMagPointMipLinear => Hexa.NET.D3D11.Filter.MaximumMinMagPointMipLinear,
                Filter.MaximumMinPointMagLinearMipPoint => Hexa.NET.D3D11.Filter.MaximumMinPointMagLinearMipPoint,
                Filter.MaximumMinPointMagMipLinear => Hexa.NET.D3D11.Filter.MaximumMinPointMagMipLinear,
                Filter.MaximumMinLinearMagMipPoint => Hexa.NET.D3D11.Filter.MaximumMinLinearMagMipPoint,
                Filter.MaximumMinLinearMagPointMipLinear => Hexa.NET.D3D11.Filter.MaximumMinLinearMagPointMipLinear,
                Filter.MaximumMinMagLinearMipPoint => Hexa.NET.D3D11.Filter.MaximumMinMagLinearMipPoint,
                Filter.MaximumMinMagMipLinear => Hexa.NET.D3D11.Filter.MaximumMinMagMipLinear,
                Filter.MaximumAnisotropic => Hexa.NET.D3D11.Filter.MaximumAnisotropic,
                _ => throw new ArgumentOutOfRangeException(nameof(filter)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.TextureAddressMode Convert(TextureAddressMode address)
        {
            return address switch
            {
                TextureAddressMode.Wrap => Hexa.NET.D3D11.TextureAddressMode.Wrap,
                TextureAddressMode.Mirror => Hexa.NET.D3D11.TextureAddressMode.Mirror,
                TextureAddressMode.Clamp => Hexa.NET.D3D11.TextureAddressMode.Clamp,
                TextureAddressMode.Border => Hexa.NET.D3D11.TextureAddressMode.Border,
                TextureAddressMode.MirrorOnce => Hexa.NET.D3D11.TextureAddressMode.MirrorOnce,
                _ => throw new ArgumentOutOfRangeException(nameof(address)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.RenderTargetViewDesc Convert(RenderTargetViewDescription description)
        {
            return new()
            {
                Union = Convert(description, description.ViewDimension),
                Format = Convert(description.Format),
                ViewDimension = Convert(description.ViewDimension),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.RtvDimension Convert(RenderTargetViewDimension viewDimension)
        {
            return viewDimension switch
            {
                RenderTargetViewDimension.Buffer => Hexa.NET.D3D11.RtvDimension.Buffer,
                RenderTargetViewDimension.Texture1D => Hexa.NET.D3D11.RtvDimension.Texture1D,
                RenderTargetViewDimension.Texture1DArray => Hexa.NET.D3D11.RtvDimension.Texture1Darray,
                RenderTargetViewDimension.Texture2D => Hexa.NET.D3D11.RtvDimension.Texture2D,
                RenderTargetViewDimension.Texture2DArray => Hexa.NET.D3D11.RtvDimension.Texture2Darray,
                RenderTargetViewDimension.Texture2DMultisampled => Hexa.NET.D3D11.RtvDimension.Texture2Dms,
                RenderTargetViewDimension.Texture2DMultisampledArray => Hexa.NET.D3D11.RtvDimension.Texture2Dmsarray,
                RenderTargetViewDimension.Texture3D => Hexa.NET.D3D11.RtvDimension.Texture3D,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion Convert(RenderTargetViewDescription description, RenderTargetViewDimension viewDimension)
        {
            return viewDimension switch
            {
                RenderTargetViewDimension.Buffer => new Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion { Buffer = Convert(description.Buffer) },
                RenderTargetViewDimension.Texture1D => new Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion { Texture1D = Convert(description.Texture1D) },
                RenderTargetViewDimension.Texture1DArray => new Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion { Texture1DArray = Convert(description.Texture1DArray) },
                RenderTargetViewDimension.Texture2D => new Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion { Texture2D = Convert(description.Texture2D) },
                RenderTargetViewDimension.Texture2DArray => new Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion { Texture2DArray = Convert(description.Texture2DArray) },
                RenderTargetViewDimension.Texture2DMultisampled => new Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion { Texture2DMS = Convert(description.Texture2DMS) },
                RenderTargetViewDimension.Texture2DMultisampledArray => new Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion { Texture2DMSArray = Convert(description.Texture2DMSArray) },
                RenderTargetViewDimension.Texture3D => new Hexa.NET.D3D11.RenderTargetViewDesc.RenderTargetViewDescUnion { Texture3D = Convert(description.Texture3D) },
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex3DRtv Convert(Texture3DRenderTargetView texture3D)
        {
            return new Hexa.NET.D3D11.Tex3DRtv((uint)texture3D.MipSlice, (uint)texture3D.FirstWSlice, (uint)texture3D.WSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DmsArrayRtv Convert(Texture2DMultisampledArrayRenderTargetView texture2DMSArray)
        {
            return new Hexa.NET.D3D11.Tex2DmsArrayRtv((uint)texture2DMSArray.FirstArraySlice, (uint)texture2DMSArray.ArraySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DmsRtv Convert(Texture2DMultisampledRenderTargetView texture2DMultisampled)
        {
            return new Hexa.NET.D3D11.Tex2DmsRtv((uint)texture2DMultisampled.UnusedFieldNothingToDefine);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DArrayRtv Convert(Texture2DArrayRenderTargetView texture2DArray)
        {
            return new Hexa.NET.D3D11.Tex2DArrayRtv((uint)texture2DArray.MipSlice, (uint)texture2DArray.FirstArraySlice, (uint)texture2DArray.ArraySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DRtv Convert(Texture2DRenderTargetView texture2D)
        {
            return new Hexa.NET.D3D11.Tex2DRtv((uint)texture2D.MipSlice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex1DArrayRtv Convert(Texture1DArrayRenderTargetView texture1DArray)
        {
            return new Hexa.NET.D3D11.Tex1DArrayRtv((uint)texture1DArray.MipSlice, (uint)texture1DArray.FirstArraySlice, (uint)texture1DArray.ArraySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex1DRtv Convert(Texture1DRenderTargetView texture1D)
        {
            return new Hexa.NET.D3D11.Tex1DRtv((uint)texture1D.MipSlice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.BufferRtv Convert(BufferRenderTargetView buffer)
        {
            return new()
            {
                Union0 = new()
                {
                    ElementOffset = (uint)buffer.ElementOffset,
                    FirstElement = (uint)buffer.FirstElement,
                },
                Union1 = new()
                {
                    ElementWidth = (uint)buffer.ElementWidth,
                    NumElements = (uint)buffer.NumElements,
                }
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.DepthStencilViewDesc Convert(DepthStencilViewDescription description)
        {
            return new()
            {
                Format = Convert(description.Format),
                Union = Convert(description, description.ViewDimension),
                Flags = (uint)Convert(description.Flags),
                ViewDimension = Convert(description.ViewDimension)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.DsvDimension Convert(DepthStencilViewDimension viewDimension)
        {
            return viewDimension switch
            {
                DepthStencilViewDimension.Texture1D => Hexa.NET.D3D11.DsvDimension.Texture1D,
                DepthStencilViewDimension.Texture1DArray => Hexa.NET.D3D11.DsvDimension.Texture1Darray,
                DepthStencilViewDimension.Texture2D => Hexa.NET.D3D11.DsvDimension.Texture2D,
                DepthStencilViewDimension.Texture2DArray => Hexa.NET.D3D11.DsvDimension.Texture2Darray,
                DepthStencilViewDimension.Texture2DMultisampled => Hexa.NET.D3D11.DsvDimension.Texture2Dms,
                DepthStencilViewDimension.Texture2DMultisampledArray => Hexa.NET.D3D11.DsvDimension.Texture2Dmsarray,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.DsvFlag Convert(DepthStencilViewFlags flags)
        {
            Hexa.NET.D3D11.DsvFlag result = 0;
            if (flags == DepthStencilViewFlags.None)
            {
                return 0;
            }

            if ((flags & DepthStencilViewFlags.ReadOnlyDepth) != 0)
            {
                result |= Hexa.NET.D3D11.DsvFlag.ReadOnlyDepth;
            }

            if ((flags & DepthStencilViewFlags.ReadOnlyStencil) != 0)
            {
                result |= Hexa.NET.D3D11.DsvFlag.ReadOnlyStencil;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.DepthStencilViewDesc.DepthStencilViewDescUnion Convert(DepthStencilViewDescription description, DepthStencilViewDimension viewDimension)
        {
            return viewDimension switch
            {
                DepthStencilViewDimension.Texture1D => new Hexa.NET.D3D11.DepthStencilViewDesc.DepthStencilViewDescUnion { Texture1D = Convert(description.Texture1D) },
                DepthStencilViewDimension.Texture1DArray => new Hexa.NET.D3D11.DepthStencilViewDesc.DepthStencilViewDescUnion { Texture1DArray = Convert(description.Texture1DArray) },
                DepthStencilViewDimension.Texture2D => new Hexa.NET.D3D11.DepthStencilViewDesc.DepthStencilViewDescUnion { Texture2D = Convert(description.Texture2D) },
                DepthStencilViewDimension.Texture2DArray => new Hexa.NET.D3D11.DepthStencilViewDesc.DepthStencilViewDescUnion { Texture2DArray = Convert(description.Texture2DArray) },
                DepthStencilViewDimension.Texture2DMultisampled => new Hexa.NET.D3D11.DepthStencilViewDesc.DepthStencilViewDescUnion { Texture2DMS = Convert(description.Texture2DMS) },
                DepthStencilViewDimension.Texture2DMultisampledArray => new Hexa.NET.D3D11.DepthStencilViewDesc.DepthStencilViewDescUnion { Texture2DMSArray = Convert(description.Texture2DMSArray) },
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DmsArrayDsv Convert(Texture2DMultisampledArrayDepthStencilView texture2DMSArray)
        {
            return new Hexa.NET.D3D11.Tex2DmsArrayDsv((uint)texture2DMSArray.FirstArraySlice, (uint)texture2DMSArray.ArraySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DmsDsv Convert(Texture2DMultisampledDepthStencilView texture2DMS)
        {
            return new Hexa.NET.D3D11.Tex2DmsDsv((uint)texture2DMS.UnusedFieldNothingToDefine);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DArrayDsv Convert(Texture2DArrayDepthStencilView texture2DArray)
        {
            return new Hexa.NET.D3D11.Tex2DArrayDsv((uint)texture2DArray.MipSlice, (uint)texture2DArray.FirstArraySlice, (uint)texture2DArray.ArraySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DDsv Convert(Texture2DDepthStencilView texture2D)
        {
            return new Hexa.NET.D3D11.Tex2DDsv((uint)texture2D.MipSlice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex1DArrayDsv Convert(Texture1DArrayDepthStencilView texture1DArray)
        {
            return new Hexa.NET.D3D11.Tex1DArrayDsv((uint)texture1DArray.MipSlice, (uint)texture1DArray.FirstArraySlice, (uint)texture1DArray.ArraySize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex1DDsv Convert(Texture1DDepthStencilView texture1D)
        {
            return new Hexa.NET.D3D11.Tex1DDsv((uint)texture1D.MipSlice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.ShaderResourceViewDesc Convert(ShaderResourceViewDescription description)
        {
            return new()
            {
                Format = Convert(description.Format),
                ViewDimension = Convert(description.ViewDimension),
                Union = Convert(description, description.ViewDimension),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion Convert(ShaderResourceViewDescription description, ShaderResourceViewDimension dimension)
        {
            return dimension switch
            {
                ShaderResourceViewDimension.Buffer => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { Buffer = Convert(description.Buffer) },
                ShaderResourceViewDimension.Texture1D => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { Texture1D = Convert(description.Texture1D) },
                ShaderResourceViewDimension.Texture1DArray => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { Texture1DArray = Convert(description.Texture1DArray) },
                ShaderResourceViewDimension.Texture2D => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { Texture2D = Convert(description.Texture2D) },
                ShaderResourceViewDimension.Texture2DArray => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { Texture2DArray = Convert(description.Texture2DArray) },
                ShaderResourceViewDimension.Texture2DMultisampled => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { Texture2DMS = Convert(description.Texture2DMS) },
                ShaderResourceViewDimension.Texture2DMultisampledArray => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { Texture2DMSArray = Convert(description.Texture2DMSArray) },
                ShaderResourceViewDimension.Texture3D => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { Texture3D = Convert(description.Texture3D) },
                ShaderResourceViewDimension.TextureCube => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { TextureCube = Convert(description.TextureCube) },
                ShaderResourceViewDimension.TextureCubeArray => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { TextureCubeArray = Convert(description.TextureCubeArray) },
                ShaderResourceViewDimension.BufferExtended => new Hexa.NET.D3D11.ShaderResourceViewDesc.ShaderResourceViewDescUnion { BufferEx = Convert(description.BufferEx) },

                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.BufferexSrv Convert(BufferExtendedShaderResourceView bufferEx)
        {
            return new Hexa.NET.D3D11.BufferexSrv((uint)bufferEx.FirstElement, (uint)bufferEx.NumElements, (uint)Convert(bufferEx.Flags));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.BufferexSrvFlag Convert(BufferExtendedShaderResourceViewFlags flags)
        {
            if ((flags & BufferExtendedShaderResourceViewFlags.Raw) != 0)
            {
                return Hexa.NET.D3D11.BufferexSrvFlag.Raw;
            }
            else
            {
                return 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.TexcubeArraySrv Convert(TextureCubeArrayShaderResourceView textureCubeArray)
        {
            return new Hexa.NET.D3D11.TexcubeArraySrv((uint)textureCubeArray.MostDetailedMip, (uint)textureCubeArray.MipLevels, (uint)textureCubeArray.First2DArrayFace, (uint)textureCubeArray.NumCubes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.TexcubeSrv Convert(TextureCubeShaderResourceView textureCube)
        {
            return new Hexa.NET.D3D11.TexcubeSrv((uint)textureCube.MostDetailedMip, (uint)textureCube.MipLevels);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex3DSrv Convert(Texture3DShaderResourceView texture3D)
        {
            return new Hexa.NET.D3D11.Tex3DSrv((uint)texture3D.MostDetailedMip, (uint)texture3D.MipLevels);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DmsArraySrv Convert(Texture2DMultisampledArrayShaderResourceView texture2DMSArray)
        {
            return new()
            {
                ArraySize = (uint)texture2DMSArray.ArraySize,
                FirstArraySlice = (uint)texture2DMSArray.FirstArraySlice
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DmsSrv Convert(Texture2DMultisampledShaderResourceView texture2DMS)
        {
            return new() { UnusedFieldNothingToDefine = (uint)texture2DMS.UnusedFieldNothingToDefine };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DArraySrv Convert(Texture2DArrayShaderResourceView texture2DArray)
        {
            return new()
            {
                ArraySize = (uint)texture2DArray.ArraySize,
                FirstArraySlice = (uint)texture2DArray.FirstArraySlice,
                MipLevels = (uint)texture2DArray.MipLevels,
                MostDetailedMip = (uint)texture2DArray.MostDetailedMip
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex2DSrv Convert(Texture2DShaderResourceView texture2D)
        {
            return new()
            {
                MipLevels = (uint)texture2D.MipLevels,
                MostDetailedMip = (uint)texture2D.MostDetailedMip
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex1DArraySrv Convert(Texture1DArrayShaderResourceView texture1DArray)
        {
            return new()
            {
                ArraySize = (uint)texture1DArray.ArraySize,
                FirstArraySlice = (uint)texture1DArray.FirstArraySlice,
                MipLevels = (uint)texture1DArray.MipLevels,
                MostDetailedMip = (uint)texture1DArray.MostDetailedMip
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Tex1DSrv Convert(Texture1DShaderResourceView texture1D)
        {
            return new()
            {
                MipLevels = (uint)texture1D.MipLevels,
                MostDetailedMip = (uint)texture1D.MostDetailedMip,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.BufferSrv Convert(BufferShaderResourceView buffer)
        {
            return new()
            {
                Union0 = new()
                {
                    ElementOffset = (uint)buffer.ElementOffset,
                    FirstElement = (uint)buffer.FirstElement
                },
                Union1 = new()
                {
                    NumElements = (uint)buffer.NumElements,
                    ElementWidth = (uint)buffer.ElementWidth
                }
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3DCommon.SrvDimension Convert(ShaderResourceViewDimension viewDimension)
        {
            return viewDimension switch
            {
                ShaderResourceViewDimension.Unknown => Hexa.NET.D3DCommon.SrvDimension.Unknown,
                ShaderResourceViewDimension.Buffer => Hexa.NET.D3DCommon.SrvDimension.Buffer,
                ShaderResourceViewDimension.Texture1D => Hexa.NET.D3DCommon.SrvDimension.Texture1D,
                ShaderResourceViewDimension.Texture1DArray => Hexa.NET.D3DCommon.SrvDimension.Texture1Darray,
                ShaderResourceViewDimension.Texture2D => Hexa.NET.D3DCommon.SrvDimension.Texture2D,
                ShaderResourceViewDimension.Texture2DArray => Hexa.NET.D3DCommon.SrvDimension.Texture2Darray,
                ShaderResourceViewDimension.Texture2DMultisampled => Hexa.NET.D3DCommon.SrvDimension.Texture2Dms,
                ShaderResourceViewDimension.Texture2DMultisampledArray => Hexa.NET.D3DCommon.SrvDimension.Texture2Dmsarray,
                ShaderResourceViewDimension.Texture3D => Hexa.NET.D3DCommon.SrvDimension.Texture3D,
                ShaderResourceViewDimension.TextureCube => Hexa.NET.D3DCommon.SrvDimension.Texturecube,
                ShaderResourceViewDimension.TextureCubeArray => Hexa.NET.D3DCommon.SrvDimension.Texturecubearray,
                ShaderResourceViewDimension.BufferExtended => Hexa.NET.D3DCommon.SrvDimension.Bufferex,
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Convert(SubresourceData[] datas, Hexa.NET.D3D11.SubresourceData* subresourceDatas)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                subresourceDatas[i] = Convert(datas[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.SubresourceData Convert(SubresourceData data)
        {
            return new()
            {
                PSysMem = (void*)data.DataPointer,
                SysMemPitch = (uint)data.RowPitch,
                SysMemSlicePitch = (uint)data.SlicePitch,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Texture3DDesc Convert(Texture3DDescription description)
        {
            return new()
            {
                BindFlags = (uint)Convert(description.BindFlags),
                CPUAccessFlags = (uint)Convert(description.CPUAccessFlags),
                Format = Convert(description.Format),
                MipLevels = (uint)description.MipLevels,
                MiscFlags = (uint)Convert(description.MiscFlags),
                Usage = Convert(description.Usage),
                Width = (uint)description.Width,
                Height = (uint)description.Height,
                Depth = (uint)description.Depth
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Texture2DDesc Convert(Texture2DDescription description)
        {
            return new()
            {
                ArraySize = (uint)description.ArraySize,
                BindFlags = (uint)Convert(description.BindFlags),
                CPUAccessFlags = (uint)Convert(description.CPUAccessFlags),
                Format = Convert(description.Format),
                MipLevels = (uint)description.MipLevels,
                MiscFlags = (uint)Convert(description.MiscFlags),
                Usage = Convert(description.Usage),
                Width = (uint)description.Width,
                Height = (uint)description.Height,
                SampleDesc = Convert(description.SampleDescription)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.SampleDesc Convert(SampleDescription sampleDescription)
        {
            return new()
            {
                Count = (uint)sampleDescription.Count,
                Quality = (uint)sampleDescription.Quality
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Texture1DDesc Convert(Texture1DDescription description)
        {
            return new()
            {
                ArraySize = (uint)description.ArraySize,
                BindFlags = (uint)Convert(description.BindFlags),
                CPUAccessFlags = (uint)Convert(description.CPUAccessFlags),
                Format = Convert(description.Format),
                MipLevels = (uint)description.MipLevels,
                MiscFlags = (uint)Convert(description.MiscFlags),
                Usage = Convert(description.Usage),
                Width = (uint)description.Width,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.DepthStencilDesc Convert(DepthStencilDescription description)
        {
            return new()
            {
                DepthEnable = description.DepthEnable,
                DepthFunc = Convert(description.DepthFunc),
                BackFace = Convert(description.BackFace),
                DepthWriteMask = Convert(description.DepthWriteMask),
                FrontFace = Convert(description.FrontFace),
                StencilEnable = description.StencilEnable,
                StencilReadMask = description.StencilReadMask,
                StencilWriteMask = description.StencilWriteMask
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.DepthStencilopDesc Convert(DepthStencilOperationDescription description)
        {
            return new()
            {
                StencilDepthFailOp = Convert(description.StencilDepthFailOp),
                StencilFailOp = Convert(description.StencilFailOp),
                StencilFunc = Convert(description.StencilFunc),
                StencilPassOp = Convert(description.StencilPassOp),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Hexa.NET.D3D11.Query Convert(Query query)
        {
            return query switch
            {
                Query.Event => Hexa.NET.D3D11.Query.Event,
                Query.Occlusion => Hexa.NET.D3D11.Query.Occlusion,
                Query.Timestamp => Hexa.NET.D3D11.Query.Timestamp,
                Query.TimestampDisjoint => Hexa.NET.D3D11.Query.TimestampDisjoint,
                Query.PipelineStatistics => Hexa.NET.D3D11.Query.PipelineStatistics,
                Query.OcclusionPredicate => Hexa.NET.D3D11.Query.OcclusionPredicate,
                Query.SOStatistics => Hexa.NET.D3D11.Query.SoStatistics,
                Query.SOOverflowPredicate => Hexa.NET.D3D11.Query.SoOverflowPredicate,
                Query.SOStatisticsStream0 => Hexa.NET.D3D11.Query.SoStatisticsStream0,
                Query.SOOverflowPredicateStream0 => Hexa.NET.D3D11.Query.SoOverflowPredicateStream0,
                Query.SOStatisticsStream1 => Hexa.NET.D3D11.Query.SoStatisticsStream1,
                Query.SOOverflowPredicateStream1 => Hexa.NET.D3D11.Query.SoOverflowPredicateStream1,
                Query.SOStatisticsStream2 => Hexa.NET.D3D11.Query.SoStatisticsStream2,
                Query.SOOverflowPredicateStream2 => Hexa.NET.D3D11.Query.SoOverflowPredicateStream2,
                Query.SOStatisticsStream3 => Hexa.NET.D3D11.Query.SoStatisticsStream3,
                Query.SOOverflowPredicateStream3 => Hexa.NET.D3D11.Query.SoOverflowPredicateStream3,
                _ => throw new NotSupportedException()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.StencilOp Convert(StencilOperation operation)
        {
            return operation switch
            {
                StencilOperation.Keep => Hexa.NET.D3D11.StencilOp.Keep,
                StencilOperation.Zero => Hexa.NET.D3D11.StencilOp.Zero,
                StencilOperation.Replace => Hexa.NET.D3D11.StencilOp.Replace,
                StencilOperation.IncrementSaturate => Hexa.NET.D3D11.StencilOp.IncrSat,
                StencilOperation.DecrementSaturate => Hexa.NET.D3D11.StencilOp.DecrSat,
                StencilOperation.Invert => Hexa.NET.D3D11.StencilOp.Invert,
                StencilOperation.Increment => Hexa.NET.D3D11.StencilOp.Incr,
                StencilOperation.Decrement => Hexa.NET.D3D11.StencilOp.Decr,
                _ => 0,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.ComparisonFunc Convert(ComparisonFunction function)
        {
            return function switch
            {
                ComparisonFunction.Never => Hexa.NET.D3D11.ComparisonFunc.Never,
                ComparisonFunction.Less => Hexa.NET.D3D11.ComparisonFunc.Less,
                ComparisonFunction.Equal => Hexa.NET.D3D11.ComparisonFunc.Equal,
                ComparisonFunction.LessEqual => Hexa.NET.D3D11.ComparisonFunc.LessEqual,
                ComparisonFunction.Greater => Hexa.NET.D3D11.ComparisonFunc.Greater,
                ComparisonFunction.NotEqual => Hexa.NET.D3D11.ComparisonFunc.NotEqual,
                ComparisonFunction.GreaterEqual => Hexa.NET.D3D11.ComparisonFunc.GreaterEqual,
                ComparisonFunction.Always => Hexa.NET.D3D11.ComparisonFunc.Always,
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.DepthWriteMask Convert(DepthWriteMask mask)
        {
            return mask switch
            {
                DepthWriteMask.Zero => Hexa.NET.D3D11.DepthWriteMask.Zero,
                DepthWriteMask.All => Hexa.NET.D3D11.DepthWriteMask.All,
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.RasterizerDesc2 Convert(RasterizerDescription description)
        {
            return new()
            {
                AntialiasedLineEnable = description.AntialiasedLineEnable,
                CullMode = Convert(description.CullMode),
                DepthBias = description.DepthBias,
                DepthBiasClamp = description.DepthBiasClamp,
                DepthClipEnable = description.DepthClipEnable,
                FillMode = Convert(description.FillMode),
                FrontCounterClockwise = description.FrontCounterClockwise,
                MultisampleEnable = description.MultisampleEnable,
                ScissorEnable = description.ScissorEnable,
                SlopeScaledDepthBias = description.SlopeScaledDepthBias,
                ForcedSampleCount = description.ForcedSampleCount,
                ConservativeRaster = Convert(description.ConservativeRaster)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.ConservativeRasterizationMode Convert(ConservativeRasterizationMode conservativeRaster)
        {
            return conservativeRaster switch
            {
                ConservativeRasterizationMode.Off => Hexa.NET.D3D11.ConservativeRasterizationMode.Off,
                ConservativeRasterizationMode.On => Hexa.NET.D3D11.ConservativeRasterizationMode.On,
                _ => 0,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.FillMode Convert(FillMode mode)
        {
            return mode switch
            {
                FillMode.Solid => Hexa.NET.D3D11.FillMode.Solid,
                FillMode.Wireframe => Hexa.NET.D3D11.FillMode.Wireframe,
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.CullMode Convert(CullMode mode)
        {
            return mode switch
            {
                CullMode.None => Hexa.NET.D3D11.CullMode.None,
                CullMode.Front => Hexa.NET.D3D11.CullMode.Front,
                CullMode.Back => Hexa.NET.D3D11.CullMode.Back,
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.BlendDesc1 Convert(BlendDescription description)
        {
            Hexa.NET.D3D11.BlendDesc1 result = new()
            {
                AlphaToCoverageEnable = description.AlphaToCoverageEnable,
                IndependentBlendEnable = description.IndependentBlendEnable,
            };

            Convert(description.RenderTarget, &result.RenderTarget_0);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Convert(Span<RenderTargetBlendDescription> descriptions, RenderTargetBlendDesc1* output)
        {
            output[0] = Convert(descriptions[0]);
            output[1] = Convert(descriptions[1]);
            output[2] = Convert(descriptions[2]);
            output[3] = Convert(descriptions[3]);
            output[4] = Convert(descriptions[4]);
            output[5] = Convert(descriptions[5]);
            output[6] = Convert(descriptions[6]);
            output[7] = Convert(descriptions[7]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.RenderTargetBlendDesc1 Convert(RenderTargetBlendDescription description)
        {
            return new()
            {
                BlendEnable = description.IsBlendEnabled,
                LogicOpEnable = description.IsLogicOpEnabled,
                BlendOp = Convert(description.BlendOperation),
                BlendOpAlpha = Convert(description.BlendOperationAlpha),
                DestBlend = Convert(description.DestinationBlend),
                DestBlendAlpha = Convert(description.DestinationBlendAlpha),
                SrcBlend = Convert(description.SourceBlend),
                SrcBlendAlpha = Convert(description.SourceBlendAlpha),
                LogicOp = Convert(description.LogicOperation),
                RenderTargetWriteMask = (byte)Convert(description.RenderTargetWriteMask),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.LogicOp Convert(LogicOperation logicOperation)
        {
            return logicOperation switch
            {
                LogicOperation.Clear => Hexa.NET.D3D11.LogicOp.Clear,
                LogicOperation.Set => Hexa.NET.D3D11.LogicOp.Set,
                LogicOperation.Copy => Hexa.NET.D3D11.LogicOp.Copy,
                LogicOperation.CopyInverted => Hexa.NET.D3D11.LogicOp.CopyInverted,
                LogicOperation.Noop => Hexa.NET.D3D11.LogicOp.Noop,
                LogicOperation.Invert => Hexa.NET.D3D11.LogicOp.Invert,
                LogicOperation.And => Hexa.NET.D3D11.LogicOp.And,
                LogicOperation.Nand => Hexa.NET.D3D11.LogicOp.Nand,
                LogicOperation.Or => Hexa.NET.D3D11.LogicOp.Or,
                LogicOperation.Nor => Hexa.NET.D3D11.LogicOp.Nor,
                LogicOperation.Xor => Hexa.NET.D3D11.LogicOp.Xor,
                LogicOperation.Equiv => Hexa.NET.D3D11.LogicOp.Equiv,
                LogicOperation.AndReverse => Hexa.NET.D3D11.LogicOp.AndReverse,
                LogicOperation.AndInverted => Hexa.NET.D3D11.LogicOp.AndInverted,
                LogicOperation.OrReverse => Hexa.NET.D3D11.LogicOp.OrReverse,
                LogicOperation.OrInverted => Hexa.NET.D3D11.LogicOp.OrInverted,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.ColorWriteEnable Convert(ColorWriteEnable flags)
        {
            Hexa.NET.D3D11.ColorWriteEnable result = 0;
            if (flags == ColorWriteEnable.All)
            {
                return Hexa.NET.D3D11.ColorWriteEnable.All;
            }

            if ((flags & ColorWriteEnable.Red) != 0)
            {
                result |= Hexa.NET.D3D11.ColorWriteEnable.Red;
            }

            if ((flags & ColorWriteEnable.Green) != 0)
            {
                result |= Hexa.NET.D3D11.ColorWriteEnable.Green;
            }

            if ((flags & ColorWriteEnable.Blue) != 0)
            {
                result |= Hexa.NET.D3D11.ColorWriteEnable.Blue;
            }

            if ((flags & ColorWriteEnable.Alpha) != 0)
            {
                result |= Hexa.NET.D3D11.ColorWriteEnable.Alpha;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Blend Convert(Blend blend)
        {
            return blend switch
            {
                Blend.Zero => Hexa.NET.D3D11.Blend.Zero,
                Blend.One => Hexa.NET.D3D11.Blend.One,
                Blend.SourceColor => Hexa.NET.D3D11.Blend.SrcColor,
                Blend.InverseSourceColor => Hexa.NET.D3D11.Blend.InvSrcColor,
                Blend.SourceAlpha => Hexa.NET.D3D11.Blend.SrcAlpha,
                Blend.InverseSourceAlpha => Hexa.NET.D3D11.Blend.InvSrcAlpha,
                Blend.DestinationAlpha => Hexa.NET.D3D11.Blend.DestAlpha,
                Blend.InverseDestinationAlpha => Hexa.NET.D3D11.Blend.InvDestAlpha,
                Blend.DestinationColor => Hexa.NET.D3D11.Blend.DestColor,
                Blend.InverseDestinationColor => Hexa.NET.D3D11.Blend.InvDestColor,
                Blend.SourceAlphaSaturate => Hexa.NET.D3D11.Blend.SrcAlphaSat,
                Blend.BlendFactor => Hexa.NET.D3D11.Blend.Factor,
                Blend.InverseBlendFactor => Hexa.NET.D3D11.Blend.InvBlendFactor,
                Blend.Source1Color => Hexa.NET.D3D11.Blend.Src1Color,
                Blend.InverseSource1Color => Hexa.NET.D3D11.Blend.InvSrc1Color,
                Blend.Source1Alpha => Hexa.NET.D3D11.Blend.Src1Alpha,
                Blend.InverseSource1Alpha => Hexa.NET.D3D11.Blend.InvSrc1Alpha,
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.BlendOp Convert(BlendOperation operation)
        {
            return operation switch
            {
                BlendOperation.Add => Hexa.NET.D3D11.BlendOp.Add,
                BlendOperation.Subtract => Hexa.NET.D3D11.BlendOp.Subtract,
                BlendOperation.ReverseSubtract => Hexa.NET.D3D11.BlendOp.RevSubtract,
                BlendOperation.Min => Hexa.NET.D3D11.BlendOp.Min,
                BlendOperation.Max => Hexa.NET.D3D11.BlendOp.Max,
                _ => 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.BufferDesc Convert(BufferDescription description)
        {
            return new()
            {
                BindFlags = (uint)Convert(description.BindFlags),
                ByteWidth = (uint)description.ByteWidth,
                CPUAccessFlags = (uint)Convert(description.CPUAccessFlags),
                MiscFlags = (uint)Convert(description.MiscFlags),
                StructureByteStride = (uint)description.StructureByteStride,
                Usage = Convert(description.Usage)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.CpuAccessFlag Convert(CpuAccessFlags flags)
        {
            Hexa.NET.D3D11.CpuAccessFlag result = 0;
            if ((flags & CpuAccessFlags.Write) != 0)
            {
                result |= Hexa.NET.D3D11.CpuAccessFlag.Write;
            }

            if ((flags & CpuAccessFlags.Read) != 0)
            {
                result |= Hexa.NET.D3D11.CpuAccessFlag.Read;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.ResourceMiscFlag Convert(ResourceMiscFlag flags)
        {
            Hexa.NET.D3D11.ResourceMiscFlag result = 0;
            if ((flags & ResourceMiscFlag.GenerateMips) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.GenerateMips;
            }

            if ((flags & ResourceMiscFlag.Shared) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.Shared;
            }

            if ((flags & ResourceMiscFlag.TextureCube) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.Texturecube;
            }

            if ((flags & ResourceMiscFlag.DrawIndirectArguments) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.DrawindirectArgs;
            }

            if ((flags & ResourceMiscFlag.BufferAllowRawViews) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.BufferAllowRawViews;
            }

            if ((flags & ResourceMiscFlag.BufferStructured) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.BufferStructured;
            }

            if ((flags & ResourceMiscFlag.ResourceClamp) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.Clamp;
            }

            if ((flags & ResourceMiscFlag.SharedKeyedMutex) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.SharedKeyedmutex;
            }

            if ((flags & ResourceMiscFlag.GdiCompatible) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.GdiCompatible;
            }

            if ((flags & ResourceMiscFlag.SharedNTHandle) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.SharedNthandle;
            }

            if ((flags & ResourceMiscFlag.RestrictedContent) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.RestrictedContent;
            }

            if ((flags & ResourceMiscFlag.RestrictSharedResource) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.RestrictSharedResource;
            }

            if ((flags & ResourceMiscFlag.RestrictSharedResourceDriver) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.RestrictSharedResourceDriver;
            }

            if ((flags & ResourceMiscFlag.Guarded) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.Guarded;
            }

            if ((flags & ResourceMiscFlag.TilePool) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.TilePool;
            }

            if ((flags & ResourceMiscFlag.Tiled) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.Tiled;
            }

            if ((flags & ResourceMiscFlag.HardwareProtected) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.HwProtected;
            }

            if ((flags & ResourceMiscFlag.SharedDisplayable) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.SharedDisplayable;
            }

            if ((flags & ResourceMiscFlag.SharedExclusiveWriter) != 0)
            {
                result |= Hexa.NET.D3D11.ResourceMiscFlag.SharedExclusiveWriter;
            }

            if ((flags & ResourceMiscFlag.None) != 0)
            {
                result |= 0;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.Usage Convert(Usage usage)
        {
            return usage switch
            {
                Usage.Default => Hexa.NET.D3D11.Usage.Default,
                Usage.Immutable => Hexa.NET.D3D11.Usage.Immutable,
                Usage.Dynamic => Hexa.NET.D3D11.Usage.Dynamic,
                Usage.Staging => Hexa.NET.D3D11.Usage.Staging,
                _ => throw new ArgumentOutOfRangeException(nameof(usage))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.BindFlag Convert(BindFlags flags)
        {
            Hexa.NET.D3D11.BindFlag result = 0;
            if ((flags & BindFlags.VertexBuffer) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.VertexBuffer;
            }

            if ((flags & BindFlags.IndexBuffer) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.IndexBuffer;
            }

            if ((flags & BindFlags.ConstantBuffer) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.ConstantBuffer;
            }

            if ((flags & BindFlags.ShaderResource) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.ShaderResource;
            }

            if ((flags & BindFlags.StreamOutput) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.StreamOutput;
            }

            if ((flags & BindFlags.RenderTarget) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.RenderTarget;
            }

            if ((flags & BindFlags.DepthStencil) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.DepthStencil;
            }

            if ((flags & BindFlags.UnorderedAccess) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.UnorderedAccess;
            }

            if ((flags & BindFlags.Decoder) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.Decoder;
            }

            if ((flags & BindFlags.VideoEncoder) != 0)
            {
                result |= Hexa.NET.D3D11.BindFlag.VideoEncoder;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Convert(InputElementDescription[] inputElements, Hexa.NET.D3D11.InputElementDesc* descs)
        {
            for (int i = 0; i < inputElements.Length; i++)
            {
                descs[i] = Convert(inputElements[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(Hexa.NET.D3D11.InputElementDesc* descs, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Marshal.FreeHGlobal((nint)descs[i].SemanticName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.InputElementDesc Convert(InputElementDescription description)
        {
            return new()
            {
                AlignedByteOffset = (uint)description.AlignedByteOffset,
                Format = Convert(description.Format),
                InputSlot = (uint)description.Slot,
                InputSlotClass = Convert(description.Classification),
                InstanceDataStepRate = (uint)description.InstanceDataStepRate,
                SemanticIndex = (uint)description.SemanticIndex,
                SemanticName = description.SemanticName.ToBytes()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.InputClassification Convert(InputClassification classification)
        {
            return classification switch
            {
                InputClassification.PerVertexData => Hexa.NET.D3D11.InputClassification.PerVertexData,
                InputClassification.PerInstanceData => Hexa.NET.D3D11.InputClassification.PerInstanceData,
                _ => throw new NotSupportedException()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.Format Convert(Format format)
        {
            return format switch
            {
                Format.Unknown => Hexa.NET.DXGI.Format.Unknown,
                Format.R32G32B32A32Typeless => Hexa.NET.DXGI.Format.R32G32B32A32Typeless,
                Format.R32G32B32A32Float => Hexa.NET.DXGI.Format.R32G32B32A32Float,
                Format.R32G32B32A32UInt => Hexa.NET.DXGI.Format.R32G32B32A32Uint,
                Format.R32G32B32A32SInt => Hexa.NET.DXGI.Format.R32G32B32A32Sint,
                Format.R32G32B32Typeless => Hexa.NET.DXGI.Format.R32G32B32Typeless,
                Format.R32G32B32Float => Hexa.NET.DXGI.Format.R32G32B32Float,
                Format.R32G32B32UInt => Hexa.NET.DXGI.Format.R32G32B32Uint,
                Format.R32G32B32SInt => Hexa.NET.DXGI.Format.R32G32B32Sint,
                Format.R16G16B16A16Typeless => Hexa.NET.DXGI.Format.R16G16B16A16Typeless,
                Format.R16G16B16A16Float => Hexa.NET.DXGI.Format.R16G16B16A16Float,
                Format.R16G16B16A16UNorm => Hexa.NET.DXGI.Format.R16G16B16A16Unorm,
                Format.R16G16B16A16UInt => Hexa.NET.DXGI.Format.R16G16B16A16Uint,
                Format.R16G16B16A16SNorm => Hexa.NET.DXGI.Format.R16G16B16A16Snorm,
                Format.R16G16B16A16Sint => Hexa.NET.DXGI.Format.R16G16B16A16Sint,
                Format.R32G32Typeless => Hexa.NET.DXGI.Format.R32G32Typeless,
                Format.R32G32Float => Hexa.NET.DXGI.Format.R32G32Float,
                Format.R32G32UInt => Hexa.NET.DXGI.Format.R32G32Uint,
                Format.R32G32SInt => Hexa.NET.DXGI.Format.R32G32Sint,
                Format.R32G8X24Typeless => Hexa.NET.DXGI.Format.R32G8X24Typeless,
                Format.D32FloatS8X24UInt => Hexa.NET.DXGI.Format.D32FloatS8X24Uint,
                Format.R32FloatX8X24Typeless => Hexa.NET.DXGI.Format.R32FloatX8X24Typeless,
                Format.X32TypelessG8X24UInt => Hexa.NET.DXGI.Format.X32TypelessG8X24Uint,
                Format.R10G10B10A2Typeless => Hexa.NET.DXGI.Format.R10G10B10A2Typeless,
                Format.R10G10B10A2UNorm => Hexa.NET.DXGI.Format.R10G10B10A2Unorm,
                Format.R10G10B10A2UInt => Hexa.NET.DXGI.Format.R10G10B10A2Uint,
                Format.R11G11B10Float => Hexa.NET.DXGI.Format.R11G11B10Float,
                Format.R8G8B8A8Typeless => Hexa.NET.DXGI.Format.R8G8B8A8Typeless,
                Format.R8G8B8A8UNorm => Hexa.NET.DXGI.Format.R8G8B8A8Unorm,
                Format.R8G8B8A8UNormSRGB => Hexa.NET.DXGI.Format.R8G8B8A8UnormSrgb,
                Format.R8G8B8A8UInt => Hexa.NET.DXGI.Format.R8G8B8A8Uint,
                Format.R8G8B8A8SNorm => Hexa.NET.DXGI.Format.R8G8B8A8Snorm,
                Format.R8G8B8A8SInt => Hexa.NET.DXGI.Format.R8G8B8A8Sint,
                Format.R16G16Typeless => Hexa.NET.DXGI.Format.R16G16Typeless,
                Format.R16G16Float => Hexa.NET.DXGI.Format.R16G16Float,
                Format.R16G16UNorm => Hexa.NET.DXGI.Format.R16G16Unorm,
                Format.R16G16UInt => Hexa.NET.DXGI.Format.R16G16Uint,
                Format.R16G16SNorm => Hexa.NET.DXGI.Format.R16G16Snorm,
                Format.R16G16Sint => Hexa.NET.DXGI.Format.R16G16Sint,
                Format.R32Typeless => Hexa.NET.DXGI.Format.R32Typeless,
                Format.D32Float => Hexa.NET.DXGI.Format.D32Float,
                Format.R32Float => Hexa.NET.DXGI.Format.R32Float,
                Format.R32UInt => Hexa.NET.DXGI.Format.R32Uint,
                Format.R32SInt => Hexa.NET.DXGI.Format.R32Sint,
                Format.R24G8Typeless => Hexa.NET.DXGI.Format.R24G8Typeless,
                Format.D24UNormS8UInt => Hexa.NET.DXGI.Format.D24UnormS8Uint,
                Format.R24UNormX8Typeless => Hexa.NET.DXGI.Format.R24UnormX8Typeless,
                Format.X24TypelessG8UInt => Hexa.NET.DXGI.Format.X24TypelessG8Uint,
                Format.R8G8Typeless => Hexa.NET.DXGI.Format.R8G8Typeless,
                Format.R8G8UNorm => Hexa.NET.DXGI.Format.R8G8Unorm,
                Format.R8G8UInt => Hexa.NET.DXGI.Format.R8G8Uint,
                Format.R8G8SNorm => Hexa.NET.DXGI.Format.R8G8Snorm,
                Format.R8G8Sint => Hexa.NET.DXGI.Format.R8G8Sint,
                Format.R16Typeless => Hexa.NET.DXGI.Format.R16Typeless,
                Format.R16Float => Hexa.NET.DXGI.Format.R16Float,
                Format.D16UNorm => Hexa.NET.DXGI.Format.D16Unorm,
                Format.R16UNorm => Hexa.NET.DXGI.Format.R16Unorm,
                Format.R16UInt => Hexa.NET.DXGI.Format.R16Uint,
                Format.R16SNorm => Hexa.NET.DXGI.Format.R16Snorm,
                Format.R16Sint => Hexa.NET.DXGI.Format.R16Sint,
                Format.R8Typeless => Hexa.NET.DXGI.Format.R8Typeless,
                Format.R8UNorm => Hexa.NET.DXGI.Format.R8Unorm,
                Format.R8UInt => Hexa.NET.DXGI.Format.R8Uint,
                Format.R8SNorm => Hexa.NET.DXGI.Format.R8Snorm,
                Format.R8SInt => Hexa.NET.DXGI.Format.R8Sint,
                Format.A8UNorm => Hexa.NET.DXGI.Format.A8Unorm,
                Format.R1UNorm => Hexa.NET.DXGI.Format.R1Unorm,
                Format.R9G9B9E5SharedExp => Hexa.NET.DXGI.Format.R9G9B9E5Sharedexp,
                Format.R8G8B8G8UNorm => Hexa.NET.DXGI.Format.R8G8B8G8Unorm,
                Format.G8R8G8B8UNorm => Hexa.NET.DXGI.Format.G8R8G8B8Unorm,
                Format.BC1Typeless => Hexa.NET.DXGI.Format.Bc1Typeless,
                Format.BC1UNorm => Hexa.NET.DXGI.Format.Bc1Unorm,
                Format.BC1UNormSRGB => Hexa.NET.DXGI.Format.Bc1UnormSrgb,
                Format.BC2Typeless => Hexa.NET.DXGI.Format.Bc2Typeless,
                Format.BC2UNorm => Hexa.NET.DXGI.Format.Bc2Unorm,
                Format.BC2UNormSRGB => Hexa.NET.DXGI.Format.Bc2UnormSrgb,
                Format.BC3Typeless => Hexa.NET.DXGI.Format.Bc3Typeless,
                Format.BC3UNorm => Hexa.NET.DXGI.Format.Bc3Unorm,
                Format.BC3UNormSRGB => Hexa.NET.DXGI.Format.Bc3UnormSrgb,
                Format.BC4Typeless => Hexa.NET.DXGI.Format.Bc4Typeless,
                Format.BC4UNorm => Hexa.NET.DXGI.Format.Bc4Unorm,
                Format.BC4SNorm => Hexa.NET.DXGI.Format.Bc4Snorm,
                Format.BC5Typeless => Hexa.NET.DXGI.Format.Bc5Typeless,
                Format.BC5UNorm => Hexa.NET.DXGI.Format.Bc5Unorm,
                Format.BC5SNorm => Hexa.NET.DXGI.Format.Bc5Snorm,
                Format.B5G6R5UNorm => Hexa.NET.DXGI.Format.B5G6R5Unorm,
                Format.B5G5R5A1UNorm => Hexa.NET.DXGI.Format.B5G5R5A1Unorm,
                Format.B8G8R8A8UNorm => Hexa.NET.DXGI.Format.B8G8R8A8Unorm,
                Format.B8G8R8X8UNorm => Hexa.NET.DXGI.Format.B8G8R8X8Unorm,
                Format.R10G10B10XRBiasA2UNorm => Hexa.NET.DXGI.Format.R10G10B10XrBiasA2Unorm,
                Format.B8G8R8A8Typeless => Hexa.NET.DXGI.Format.B8G8R8A8Typeless,
                Format.B8G8R8A8UNormSRGB => Hexa.NET.DXGI.Format.B8G8R8A8UnormSrgb,
                Format.B8G8R8X8Typeless => Hexa.NET.DXGI.Format.B8G8R8X8Typeless,
                Format.B8G8R8X8UNormSRGB => Hexa.NET.DXGI.Format.B8G8R8X8UnormSrgb,
                Format.BC6HTypeless => Hexa.NET.DXGI.Format.Bc6HTypeless,
                Format.BC6HUF16 => Hexa.NET.DXGI.Format.Bc6HUf16,
                Format.BC6HSF16 => Hexa.NET.DXGI.Format.Bc6HSf16,
                Format.BC7Typeless => Hexa.NET.DXGI.Format.Bc7Typeless,
                Format.BC7UNorm => Hexa.NET.DXGI.Format.Bc7Unorm,
                Format.BC7UNormSRGB => Hexa.NET.DXGI.Format.Bc7UnormSrgb,
                Format.AYUV => Hexa.NET.DXGI.Format.Ayuv,
                Format.Y410 => Hexa.NET.DXGI.Format.Y410,
                Format.Y416 => Hexa.NET.DXGI.Format.Y416,
                Format.NV12 => Hexa.NET.DXGI.Format.Nv12,
                Format.P010 => Hexa.NET.DXGI.Format.P010,
                Format.P016 => Hexa.NET.DXGI.Format.P016,
                Format.Opaque420 => Hexa.NET.DXGI.Format.Format420Opaque,
                Format.YUY2 => Hexa.NET.DXGI.Format.Yuy2,
                Format.Y210 => Hexa.NET.DXGI.Format.Y210,
                Format.Y216 => Hexa.NET.DXGI.Format.Y216,
                Format.NV11 => Hexa.NET.DXGI.Format.Nv11,
                Format.AI44 => Hexa.NET.DXGI.Format.Ai44,
                Format.IA44 => Hexa.NET.DXGI.Format.Ia44,
                Format.P8 => Hexa.NET.DXGI.Format.P8,
                Format.A8P8 => Hexa.NET.DXGI.Format.A8P8,
                Format.B4G4R4A4UNorm => Hexa.NET.DXGI.Format.B4G4R4A4Unorm,
                Format.P208 => Hexa.NET.DXGI.Format.P208,
                Format.V208 => Hexa.NET.DXGI.Format.V208,
                Format.V408 => Hexa.NET.DXGI.Format.V408,
                Format.SamplerFeedbackMinMipOpaque => Hexa.NET.DXGI.Format.SamplerFeedbackMinMipOpaque,
                Format.SamplerFeedbackMipRegionUsedOpaque => Hexa.NET.DXGI.Format.SamplerFeedbackMipRegionUsedOpaque,
                Format.ForceUInt => Hexa.NET.DXGI.Format.ForceUint,
                _ => Hexa.NET.DXGI.Format.Unknown,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Format ConvertBack(Hexa.NET.DXGI.Format format)
        {
            return format switch
            {
                Hexa.NET.DXGI.Format.Unknown => Format.Unknown,
                Hexa.NET.DXGI.Format.R32G32B32A32Typeless => Format.R32G32B32A32Typeless,
                Hexa.NET.DXGI.Format.R32G32B32A32Float => Format.R32G32B32A32Float,
                Hexa.NET.DXGI.Format.R32G32B32A32Uint => Format.R32G32B32A32UInt,
                Hexa.NET.DXGI.Format.R32G32B32A32Sint => Format.R32G32B32A32SInt,
                Hexa.NET.DXGI.Format.R32G32B32Typeless => Format.R32G32B32Typeless,
                Hexa.NET.DXGI.Format.R32G32B32Float => Format.R32G32B32Float,
                Hexa.NET.DXGI.Format.R32G32B32Uint => Format.R32G32B32UInt,
                Hexa.NET.DXGI.Format.R32G32B32Sint => Format.R32G32B32SInt,
                Hexa.NET.DXGI.Format.R16G16B16A16Typeless => Format.R16G16B16A16Typeless,
                Hexa.NET.DXGI.Format.R16G16B16A16Float => Format.R16G16B16A16Float,
                Hexa.NET.DXGI.Format.R16G16B16A16Unorm => Format.R16G16B16A16UNorm,
                Hexa.NET.DXGI.Format.R16G16B16A16Uint => Format.R16G16B16A16UInt,
                Hexa.NET.DXGI.Format.R16G16B16A16Snorm => Format.R16G16B16A16SNorm,
                Hexa.NET.DXGI.Format.R16G16B16A16Sint => Format.R16G16B16A16Sint,
                Hexa.NET.DXGI.Format.R32G32Typeless => Format.R32G32Typeless,
                Hexa.NET.DXGI.Format.R32G32Float => Format.R32G32Float,
                Hexa.NET.DXGI.Format.R32G32Uint => Format.R32G32UInt,
                Hexa.NET.DXGI.Format.R32G32Sint => Format.R32G32SInt,
                Hexa.NET.DXGI.Format.R32G8X24Typeless => Format.R32G8X24Typeless,
                Hexa.NET.DXGI.Format.D32FloatS8X24Uint => Format.D32FloatS8X24UInt,
                Hexa.NET.DXGI.Format.R32FloatX8X24Typeless => Format.R32FloatX8X24Typeless,
                Hexa.NET.DXGI.Format.X32TypelessG8X24Uint => Format.X32TypelessG8X24UInt,
                Hexa.NET.DXGI.Format.R10G10B10A2Typeless => Format.R10G10B10A2Typeless,
                Hexa.NET.DXGI.Format.R10G10B10A2Unorm => Format.R10G10B10A2UNorm,
                Hexa.NET.DXGI.Format.R10G10B10A2Uint => Format.R10G10B10A2UInt,
                Hexa.NET.DXGI.Format.R11G11B10Float => Format.R11G11B10Float,
                Hexa.NET.DXGI.Format.R8G8B8A8Typeless => Format.R8G8B8A8Typeless,
                Hexa.NET.DXGI.Format.R8G8B8A8Unorm => Format.R8G8B8A8UNorm,
                Hexa.NET.DXGI.Format.R8G8B8A8UnormSrgb => Format.R8G8B8A8UNormSRGB,
                Hexa.NET.DXGI.Format.R8G8B8A8Uint => Format.R8G8B8A8UInt,
                Hexa.NET.DXGI.Format.R8G8B8A8Snorm => Format.R8G8B8A8SNorm,
                Hexa.NET.DXGI.Format.R8G8B8A8Sint => Format.R8G8B8A8SInt,
                Hexa.NET.DXGI.Format.R16G16Typeless => Format.R16G16Typeless,
                Hexa.NET.DXGI.Format.R16G16Float => Format.R16G16Float,
                Hexa.NET.DXGI.Format.R16G16Unorm => Format.R16G16UNorm,
                Hexa.NET.DXGI.Format.R16G16Uint => Format.R16G16UInt,
                Hexa.NET.DXGI.Format.R16G16Snorm => Format.R16G16SNorm,
                Hexa.NET.DXGI.Format.R16G16Sint => Format.R16G16Sint,
                Hexa.NET.DXGI.Format.R32Typeless => Format.R32Typeless,
                Hexa.NET.DXGI.Format.D32Float => Format.D32Float,
                Hexa.NET.DXGI.Format.R32Float => Format.R32Float,
                Hexa.NET.DXGI.Format.R32Uint => Format.R32UInt,
                Hexa.NET.DXGI.Format.R32Sint => Format.R32SInt,
                Hexa.NET.DXGI.Format.R24G8Typeless => Format.R24G8Typeless,
                Hexa.NET.DXGI.Format.D24UnormS8Uint => Format.D24UNormS8UInt,
                Hexa.NET.DXGI.Format.R24UnormX8Typeless => Format.R24UNormX8Typeless,
                Hexa.NET.DXGI.Format.X24TypelessG8Uint => Format.X24TypelessG8UInt,
                Hexa.NET.DXGI.Format.R8G8Typeless => Format.R8G8Typeless,
                Hexa.NET.DXGI.Format.R8G8Unorm => Format.R8G8UNorm,
                Hexa.NET.DXGI.Format.R8G8Uint => Format.R8G8UInt,
                Hexa.NET.DXGI.Format.R8G8Snorm => Format.R8G8SNorm,
                Hexa.NET.DXGI.Format.R8G8Sint => Format.R8G8Sint,
                Hexa.NET.DXGI.Format.R16Typeless => Format.R16Typeless,
                Hexa.NET.DXGI.Format.R16Float => Format.R16Float,
                Hexa.NET.DXGI.Format.D16Unorm => Format.D16UNorm,
                Hexa.NET.DXGI.Format.R16Unorm => Format.R16UNorm,
                Hexa.NET.DXGI.Format.R16Uint => Format.R16UInt,
                Hexa.NET.DXGI.Format.R16Snorm => Format.R16SNorm,
                Hexa.NET.DXGI.Format.R16Sint => Format.R16Sint,
                Hexa.NET.DXGI.Format.R8Typeless => Format.R8Typeless,
                Hexa.NET.DXGI.Format.R8Unorm => Format.R8UNorm,
                Hexa.NET.DXGI.Format.R8Uint => Format.R8UInt,
                Hexa.NET.DXGI.Format.R8Snorm => Format.R8SNorm,
                Hexa.NET.DXGI.Format.R8Sint => Format.R8SInt,
                Hexa.NET.DXGI.Format.A8Unorm => Format.A8UNorm,
                Hexa.NET.DXGI.Format.R1Unorm => Format.R1UNorm,
                Hexa.NET.DXGI.Format.R9G9B9E5Sharedexp => Format.R9G9B9E5SharedExp,
                Hexa.NET.DXGI.Format.R8G8B8G8Unorm => Format.R8G8B8G8UNorm,
                Hexa.NET.DXGI.Format.G8R8G8B8Unorm => Format.G8R8G8B8UNorm,
                Hexa.NET.DXGI.Format.Bc1Typeless => Format.BC1Typeless,
                Hexa.NET.DXGI.Format.Bc1Unorm => Format.BC1UNorm,
                Hexa.NET.DXGI.Format.Bc1UnormSrgb => Format.BC1UNormSRGB,
                Hexa.NET.DXGI.Format.Bc2Typeless => Format.BC2Typeless,
                Hexa.NET.DXGI.Format.Bc2Unorm => Format.BC2UNorm,
                Hexa.NET.DXGI.Format.Bc2UnormSrgb => Format.BC2UNormSRGB,
                Hexa.NET.DXGI.Format.Bc3Typeless => Format.BC3Typeless,
                Hexa.NET.DXGI.Format.Bc3Unorm => Format.BC3UNorm,
                Hexa.NET.DXGI.Format.Bc3UnormSrgb => Format.BC3UNormSRGB,
                Hexa.NET.DXGI.Format.Bc4Typeless => Format.BC4Typeless,
                Hexa.NET.DXGI.Format.Bc4Unorm => Format.BC4UNorm,
                Hexa.NET.DXGI.Format.Bc4Snorm => Format.BC4SNorm,
                Hexa.NET.DXGI.Format.Bc5Typeless => Format.BC5Typeless,
                Hexa.NET.DXGI.Format.Bc5Unorm => Format.BC5UNorm,
                Hexa.NET.DXGI.Format.Bc5Snorm => Format.BC5SNorm,
                Hexa.NET.DXGI.Format.B5G6R5Unorm => Format.B5G6R5UNorm,
                Hexa.NET.DXGI.Format.B5G5R5A1Unorm => Format.B5G5R5A1UNorm,
                Hexa.NET.DXGI.Format.B8G8R8A8Unorm => Format.B8G8R8A8UNorm,
                Hexa.NET.DXGI.Format.B8G8R8X8Unorm => Format.B8G8R8X8UNorm,
                Hexa.NET.DXGI.Format.R10G10B10XrBiasA2Unorm => Format.R10G10B10XRBiasA2UNorm,
                Hexa.NET.DXGI.Format.B8G8R8A8Typeless => Format.B8G8R8A8Typeless,
                Hexa.NET.DXGI.Format.B8G8R8A8UnormSrgb => Format.B8G8R8A8UNormSRGB,
                Hexa.NET.DXGI.Format.B8G8R8X8Typeless => Format.B8G8R8X8Typeless,
                Hexa.NET.DXGI.Format.B8G8R8X8UnormSrgb => Format.B8G8R8X8UNormSRGB,
                Hexa.NET.DXGI.Format.Bc6HTypeless => Format.BC6HTypeless,
                Hexa.NET.DXGI.Format.Bc6HUf16 => Format.BC6HUF16,
                Hexa.NET.DXGI.Format.Bc6HSf16 => Format.BC6HSF16,
                Hexa.NET.DXGI.Format.Bc7Typeless => Format.BC7Typeless,
                Hexa.NET.DXGI.Format.Bc7Unorm => Format.BC7UNorm,
                Hexa.NET.DXGI.Format.Bc7UnormSrgb => Format.BC7UNormSRGB,
                Hexa.NET.DXGI.Format.Ayuv => Format.AYUV,
                Hexa.NET.DXGI.Format.Y410 => Format.Y410,
                Hexa.NET.DXGI.Format.Y416 => Format.Y416,
                Hexa.NET.DXGI.Format.Nv12 => Format.NV12,
                Hexa.NET.DXGI.Format.P010 => Format.P010,
                Hexa.NET.DXGI.Format.P016 => Format.P016,
                Hexa.NET.DXGI.Format.Format420Opaque => Format.Opaque420,
                Hexa.NET.DXGI.Format.Yuy2 => Format.YUY2,
                Hexa.NET.DXGI.Format.Y210 => Format.Y210,
                Hexa.NET.DXGI.Format.Y216 => Format.Y216,
                Hexa.NET.DXGI.Format.Nv11 => Format.NV11,
                Hexa.NET.DXGI.Format.Ai44 => Format.AI44,
                Hexa.NET.DXGI.Format.Ia44 => Format.IA44,
                Hexa.NET.DXGI.Format.P8 => Format.P8,
                Hexa.NET.DXGI.Format.A8P8 => Format.A8P8,
                Hexa.NET.DXGI.Format.B4G4R4A4Unorm => Format.B4G4R4A4UNorm,
                Hexa.NET.DXGI.Format.P208 => Format.P208,
                Hexa.NET.DXGI.Format.V208 => Format.V208,
                Hexa.NET.DXGI.Format.V408 => Format.V408,
                Hexa.NET.DXGI.Format.SamplerFeedbackMinMipOpaque => Format.SamplerFeedbackMinMipOpaque,
                Hexa.NET.DXGI.Format.SamplerFeedbackMipRegionUsedOpaque => Format.SamplerFeedbackMipRegionUsedOpaque,
                Hexa.NET.DXGI.Format.ForceUint => Format.ForceUInt,
                _ => Format.Unknown,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ShaderInputBindDescription Convert(Hexa.NET.D3D11.ShaderInputBindDesc shaderInputDesc)
        {
            return new()
            {
                BindCount = shaderInputDesc.BindCount,
                BindPoint = shaderInputDesc.BindPoint,
                Dimension = Convert(shaderInputDesc.Dimension),
                Name = Utils.ToStr(shaderInputDesc.Name),
                NumSamples = shaderInputDesc.NumSamples,
                ReturnType = Convert(shaderInputDesc.ReturnType),
                Type = Convert(shaderInputDesc.Type),
                UFlags = shaderInputDesc.UFlags
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ShaderInputType Convert(Hexa.NET.D3DCommon.ShaderInputType type)
        {
            return type switch
            {
                Hexa.NET.D3DCommon.ShaderInputType.SitCbuffer => ShaderInputType.SitCBuffer,
                Hexa.NET.D3DCommon.ShaderInputType.SitTbuffer => ShaderInputType.SitTBuffer,
                Hexa.NET.D3DCommon.ShaderInputType.SitTexture => ShaderInputType.SitTexture,
                Hexa.NET.D3DCommon.ShaderInputType.SitSampler => ShaderInputType.SitSampler,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavRwtyped => ShaderInputType.SitUavRwTyped,
                Hexa.NET.D3DCommon.ShaderInputType.SitStructured => ShaderInputType.SitStructured,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavRwstructured => ShaderInputType.SitUavRwStructured,
                Hexa.NET.D3DCommon.ShaderInputType.SitByteaddress => ShaderInputType.SitByteAddress,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavRwbyteaddress => ShaderInputType.SitUavRwByteAddress,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavAppendStructured => ShaderInputType.SitUavAppendStructured,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavConsumeStructured => ShaderInputType.SitUavConsumeStructured,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavRwstructuredWithCounter => ShaderInputType.SitUavRwStructuredWithCounter,
                Hexa.NET.D3DCommon.ShaderInputType.SitRtaccelerationstructure => ShaderInputType.SitRtAccelerationStructure,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavFeedbacktexture => ShaderInputType.SitUavFeedbackTexture,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ResourceReturnType Convert(Hexa.NET.D3DCommon.ResourceReturnType returnType)
        {
            return returnType switch
            {
                0 => ResourceReturnType.None,
                Hexa.NET.D3DCommon.ResourceReturnType.Unorm => ResourceReturnType.UNorm,
                Hexa.NET.D3DCommon.ResourceReturnType.Snorm => ResourceReturnType.SNorm,
                Hexa.NET.D3DCommon.ResourceReturnType.Sint => ResourceReturnType.SInt,
                Hexa.NET.D3DCommon.ResourceReturnType.Uint => ResourceReturnType.UInt,
                Hexa.NET.D3DCommon.ResourceReturnType.Float => ResourceReturnType.Float,
                Hexa.NET.D3DCommon.ResourceReturnType.Mixed => ResourceReturnType.Mixed,
                Hexa.NET.D3DCommon.ResourceReturnType.Double => ResourceReturnType.Double,
                Hexa.NET.D3DCommon.ResourceReturnType.Continued => ResourceReturnType.Continued,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SrvDimension Convert(Hexa.NET.D3DCommon.SrvDimension dimension)
        {
            return dimension switch
            {
                Hexa.NET.D3DCommon.SrvDimension.Unknown => SrvDimension.Unknown,
                Hexa.NET.D3DCommon.SrvDimension.Buffer => SrvDimension.Buffer,
                Hexa.NET.D3DCommon.SrvDimension.Texture1D => SrvDimension.Texture1D,
                Hexa.NET.D3DCommon.SrvDimension.Texture1Darray => SrvDimension.Texture1DArray,
                Hexa.NET.D3DCommon.SrvDimension.Texture2D => SrvDimension.Texture2D,
                Hexa.NET.D3DCommon.SrvDimension.Texture2Darray => SrvDimension.Texture2DArray,
                Hexa.NET.D3DCommon.SrvDimension.Texture2Dms => SrvDimension.Texture2DMS,
                Hexa.NET.D3DCommon.SrvDimension.Texture2Dmsarray => SrvDimension.Texture2DMSArray,
                Hexa.NET.D3DCommon.SrvDimension.Texture3D => SrvDimension.Texture3D,
                Hexa.NET.D3DCommon.SrvDimension.Texturecube => SrvDimension.TextureCube,
                Hexa.NET.D3DCommon.SrvDimension.Texturecubearray => SrvDimension.TextureCubeArray,
                Hexa.NET.D3DCommon.SrvDimension.Bufferex => SrvDimension.BufferEx,
                _ => throw new NotSupportedException()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SignatureParameterDescription Convert(Hexa.NET.D3D11.SignatureParameterDesc shaderInputDesc)
        {
            return new()
            {
                Mask = shaderInputDesc.Mask,
                ComponentType = Convert(shaderInputDesc.ComponentType),
                MinPrecision = Convert(shaderInputDesc.MinPrecision),
                ReadWriteMask = shaderInputDesc.ReadWriteMask,
                Register = shaderInputDesc.Register,
                SemanticIndex = shaderInputDesc.SemanticIndex,
                SemanticName = Utils.ToStr(shaderInputDesc.SemanticName),
                Stream = shaderInputDesc.Stream,
                SystemValueType = Convert(shaderInputDesc.SystemValueType)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Name Convert(Hexa.NET.D3DCommon.Name systemValueType)
        {
            return systemValueType switch
            {
                Hexa.NET.D3DCommon.Name.Undefined => Name.Undefined,
                Hexa.NET.D3DCommon.Name.Position => Name.Position,
                Hexa.NET.D3DCommon.Name.ClipDistance => Name.ClipDistance,
                Hexa.NET.D3DCommon.Name.CullDistance => Name.CullDistance,
                Hexa.NET.D3DCommon.Name.RenderTargetArrayIndex => Name.RenderTargetArrayIndex,
                Hexa.NET.D3DCommon.Name.ViewportArrayIndex => Name.ViewportArrayIndex,
                Hexa.NET.D3DCommon.Name.VertexId => Name.VertexID,
                Hexa.NET.D3DCommon.Name.PrimitiveId => Name.PrimitiveID,
                Hexa.NET.D3DCommon.Name.InstanceId => Name.InstanceID,
                Hexa.NET.D3DCommon.Name.IsFrontFace => Name.IsFrontFace,
                Hexa.NET.D3DCommon.Name.SampleIndex => Name.SampleIndex,
                Hexa.NET.D3DCommon.Name.FinalQuadEdgeTessfactor => Name.FinalQuadEdgeTessfactor,
                Hexa.NET.D3DCommon.Name.FinalQuadInsideTessfactor => Name.FinalQuadInsideTessfactor,
                Hexa.NET.D3DCommon.Name.FinalTriEdgeTessfactor => Name.FinalTriEdgeTessfactor,
                Hexa.NET.D3DCommon.Name.FinalTriInsideTessfactor => Name.FinalTriInsideTessfactor,
                Hexa.NET.D3DCommon.Name.FinalLineDetailTessfactor => Name.FinalLineDetailTessfactor,
                Hexa.NET.D3DCommon.Name.FinalLineDensityTessfactor => Name.FinalLineDensityTessfactor,
                Hexa.NET.D3DCommon.Name.Barycentrics => Name.Barycentrics,
                Hexa.NET.D3DCommon.Name.Shadingrate => Name.Shadingrate,
                Hexa.NET.D3DCommon.Name.Cullprimitive => Name.Cullprimitive,
                Hexa.NET.D3DCommon.Name.Target => Name.Target,
                Hexa.NET.D3DCommon.Name.Depth => Name.Depth,
                Hexa.NET.D3DCommon.Name.Coverage => Name.Coverage,
                Hexa.NET.D3DCommon.Name.DepthGreaterEqual => Name.DepthGreaterEqual,
                Hexa.NET.D3DCommon.Name.DepthLessEqual => Name.DepthLessEqual,
                Hexa.NET.D3DCommon.Name.StencilRef => Name.StencilRef,
                Hexa.NET.D3DCommon.Name.InnerCoverage => Name.InnerCoverage,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MinPrecision Convert(Hexa.NET.D3DCommon.MinPrecision minPrecision)
        {
            return minPrecision switch
            {
                Hexa.NET.D3DCommon.MinPrecision.Default => MinPrecision.Default,
                Hexa.NET.D3DCommon.MinPrecision.Float16 => MinPrecision.Float16,
                Hexa.NET.D3DCommon.MinPrecision.Float28 => MinPrecision.Float28,
                Hexa.NET.D3DCommon.MinPrecision.Reserved => MinPrecision.Reserved,
                Hexa.NET.D3DCommon.MinPrecision.Sint16 => MinPrecision.Sint16,
                Hexa.NET.D3DCommon.MinPrecision.Uint16 => MinPrecision.Uint16,
                Hexa.NET.D3DCommon.MinPrecision.Any16 => MinPrecision.Any16,
                Hexa.NET.D3DCommon.MinPrecision.Any10 => MinPrecision.Any10,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RegisterComponentType Convert(Hexa.NET.D3DCommon.RegisterComponentType componentType)
        {
            return componentType switch
            {
                0 => RegisterComponentType.Unknown,
                Hexa.NET.D3DCommon.RegisterComponentType.Uint32 => RegisterComponentType.Uint32,
                Hexa.NET.D3DCommon.RegisterComponentType.Sint32 => RegisterComponentType.Sint32,
                Hexa.NET.D3DCommon.RegisterComponentType.Float32 => RegisterComponentType.Float32,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Hexa.NET.D3D11.UnorderedAccessViewDesc Convert(UnorderedAccessViewDescription description)
        {
            Hexa.NET.D3D11.UnorderedAccessViewDesc result = new()
            {
                Format = Convert(description.Format),
                ViewDimension = Convert(description.ViewDimension),
                Union = new(),
            };

            switch (description.ViewDimension)
            {
                case UnorderedAccessViewDimension.Unknown:
                    break;

                case UnorderedAccessViewDimension.Buffer:
                    result.Union.Buffer = Convert(description.Buffer);
                    break;

                case UnorderedAccessViewDimension.Texture1D:
                    result.Union.Texture1D = Convert(description.Texture1D);
                    break;

                case UnorderedAccessViewDimension.Texture1DArray:
                    result.Union.Texture1DArray = Convert(description.Texture1DArray);
                    break;

                case UnorderedAccessViewDimension.Texture2D:
                    result.Union.Texture2D = Convert(description.Texture2D);
                    break;

                case UnorderedAccessViewDimension.Texture2DArray:
                    result.Union.Texture2DArray = Convert(description.Texture2DArray);
                    break;

                case UnorderedAccessViewDimension.Texture3D:
                    result.Union.Texture3D = Convert(description.Texture3D);
                    break;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.Tex3DUav Convert(Texture3DUnorderedAccessView texture3D)
        {
            return new()
            {
                FirstWSlice = (uint)texture3D.FirstWSlice,
                MipSlice = (uint)texture3D.MipSlice,
                WSize = (uint)texture3D.WSize,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.Tex2DArrayUav Convert(Texture2DArrayUnorderedAccessView texture2DArray)
        {
            return new()
            {
                ArraySize = (uint)texture2DArray.ArraySize,
                FirstArraySlice = (uint)texture2DArray.FirstArraySlice,
                MipSlice = (uint)texture2DArray.MipSlice,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.Tex2DUav Convert(Texture2DUnorderedAccessView texture2D)
        {
            return new()
            {
                MipSlice = (uint)texture2D.MipSlice
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.Tex1DArrayUav Convert(Texture1DArrayUnorderedAccessView texture1DArray)
        {
            return new()
            {
                ArraySize = (uint)texture1DArray.ArraySize,
                FirstArraySlice = (uint)texture1DArray.FirstArraySlice,
                MipSlice = (uint)texture1DArray.MipSlice
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.Tex1DUav Convert(Texture1DUnorderedAccessView texture1D)
        {
            return new()
            {
                MipSlice = (uint)texture1D.MipSlice
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.BufferUav Convert(BufferUnorderedAccessView buffer)
        {
            return new()
            {
                FirstElement = (uint)buffer.FirstElement,
                Flags = (uint)Convert(buffer.Flags),
                NumElements = (uint)buffer.NumElements,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.BufferUavFlag Convert(BufferUnorderedAccessViewFlags flags)
        {
            return flags switch
            {
                BufferUnorderedAccessViewFlags.Raw => Hexa.NET.D3D11.BufferUavFlag.Raw,
                BufferUnorderedAccessViewFlags.Append => Hexa.NET.D3D11.BufferUavFlag.Append,
                BufferUnorderedAccessViewFlags.Counter => Hexa.NET.D3D11.BufferUavFlag.Counter,
                BufferUnorderedAccessViewFlags.None => 0,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.D3D11.UavDimension Convert(UnorderedAccessViewDimension viewDimension)
        {
            return viewDimension switch
            {
                UnorderedAccessViewDimension.Unknown => Hexa.NET.D3D11.UavDimension.Unknown,
                UnorderedAccessViewDimension.Buffer => Hexa.NET.D3D11.UavDimension.Buffer,
                UnorderedAccessViewDimension.Texture1D => Hexa.NET.D3D11.UavDimension.Texture1D,
                UnorderedAccessViewDimension.Texture1DArray => Hexa.NET.D3D11.UavDimension.Texture1Darray,
                UnorderedAccessViewDimension.Texture2D => Hexa.NET.D3D11.UavDimension.Texture2D,
                UnorderedAccessViewDimension.Texture2DArray => Hexa.NET.D3D11.UavDimension.Texture2Darray,
                UnorderedAccessViewDimension.Texture3D => Hexa.NET.D3D11.UavDimension.Texture3D,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DirectXTex.TexMetadata Convert(TexMetadata metadata)
        {
            Hexa.NET.DirectXTex.TexMetadata texMetadata;
            texMetadata.Format = (int)Convert(metadata.Format);
            texMetadata.ArraySize = (nuint)metadata.ArraySize;
            texMetadata.Width = (nuint)metadata.Width;
            texMetadata.Height = (nuint)metadata.Height;
            texMetadata.Depth = (nuint)metadata.Depth;
            texMetadata.MipLevels = (nuint)metadata.MipLevels;
            texMetadata.Dimension = Convert(metadata.Dimension);
            texMetadata.MiscFlags = (uint)Convert(metadata.MiscFlags);
            texMetadata.MiscFlags2 = 0;
            texMetadata.SetAlphaMode(Convert(metadata.AlphaMode));
            return texMetadata;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DirectXTex.TexAlphaMode Convert(TexAlphaMode mode)
        {
            return mode switch
            {
                TexAlphaMode.Unknown => Hexa.NET.DirectXTex.TexAlphaMode.Unknown,
                TexAlphaMode.Straight => Hexa.NET.DirectXTex.TexAlphaMode.Straight,
                TexAlphaMode.Premultiplied => Hexa.NET.DirectXTex.TexAlphaMode.Premultiplied,
                TexAlphaMode.Opaque => Hexa.NET.DirectXTex.TexAlphaMode.Opaque,
                TexAlphaMode.Custom => Hexa.NET.DirectXTex.TexAlphaMode.Custom,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DirectXTex.TexMiscFlag Convert(TexMiscFlags flags)
        {
            Hexa.NET.DirectXTex.TexMiscFlag result = 0;

            if ((flags & TexMiscFlags.TextureCube) != 0)
            {
                result |= Hexa.NET.DirectXTex.TexMiscFlag.Texturecube;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DirectXTex.TexDimension Convert(TexDimension dimension)
        {
            return dimension switch
            {
                TexDimension.Texture1D => Hexa.NET.DirectXTex.TexDimension.Texture1D,
                TexDimension.Texture2D => Hexa.NET.DirectXTex.TexDimension.Texture2D,
                TexDimension.Texture3D => Hexa.NET.DirectXTex.TexDimension.Texture3D,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TexMetadata ConvertBack(Hexa.NET.DirectXTex.TexMetadata metadata)
        {
            TexMetadata texMetadata;
            texMetadata.Format = ConvertBack((Hexa.NET.DXGI.Format)metadata.Format);
            texMetadata.ArraySize = (int)metadata.ArraySize;
            texMetadata.Width = (int)metadata.Width;
            texMetadata.Height = (int)metadata.Height;
            texMetadata.Depth = (int)metadata.Depth;
            texMetadata.MipLevels = (int)metadata.MipLevels;
            texMetadata.Dimension = ConvertBack(metadata.Dimension);
            texMetadata.MiscFlags = ConvertBack((Hexa.NET.DirectXTex.TexMiscFlag)metadata.MiscFlags);
            texMetadata.AlphaMode = ConvertBack(metadata.GetAlphaMode());
            return texMetadata;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TexDimension ConvertBack(Hexa.NET.DirectXTex.TexDimension dimension)
        {
            return dimension switch
            {
                Hexa.NET.DirectXTex.TexDimension.Texture1D => TexDimension.Texture1D,
                Hexa.NET.DirectXTex.TexDimension.Texture2D => TexDimension.Texture2D,
                Hexa.NET.DirectXTex.TexDimension.Texture3D => TexDimension.Texture3D,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TexMiscFlags ConvertBack(Hexa.NET.DirectXTex.TexMiscFlag flags)
        {
            TexMiscFlags result = 0;

            if ((flags & Hexa.NET.DirectXTex.TexMiscFlag.Texturecube) != 0)
            {
                result |= TexMiscFlags.TextureCube;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TexAlphaMode ConvertBack(Hexa.NET.DirectXTex.TexAlphaMode alphaMode)
        {
            return alphaMode switch
            {
                Hexa.NET.DirectXTex.TexAlphaMode.Unknown => TexAlphaMode.Unknown,
                Hexa.NET.DirectXTex.TexAlphaMode.Straight => TexAlphaMode.Straight,
                Hexa.NET.DirectXTex.TexAlphaMode.Premultiplied => TexAlphaMode.Premultiplied,
                Hexa.NET.DirectXTex.TexAlphaMode.Opaque => TexAlphaMode.Opaque,
                Hexa.NET.DirectXTex.TexAlphaMode.Custom => TexAlphaMode.Custom,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect32 Convert(Rect rect)
        {
            return *(Rect32*)(&rect);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.SwapChainDesc1 Convert(SwapChainDescription swapChainDescription)
        {
            return new()
            {
                BufferCount = swapChainDescription.BufferCount,
                BufferUsage = swapChainDescription.BufferUsage,
                Width = swapChainDescription.Width,
                Height = swapChainDescription.Height,
                Format = Convert(swapChainDescription.Format),
                SampleDesc = Convert(swapChainDescription.SampleDesc),
                Stereo = swapChainDescription.Stereo,
                AlphaMode = Convert(swapChainDescription.AlphaMode),
                SwapEffect = Convert(swapChainDescription.SwapEffect),
                Scaling = Convert(swapChainDescription.Scaling),
                Flags = (uint)Convert(swapChainDescription.Flags)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Hexa.NET.DXGI.SwapChainFlag Convert(SwapChainFlags flags)
        {
            return flags switch
            {
                SwapChainFlags.None => 0,
                SwapChainFlags.Nonprerotated => Hexa.NET.DXGI.SwapChainFlag.Nonprerotated,
                SwapChainFlags.AllowModeSwitch => Hexa.NET.DXGI.SwapChainFlag.AllowModeSwitch,
                SwapChainFlags.GdiCompatible => Hexa.NET.DXGI.SwapChainFlag.GdiCompatible,
                SwapChainFlags.RestrictedContent => Hexa.NET.DXGI.SwapChainFlag.RestrictedContent,
                SwapChainFlags.RestrictSharedResourceDriver => Hexa.NET.DXGI.SwapChainFlag.RestrictSharedResourceDriver,
                SwapChainFlags.DisplayOnly => Hexa.NET.DXGI.SwapChainFlag.DisplayOnly,
                SwapChainFlags.FrameLatencyWaitableObject => Hexa.NET.DXGI.SwapChainFlag.FrameLatencyWaitableObject,
                SwapChainFlags.ForegroundLayer => Hexa.NET.DXGI.SwapChainFlag.ForegroundLayer,
                SwapChainFlags.FullscreenVideo => Hexa.NET.DXGI.SwapChainFlag.FullscreenVideo,
                SwapChainFlags.YuvVideo => Hexa.NET.DXGI.SwapChainFlag.YuvVideo,
                SwapChainFlags.HWProtected => Hexa.NET.DXGI.SwapChainFlag.HwProtected,
                SwapChainFlags.AllowTearing => Hexa.NET.DXGI.SwapChainFlag.AllowTearing,
                SwapChainFlags.RestrictedToAllHolographicDisplays => Hexa.NET.DXGI.SwapChainFlag.RestrictedToAllHolographicDisplays,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.AlphaMode Convert(SwapChainAlphaMode alphaMode)
        {
            return alphaMode switch
            {
                SwapChainAlphaMode.Unspecified => Hexa.NET.DXGI.AlphaMode.Unspecified,
                SwapChainAlphaMode.Premultiplied => Hexa.NET.DXGI.AlphaMode.Premultiplied,
                SwapChainAlphaMode.Straight => Hexa.NET.DXGI.AlphaMode.Straight,
                SwapChainAlphaMode.Ignore => Hexa.NET.DXGI.AlphaMode.Ignore,
                SwapChainAlphaMode.ForceDword => Hexa.NET.DXGI.AlphaMode.ForceDword,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.SwapEffect Convert(SwapEffect swapEffect)
        {
            return swapEffect switch
            {
                SwapEffect.Discard => Hexa.NET.DXGI.SwapEffect.Discard,
                SwapEffect.Sequential => Hexa.NET.DXGI.SwapEffect.Sequential,
                SwapEffect.FlipSequential => Hexa.NET.DXGI.SwapEffect.FlipSequential,
                SwapEffect.FlipDiscard => Hexa.NET.DXGI.SwapEffect.FlipDiscard,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.Scaling Convert(Scaling scaling)
        {
            return scaling switch
            {
                Scaling.Stretch => Hexa.NET.DXGI.Scaling.Stretch,
                Scaling.None => Hexa.NET.DXGI.Scaling.None,
                Scaling.AspectRatioStretch => Hexa.NET.DXGI.Scaling.AspectRatioStretch,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.SwapChainFullscreenDesc Convert(SwapChainFullscreenDescription description)
        {
            return new()
            {
                Windowed = description.Windowed,
                RefreshRate = Convert(description.RefreshRate),
                Scaling = Convert(description.Scaling),
                ScanlineOrdering = Convert(description.ScanlineOrdering)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.Rational Convert(Rational value)
        {
            return new()
            {
                Denominator = value.Denominator,
                Numerator = value.Numerator,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.ModeScaling Convert(ModeScaling scaling)
        {
            return scaling switch
            {
                ModeScaling.Unspecified => Hexa.NET.DXGI.ModeScaling.Unspecified,
                ModeScaling.Centered => Hexa.NET.DXGI.ModeScaling.Centered,
                ModeScaling.Stretched => Hexa.NET.DXGI.ModeScaling.Stretched,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.DXGI.ModeScanlineOrder Convert(ModeScanlineOrder order)
        {
            return order switch
            {
                ModeScanlineOrder.Unspecified => Hexa.NET.DXGI.ModeScanlineOrder.Unspecified,
                ModeScanlineOrder.Progressive => Hexa.NET.DXGI.ModeScanlineOrder.Progressive,
                ModeScanlineOrder.UpperFieldFirst => Hexa.NET.DXGI.ModeScanlineOrder.UpperFieldFirst,
                ModeScanlineOrder.LowerFieldFirst => Hexa.NET.DXGI.ModeScanlineOrder.LowerFieldFirst,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PrimitiveTopology ConvertBack(Hexa.NET.D3DCommon.PrimitiveTopology topology)
        {
            return topology switch
            {
                Hexa.NET.D3DCommon.PrimitiveTopology.Undefined => PrimitiveTopology.Undefined,
                Hexa.NET.D3DCommon.PrimitiveTopology.Pointlist => PrimitiveTopology.PointList,
                Hexa.NET.D3DCommon.PrimitiveTopology.Linelist => PrimitiveTopology.LineList,
                Hexa.NET.D3DCommon.PrimitiveTopology.Linestrip => PrimitiveTopology.LineStrip,
                Hexa.NET.D3DCommon.PrimitiveTopology.Trianglelist => PrimitiveTopology.TriangleList,
                Hexa.NET.D3DCommon.PrimitiveTopology.Trianglestrip => PrimitiveTopology.TriangleStrip,
                Hexa.NET.D3DCommon.PrimitiveTopology.LinelistAdj => PrimitiveTopology.LineListAdjacency,
                Hexa.NET.D3DCommon.PrimitiveTopology.LinestripAdj => PrimitiveTopology.LineStripAdjacency,
                Hexa.NET.D3DCommon.PrimitiveTopology.TrianglelistAdj => PrimitiveTopology.TriangleListAdjacency,
                Hexa.NET.D3DCommon.PrimitiveTopology.TrianglestripAdj => PrimitiveTopology.TriangleStripAdjacency,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology1ControlPointPatchlist => PrimitiveTopology.PatchListWith1ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology2ControlPointPatchlist => PrimitiveTopology.PatchListWith2ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology3ControlPointPatchlist => PrimitiveTopology.PatchListWith3ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology4ControlPointPatchlist => PrimitiveTopology.PatchListWith4ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology5ControlPointPatchlist => PrimitiveTopology.PatchListWith5ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology6ControlPointPatchlist => PrimitiveTopology.PatchListWith6ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology7ControlPointPatchlist => PrimitiveTopology.PatchListWith7ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology8ControlPointPatchlist => PrimitiveTopology.PatchListWith8ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology9ControlPointPatchlist => PrimitiveTopology.PatchListWith9ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology10ControlPointPatchlist => PrimitiveTopology.PatchListWith10ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology11ControlPointPatchlist => PrimitiveTopology.PatchListWith11ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology12ControlPointPatchlist => PrimitiveTopology.PatchListWith12ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology13ControlPointPatchlist => PrimitiveTopology.PatchListWith13ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology14ControlPointPatchlist => PrimitiveTopology.PatchListWith14ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology15ControlPointPatchlist => PrimitiveTopology.PatchListWith15ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology16ControlPointPatchlist => PrimitiveTopology.PatchListWith16ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology17ControlPointPatchlist => PrimitiveTopology.PatchListWith17ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology18ControlPointPatchlist => PrimitiveTopology.PatchListWith18ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology19ControlPointPatchlist => PrimitiveTopology.PatchListWith19ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology20ControlPointPatchlist => PrimitiveTopology.PatchListWith20ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology21ControlPointPatchlist => PrimitiveTopology.PatchListWith21ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology22ControlPointPatchlist => PrimitiveTopology.PatchListWith22ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology23ControlPointPatchlist => PrimitiveTopology.PatchListWith23ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology24ControlPointPatchlist => PrimitiveTopology.PatchListWith24ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology25ControlPointPatchlist => PrimitiveTopology.PatchListWith25ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology26ControlPointPatchlist => PrimitiveTopology.PatchListWith26ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology27ControlPointPatchlist => PrimitiveTopology.PatchListWith27ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology28ControlPointPatchlist => PrimitiveTopology.PatchListWith28ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology29ControlPointPatchlist => PrimitiveTopology.PatchListWith29ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology30ControlPointPatchlist => PrimitiveTopology.PatchListWith30ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology31ControlPointPatchlist => PrimitiveTopology.PatchListWith31ControlPoints,
                Hexa.NET.D3DCommon.PrimitiveTopology.Topology32ControlPointPatchlist => PrimitiveTopology.PatchListWith32ControlPoints,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Primitive ConvertBack(Hexa.NET.D3DCommon.Primitive primitive)
        {
            return primitive switch
            {
                Hexa.NET.D3DCommon.Primitive.Undefined => Primitive.Undefined,
                Hexa.NET.D3DCommon.Primitive.Point => Primitive.Point,
                Hexa.NET.D3DCommon.Primitive.Line => Primitive.Line,
                Hexa.NET.D3DCommon.Primitive.Triangle => Primitive.Triangle,
                Hexa.NET.D3DCommon.Primitive.LineAdj => Primitive.LineAdj,
                Hexa.NET.D3DCommon.Primitive.TriangleAdj => Primitive.TriangleAdj,
                Hexa.NET.D3DCommon.Primitive.Primitive1ControlPointPatch => Primitive.PatchListWith1ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive2ControlPointPatch => Primitive.PatchListWith2ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive3ControlPointPatch => Primitive.PatchListWith3ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive4ControlPointPatch => Primitive.PatchListWith4ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive5ControlPointPatch => Primitive.PatchListWith5ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive6ControlPointPatch => Primitive.PatchListWith6ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive7ControlPointPatch => Primitive.PatchListWith7ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive8ControlPointPatch => Primitive.PatchListWith8ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive9ControlPointPatch => Primitive.PatchListWith9ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive10ControlPointPatch => Primitive.PatchListWith10ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive11ControlPointPatch => Primitive.PatchListWith11ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive12ControlPointPatch => Primitive.PatchListWith12ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive13ControlPointPatch => Primitive.PatchListWith13ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive14ControlPointPatch => Primitive.PatchListWith14ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive15ControlPointPatch => Primitive.PatchListWith15ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive16ControlPointPatch => Primitive.PatchListWith16ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive17ControlPointPatch => Primitive.PatchListWith17ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive18ControlPointPatch => Primitive.PatchListWith18ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive19ControlPointPatch => Primitive.PatchListWith19ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive20ControlPointPatch => Primitive.PatchListWith20ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive21ControlPointPatch => Primitive.PatchListWith21ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive22ControlPointPatch => Primitive.PatchListWith22ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive23ControlPointPatch => Primitive.PatchListWith23ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive24ControlPointPatch => Primitive.PatchListWith24ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive25ControlPointPatch => Primitive.PatchListWith25ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive26ControlPointPatch => Primitive.PatchListWith26ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive27ControlPointPatch => Primitive.PatchListWith27ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive28ControlPointPatch => Primitive.PatchListWith28ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive29ControlPointPatch => Primitive.PatchListWith29ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive30ControlPointPatch => Primitive.PatchListWith30ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive31ControlPointPatch => Primitive.PatchListWith31ControlPoint,
                Hexa.NET.D3DCommon.Primitive.Primitive32ControlPointPatch => Primitive.PatchListWith32ControlPoint,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TessellatorOutputPrimitive ConvertBack(Hexa.NET.D3DCommon.TessellatorOutputPrimitive tessellatorOutputPrimitive)
        {
            return tessellatorOutputPrimitive switch
            {
                Hexa.NET.D3DCommon.TessellatorOutputPrimitive.Undefined => TessellatorOutputPrimitive.Undefined,
                Hexa.NET.D3DCommon.TessellatorOutputPrimitive.Point => TessellatorOutputPrimitive.Point,
                Hexa.NET.D3DCommon.TessellatorOutputPrimitive.Line => TessellatorOutputPrimitive.Line,
                Hexa.NET.D3DCommon.TessellatorOutputPrimitive.TriangleCw => TessellatorOutputPrimitive.TriangleCW,
                Hexa.NET.D3DCommon.TessellatorOutputPrimitive.TriangleCcw => TessellatorOutputPrimitive.TriangleCcw,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TessellatorPartitioning ConvertBack(Hexa.NET.D3DCommon.TessellatorPartitioning tessellatorPartitioning)
        {
            return tessellatorPartitioning switch
            {
                Hexa.NET.D3DCommon.TessellatorPartitioning.Undefined => TessellatorPartitioning.Undefined,
                Hexa.NET.D3DCommon.TessellatorPartitioning.Integer => TessellatorPartitioning.Integer,
                Hexa.NET.D3DCommon.TessellatorPartitioning.Pow2 => TessellatorPartitioning.Pow2,
                Hexa.NET.D3DCommon.TessellatorPartitioning.FractionalOdd => TessellatorPartitioning.FractionalOdd,
                Hexa.NET.D3DCommon.TessellatorPartitioning.FractionalEven => TessellatorPartitioning.FractionalEven,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TessellatorDomain ConvertBack(Hexa.NET.D3DCommon.TessellatorDomain tessellatorDomain)
        {
            return tessellatorDomain switch
            {
                Hexa.NET.D3DCommon.TessellatorDomain.Undefined => TessellatorDomain.Undefined,
                Hexa.NET.D3DCommon.TessellatorDomain.Isoline => TessellatorDomain.Isoline,
                Hexa.NET.D3DCommon.TessellatorDomain.Tri => TessellatorDomain.Tri,
                Hexa.NET.D3DCommon.TessellatorDomain.Quad => TessellatorDomain.Quad,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ShaderBufferDesc ConvertBack(Hexa.NET.D3D11.ShaderBufferDesc shaderBufferDesc)
        {
            return new()
            {
                Name = ToStringFromUTF8(shaderBufferDesc.Name) ?? string.Empty,
                Type = ConvertBack(shaderBufferDesc.Type),
                Variables = new ShaderVariableDesc[shaderBufferDesc.Variables],
                Size = shaderBufferDesc.Size,
                UFlags = shaderBufferDesc.UFlags,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CBufferType ConvertBack(Hexa.NET.D3DCommon.CbufferType type)
        {
            return type switch
            {
                Hexa.NET.D3DCommon.CbufferType.CtCbuffer => CBufferType.CBuffer,
                Hexa.NET.D3DCommon.CbufferType.CtTbuffer => CBufferType.TBuffer,
                Hexa.NET.D3DCommon.CbufferType.CtInterfacePointers => CBufferType.InterfacePointers,
                Hexa.NET.D3DCommon.CbufferType.CtResourceBindInfo => CBufferType.ResourceBindInfo,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ShaderInputBindDescription ConvertBack(Hexa.NET.D3D11.ShaderInputBindDesc shaderInputBindDesc)
        {
            return new()
            {
                BindCount = shaderInputBindDesc.BindCount,
                BindPoint = shaderInputBindDesc.BindPoint,
                Dimension = ConvertBack(shaderInputBindDesc.Dimension),
                Name = ToStringFromUTF8(shaderInputBindDesc.Name) ?? string.Empty,
                NumSamples = shaderInputBindDesc.NumSamples,
                ReturnType = ConvertBack(shaderInputBindDesc.ReturnType),
                Type = ConvertBack(shaderInputBindDesc.Type),
                UFlags = shaderInputBindDesc.UFlags,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ShaderInputType ConvertBack(Hexa.NET.D3DCommon.ShaderInputType type)
        {
            return type switch
            {
                Hexa.NET.D3DCommon.ShaderInputType.SitCbuffer => ShaderInputType.SitCBuffer,
                Hexa.NET.D3DCommon.ShaderInputType.SitTbuffer => ShaderInputType.SitTBuffer,
                Hexa.NET.D3DCommon.ShaderInputType.SitTexture => ShaderInputType.SitTexture,
                Hexa.NET.D3DCommon.ShaderInputType.SitSampler => ShaderInputType.SitSampler,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavRwtyped => ShaderInputType.SitUavRwTyped,
                Hexa.NET.D3DCommon.ShaderInputType.SitStructured => ShaderInputType.SitStructured,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavRwstructured => ShaderInputType.SitUavRwStructured,
                Hexa.NET.D3DCommon.ShaderInputType.SitByteaddress => ShaderInputType.SitByteAddress,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavRwbyteaddress => ShaderInputType.SitUavRwByteAddress,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavAppendStructured => ShaderInputType.SitUavAppendStructured,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavConsumeStructured => ShaderInputType.SitUavConsumeStructured,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavRwstructuredWithCounter => ShaderInputType.SitUavRwStructuredWithCounter,
                Hexa.NET.D3DCommon.ShaderInputType.SitRtaccelerationstructure => ShaderInputType.SitRtAccelerationStructure,
                Hexa.NET.D3DCommon.ShaderInputType.SitUavFeedbacktexture => ShaderInputType.SitUavFeedbackTexture,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResourceReturnType ConvertBack(Hexa.NET.D3DCommon.ResourceReturnType returnType)
        {
            return returnType switch
            {
                0 => ResourceReturnType.None,
                Hexa.NET.D3DCommon.ResourceReturnType.Unorm => ResourceReturnType.UNorm,
                Hexa.NET.D3DCommon.ResourceReturnType.Snorm => ResourceReturnType.SNorm,
                Hexa.NET.D3DCommon.ResourceReturnType.Sint => ResourceReturnType.SInt,
                Hexa.NET.D3DCommon.ResourceReturnType.Uint => ResourceReturnType.UInt,
                Hexa.NET.D3DCommon.ResourceReturnType.Float => ResourceReturnType.Float,
                Hexa.NET.D3DCommon.ResourceReturnType.Mixed => ResourceReturnType.Mixed,
                Hexa.NET.D3DCommon.ResourceReturnType.Double => ResourceReturnType.Double,
                Hexa.NET.D3DCommon.ResourceReturnType.Continued => ResourceReturnType.Continued,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SrvDimension ConvertBack(Hexa.NET.D3DCommon.SrvDimension dimension)
        {
            return dimension switch
            {
                Hexa.NET.D3DCommon.SrvDimension.Unknown => SrvDimension.Unknown,
                Hexa.NET.D3DCommon.SrvDimension.Buffer => SrvDimension.Buffer,
                Hexa.NET.D3DCommon.SrvDimension.Texture1D => SrvDimension.Texture1D,
                Hexa.NET.D3DCommon.SrvDimension.Texture1Darray => SrvDimension.Texture1DArray,
                Hexa.NET.D3DCommon.SrvDimension.Texture2D => SrvDimension.Texture2D,
                Hexa.NET.D3DCommon.SrvDimension.Texture2Darray => SrvDimension.Texture2DArray,
                Hexa.NET.D3DCommon.SrvDimension.Texture2Dms => SrvDimension.Texture2DMS,
                Hexa.NET.D3DCommon.SrvDimension.Texture2Dmsarray => SrvDimension.Texture2DMSArray,
                Hexa.NET.D3DCommon.SrvDimension.Texture3D => SrvDimension.Texture3D,
                Hexa.NET.D3DCommon.SrvDimension.Texturecube => SrvDimension.TextureCube,
                Hexa.NET.D3DCommon.SrvDimension.Texturecubearray => SrvDimension.TextureCubeArray,
                Hexa.NET.D3DCommon.SrvDimension.Bufferex => SrvDimension.BufferEx,
                _ => throw new NotSupportedException(),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ShaderVariableDesc ConvertBack(Hexa.NET.D3D11.ShaderVariableDesc shaderVariableDesc)
        {
            return new()
            {
                DefaultValue = shaderVariableDesc.DefaultValue,
                Name = ToStringFromUTF8(shaderVariableDesc.Name) ?? string.Empty,
                SamplerSize = shaderVariableDesc.SamplerSize,
                Size = shaderVariableDesc.Size,
                StartOffset = shaderVariableDesc.StartOffset,
                StartSampler = shaderVariableDesc.StartSampler,
                StartTexture = shaderVariableDesc.StartTexture,
                TextureSize = shaderVariableDesc.TextureSize,
                UFlags = shaderVariableDesc.UFlags,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hexa.NET.D3D11.FenceFlag Convert(FenceFlags flags)
        {
            return flags switch
            {
                FenceFlags.None => Hexa.NET.D3D11.FenceFlag.None,
                FenceFlags.Shared => Hexa.NET.D3D11.FenceFlag.Shared,
                FenceFlags.SharedCrossAdapter => Hexa.NET.D3D11.FenceFlag.SharedCrossAdapter,
                FenceFlags.NonMonitored => Hexa.NET.D3D11.FenceFlag.NonMonitored,
                _ => throw new NotSupportedException(),
            };
        }

        internal static void Convert(CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, out Usage usage, out BindFlags bindFlags)
        {
            usage = 0;
            bindFlags = 0;

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (cpuAccessFlags != CpuAccessFlags.None && (gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot use rw with uva at the same time");
            }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ShaderResourceViewDescription ConvertBack(ShaderResourceViewDesc desc)
        {
            var result = new ShaderResourceViewDescription()
            {
                Format = ConvertBack(desc.Format),
            };

            switch (desc.ViewDimension)
            {
                case Hexa.NET.D3DCommon.SrvDimension.Buffer:
                    result.ViewDimension = ShaderResourceViewDimension.Buffer;
                    result.Buffer = new()
                    {
                        ElementOffset = (int)desc.Union.Buffer.Union0.ElementOffset,
                        FirstElement = (int)desc.Union.Buffer.Union0.FirstElement,
                        ElementWidth = (int)desc.Union.Buffer.Union1.ElementWidth,
                        NumElements = (int)desc.Union.Buffer.Union1.NumElements
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texture1D:
                    result.ViewDimension = ShaderResourceViewDimension.Texture1D;
                    result.Texture1D = new()
                    {
                        MipLevels = (int)desc.Union.Texture1D.MipLevels,
                        MostDetailedMip = (int)desc.Union.Texture1D.MostDetailedMip
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texture1Darray:
                    result.ViewDimension = ShaderResourceViewDimension.Texture1DArray;
                    result.Texture1DArray = new()
                    {
                        ArraySize = (int)desc.Union.Texture1DArray.ArraySize,
                        FirstArraySlice = (int)desc.Union.Texture1DArray.FirstArraySlice,
                        MipLevels = (int)desc.Union.Texture1DArray.MipLevels,
                        MostDetailedMip = (int)desc.Union.Texture1DArray.MostDetailedMip
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texture2D:
                    result.ViewDimension = ShaderResourceViewDimension.Texture2D;
                    result.Texture2D = new()
                    {
                        MipLevels = (int)desc.Union.Texture2D.MipLevels,
                        MostDetailedMip = (int)desc.Union.Texture2D.MostDetailedMip
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texture2Darray:
                    result.ViewDimension = ShaderResourceViewDimension.Texture2DArray;
                    result.Texture2DArray = new()
                    {
                        ArraySize = (int)desc.Union.Texture2DArray.ArraySize,
                        FirstArraySlice = (int)desc.Union.Texture2DArray.FirstArraySlice,
                        MipLevels = (int)desc.Union.Texture2DArray.MipLevels,
                        MostDetailedMip = (int)desc.Union.Texture2DArray.MostDetailedMip
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texture2Dms:
                    result.ViewDimension = ShaderResourceViewDimension.Texture2DMultisampled;
                    result.Texture2DMS = new()
                    {
                        UnusedFieldNothingToDefine = (int)desc.Union.Texture2DMS.UnusedFieldNothingToDefine
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texture2Dmsarray:
                    result.ViewDimension = ShaderResourceViewDimension.Texture2DMultisampledArray;
                    result.Texture2DMSArray = new()
                    {
                        ArraySize = (int)desc.Union.Texture2DMSArray.ArraySize,
                        FirstArraySlice = (int)desc.Union.Texture2DMSArray.FirstArraySlice
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texture3D:
                    result.ViewDimension = ShaderResourceViewDimension.Texture3D;
                    result.Texture3D = new()
                    {
                        MipLevels = (int)desc.Union.TextureCube.MipLevels,
                        MostDetailedMip = (int)desc.Union.TextureCube.MostDetailedMip
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texturecube:
                    result.ViewDimension = ShaderResourceViewDimension.TextureCube;
                    result.TextureCube = new()
                    {
                        MipLevels = (int)desc.Union.TextureCube.MipLevels,
                        MostDetailedMip = (int)desc.Union.TextureCube.MostDetailedMip
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Texturecubearray:
                    result.ViewDimension = ShaderResourceViewDimension.TextureCubeArray;
                    result.TextureCubeArray = new()
                    {
                        First2DArrayFace = (int)desc.Union.TextureCubeArray.First2DArrayFace,
                        MipLevels = (int)desc.Union.TextureCubeArray.MipLevels,
                        MostDetailedMip = (int)desc.Union.TextureCubeArray.MostDetailedMip,
                        NumCubes = (int)desc.Union.TextureCubeArray.NumCubes
                    };
                    break;

                case Hexa.NET.D3DCommon.SrvDimension.Bufferex:
                    result.ViewDimension = ShaderResourceViewDimension.BufferExtended;
                    result.BufferEx = new()
                    {
                        FirstElement = (int)desc.Union.BufferEx.FirstElement,
                        Flags = ConvertBack((BufferexSrvFlag)desc.Union.BufferEx.Flags),
                        NumElements = (int)desc.Union.BufferEx.NumElements,
                    };
                    break;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BufferExtendedShaderResourceViewFlags ConvertBack(BufferexSrvFlag flags)
        {
            BufferExtendedShaderResourceViewFlags result = 0;
            if ((flags & BufferexSrvFlag.Raw) != 0)
            {
                result |= BufferExtendedShaderResourceViewFlags.Raw;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderTargetViewDescription ConvertBack(RenderTargetViewDesc desc)
        {
            RenderTargetViewDescription result = new()
            {
                Format = ConvertBack(desc.Format)
            };

            switch (desc.ViewDimension)
            {
                case RtvDimension.Buffer:
                    result.ViewDimension = RenderTargetViewDimension.Buffer;
                    result.Buffer = new()
                    {
                        ElementOffset = (int)desc.Union.Buffer.Union0.ElementOffset,
                        FirstElement = (int)desc.Union.Buffer.Union0.FirstElement,
                        ElementWidth = (int)desc.Union.Buffer.Union1.ElementWidth,
                        NumElements = (int)desc.Union.Buffer.Union1.NumElements,
                    };
                    break;

                case RtvDimension.Texture1D:
                    result.ViewDimension = RenderTargetViewDimension.Texture1D;
                    result.Texture1D = new()
                    {
                        MipSlice = (int)desc.Union.Texture1D.MipSlice,
                    };
                    break;

                case RtvDimension.Texture1Darray:
                    result.ViewDimension = RenderTargetViewDimension.Texture1DArray;
                    result.Texture1DArray = new()
                    {
                        ArraySize = (int)desc.Union.Texture1DArray.ArraySize,
                        FirstArraySlice = (int)desc.Union.Texture1DArray.FirstArraySlice,
                        MipSlice = (int)desc.Union.Texture1DArray.MipSlice,
                    };
                    break;

                case RtvDimension.Texture2D:
                    result.ViewDimension = RenderTargetViewDimension.Texture2D;
                    result.Texture2D = new()
                    {
                        MipSlice = (int)desc.Union.Texture2D.MipSlice,
                    };
                    break;

                case RtvDimension.Texture2Darray:
                    result.ViewDimension = RenderTargetViewDimension.Texture2DArray;
                    result.Texture2DArray = new()
                    {
                        ArraySize = (int)desc.Union.Texture2DArray.ArraySize,
                        FirstArraySlice = (int)desc.Union.Texture2DArray.FirstArraySlice,
                        MipSlice = (int)desc.Union.Texture2DArray.MipSlice,
                    };
                    break;

                case RtvDimension.Texture2Dms:
                    result.ViewDimension = RenderTargetViewDimension.Texture2DMultisampled;
                    result.Texture2DMS = new()
                    {
                        UnusedFieldNothingToDefine = (int)desc.Union.Texture2DMS.UnusedFieldNothingToDefine,
                    };
                    break;

                case RtvDimension.Texture2Dmsarray:
                    result.ViewDimension = RenderTargetViewDimension.Texture2DMultisampledArray;
                    result.Texture2DMSArray = new()
                    {
                        ArraySize = (int)desc.Union.Texture2DMSArray.ArraySize,
                        FirstArraySlice = (int)desc.Union.Texture2DMSArray.FirstArraySlice,
                    };
                    break;

                case RtvDimension.Texture3D:
                    result.ViewDimension = RenderTargetViewDimension.Texture3D;
                    result.Texture3D = new()
                    {
                        FirstWSlice = (int)desc.Union.Texture3D.FirstWSlice,
                        MipSlice = (int)desc.Union.Texture3D.MipSlice,
                        WSize = (int)desc.Union.Texture3D.WSize,
                    };
                    break;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RenderTargetViewDimension ConvertBack(RtvDimension viewDimension)
        {
            return viewDimension switch
            {
                RtvDimension.Unknown => RenderTargetViewDimension.Unknown,
                RtvDimension.Buffer => RenderTargetViewDimension.Buffer,
                RtvDimension.Texture1D => RenderTargetViewDimension.Texture1D,
                RtvDimension.Texture1Darray => RenderTargetViewDimension.Texture1DArray,
                RtvDimension.Texture2D => RenderTargetViewDimension.Texture2D,
                RtvDimension.Texture2Darray => RenderTargetViewDimension.Texture2DArray,
                RtvDimension.Texture2Dms => RenderTargetViewDimension.Texture2DMultisampled,
                RtvDimension.Texture2Dmsarray => RenderTargetViewDimension.Texture2DMultisampled,
                RtvDimension.Texture3D => RenderTargetViewDimension.Texture3D,
                _ => throw new NotSupportedException()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DepthStencilViewDescription ConvertBack(DepthStencilViewDesc desc)
        {
            DepthStencilViewDescription result = default;

            result.Format = ConvertBack(desc.Format);

            switch (desc.ViewDimension)
            {
                case DsvDimension.Unknown:
                    break;

                case DsvDimension.Texture1D:
                    result.ViewDimension = DepthStencilViewDimension.Texture1D;
                    result.Texture1D = new()
                    {
                        MipSlice = (int)desc.Union.Texture1D.MipSlice
                    };
                    break;

                case DsvDimension.Texture1Darray:
                    result.ViewDimension = DepthStencilViewDimension.Texture1DArray;
                    result.Texture1DArray = new()
                    {
                        MipSlice = (int)desc.Union.Texture1DArray.MipSlice,
                        FirstArraySlice = (int)desc.Union.Texture1DArray.FirstArraySlice,
                        ArraySize = (int)desc.Union.Texture1DArray.ArraySize,
                    };
                    break;

                case DsvDimension.Texture2D:
                    result.ViewDimension = DepthStencilViewDimension.Texture2D;
                    result.Texture2D = new()
                    {
                        MipSlice = (int)desc.Union.Texture2D.MipSlice,
                    };
                    break;

                case DsvDimension.Texture2Darray:
                    result.ViewDimension = DepthStencilViewDimension.Texture2DArray;
                    result.Texture2DArray = new()
                    {
                        MipSlice = (int)desc.Union.Texture2DArray.MipSlice,
                        FirstArraySlice = (int)desc.Union.Texture2DArray.FirstArraySlice,
                        ArraySize = (int)desc.Union.Texture2DArray.ArraySize,
                    };
                    break;

                case DsvDimension.Texture2Dms:
                    result.ViewDimension = DepthStencilViewDimension.Texture2DMultisampled;
                    result.Texture2DMS = new()
                    {
                        UnusedFieldNothingToDefine = (int)desc.Union.Texture2DMS.UnusedFieldNothingToDefine,
                    };
                    break;

                case DsvDimension.Texture2Dmsarray:
                    result.ViewDimension = DepthStencilViewDimension.Texture2DMultisampledArray;
                    result.Texture2DMSArray = new()
                    {
                        FirstArraySlice = (int)desc.Union.Texture2DMSArray.FirstArraySlice,
                        ArraySize = (int)desc.Union.Texture2DMSArray.ArraySize,
                    };
                    break;
            }

            return result;
        }

        public static ColorSpaceType Convert(ColorSpace colorSpace)
        {
            return colorSpace switch
            {
                ColorSpace.RGBFullG22NoneP709 => ColorSpaceType.RgbFullG22NoneP709,
                ColorSpace.RGBFullG2084NoneP2020 => ColorSpaceType.RgbFullG2084NoneP2020,
                _ => throw new NotSupportedException(),
            };
        }
    }
}