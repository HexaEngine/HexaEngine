﻿namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public unsafe class Helper
    {
        public static Silk.NET.Direct3D11.Viewport Convert(Viewport viewport)
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

        public static uint PackUNorm(float bitmask, float value)
        {
            value *= bitmask;
            return (uint)ClampAndRound(value, 0.0f, bitmask);
        }

        public static uint PackRGBA(float x, float y, float z, float w)
        {
            uint red = PackUNorm(255.0f, x);
            uint green = PackUNorm(255.0f, y) << 8;
            uint blue = PackUNorm(255.0f, z) << 16;
            uint alpha = PackUNorm(255.0f, w) << 24;
            return red | green | blue | alpha;
        }

        public static uint PackRGBA(Vector4 color) => PackRGBA(color.X, color.Y, color.Z, color.W);

        public static Silk.NET.Core.Native.D3DPrimitiveTopology Convert(PrimitiveTopology topology)
        {
            return topology switch
            {
                PrimitiveTopology.Undefined => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyUndefined,
                PrimitiveTopology.PointList => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyPointlist,
                PrimitiveTopology.LineList => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyLinelist,
                PrimitiveTopology.LineStrip => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyLinestrip,
                PrimitiveTopology.TriangleList => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist,
                PrimitiveTopology.TriangleStrip => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglestrip,
                PrimitiveTopology.LineListAdjacency => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyLinelistAdj,
                PrimitiveTopology.LineStripAdjacency => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyLinestripAdj,
                PrimitiveTopology.TriangleListAdjacency => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelistAdj,
                PrimitiveTopology.TriangleStripAdjacency => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglestripAdj,
                PrimitiveTopology.PatchListWith1ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology1ControlPointPatchlist,
                PrimitiveTopology.PatchListWith2ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology2ControlPointPatchlist,
                PrimitiveTopology.PatchListWith3ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology3ControlPointPatchlist,
                PrimitiveTopology.PatchListWith4ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology4ControlPointPatchlist,
                PrimitiveTopology.PatchListWith5ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology5ControlPointPatchlist,
                PrimitiveTopology.PatchListWith6ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology6ControlPointPatchlist,
                PrimitiveTopology.PatchListWith7ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology7ControlPointPatchlist,
                PrimitiveTopology.PatchListWith8ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology8ControlPointPatchlist,
                PrimitiveTopology.PatchListWith9ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology9ControlPointPatchlist,
                PrimitiveTopology.PatchListWith10ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology10ControlPointPatchlist,
                PrimitiveTopology.PatchListWith11ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology11ControlPointPatchlist,
                PrimitiveTopology.PatchListWith12ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology12ControlPointPatchlist,
                PrimitiveTopology.PatchListWith13ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology13ControlPointPatchlist,
                PrimitiveTopology.PatchListWith14ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology14ControlPointPatchlist,
                PrimitiveTopology.PatchListWith15ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology15ControlPointPatchlist,
                PrimitiveTopology.PatchListWith16ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology16ControlPointPatchlist,
                PrimitiveTopology.PatchListWith17ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology17ControlPointPatchlist,
                PrimitiveTopology.PatchListWith18ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology18ControlPointPatchlist,
                PrimitiveTopology.PatchListWith19ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology19ControlPointPatchlist,
                PrimitiveTopology.PatchListWith20ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology20ControlPointPatchlist,
                PrimitiveTopology.PatchListWith21ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology21ControlPointPatchlist,
                PrimitiveTopology.PatchListWith22ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology22ControlPointPatchlist,
                PrimitiveTopology.PatchListWith23ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology23ControlPointPatchlist,
                PrimitiveTopology.PatchListWith24ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology24ControlPointPatchlist,
                PrimitiveTopology.PatchListWith25ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology25ControlPointPatchlist,
                PrimitiveTopology.PatchListWith26ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology26ControlPointPatchlist,
                PrimitiveTopology.PatchListWith27ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology27ControlPointPatchlist,
                PrimitiveTopology.PatchListWith28ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology28ControlPointPatchlist,
                PrimitiveTopology.PatchListWith29ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology29ControlPointPatchlist,
                PrimitiveTopology.PatchListWith30ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology30ControlPointPatchlist,
                PrimitiveTopology.PatchListWith31ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology31ControlPointPatchlist,
                PrimitiveTopology.PatchListWith32ControlPoints => Silk.NET.Core.Native.D3DPrimitiveTopology.D3DPrimitiveTopology32ControlPointPatchlist,
                _ => throw new ArgumentOutOfRangeException(nameof(topology)),
            };
        }

        public static Silk.NET.Direct3D11.MapFlag Convert(MapFlags flags)
        {
            return flags switch
            {
                MapFlags.DoNotWait => Silk.NET.Direct3D11.MapFlag.MapFlagDONotWait,
                MapFlags.None => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(flags)),
            };
        }

        public static Silk.NET.Direct3D11.Map Convert(MapMode mode)
        {
            return mode switch
            {
                MapMode.Read => Silk.NET.Direct3D11.Map.MapRead,
                MapMode.Write => Silk.NET.Direct3D11.Map.MapWrite,
                MapMode.ReadWrite => Silk.NET.Direct3D11.Map.MapReadWrite,
                MapMode.WriteDiscard => Silk.NET.Direct3D11.Map.MapWriteDiscard,
                MapMode.WriteNoOverwrite => Silk.NET.Direct3D11.Map.MapWriteNoOverwrite,
                _ => throw new ArgumentOutOfRangeException(nameof(mode)),
            };
        }

        public static Silk.NET.Direct3D11.ClearFlag Convert(DepthStencilClearFlags flags)
        {
            return flags switch
            {
                DepthStencilClearFlags.None => 0,
                DepthStencilClearFlags.Depth => Silk.NET.Direct3D11.ClearFlag.ClearDepth,
                DepthStencilClearFlags.Stencil => Silk.NET.Direct3D11.ClearFlag.ClearStencil,
                DepthStencilClearFlags.All => Silk.NET.Direct3D11.ClearFlag.ClearDepth | Silk.NET.Direct3D11.ClearFlag.ClearStencil,
                _ => throw new ArgumentOutOfRangeException(nameof(flags)),
            };
        }

        public static Texture2DDescription ConvertBack(Silk.NET.Direct3D11.Texture2DDesc desc)
        {
            return new()
            {
                Format = ConvertBack(desc.Format),
                ArraySize = (int)desc.ArraySize,
                BindFlags = ConvertBack((Silk.NET.Direct3D11.BindFlag)desc.BindFlags),
                CPUAccessFlags = ConvertBack((Silk.NET.Direct3D11.CpuAccessFlag)desc.CPUAccessFlags),
                Height = (int)desc.Height,
                MipLevels = (int)desc.MipLevels,
                MiscFlags = ConvertBack((Silk.NET.Direct3D11.ResourceMiscFlag)desc.MiscFlags),
                SampleDescription = ConvertBack(desc.SampleDesc),
                Usage = ConvertBack(desc.Usage),
                Width = (int)desc.Width
            };
        }

        private static CpuAccessFlags ConvertBack(Silk.NET.Direct3D11.CpuAccessFlag flags)
        {
            if (flags == Silk.NET.Direct3D11.CpuAccessFlag.CpuAccessWrite)
                return CpuAccessFlags.Write;
            if (flags == Silk.NET.Direct3D11.CpuAccessFlag.CpuAccessRead)
                return CpuAccessFlags.Read;
            return CpuAccessFlags.None;
        }

        private static ResourceMiscFlag ConvertBack(Silk.NET.Direct3D11.ResourceMiscFlag flags)
        {
            ResourceMiscFlag result = 0;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscGenerateMips)) result |= ResourceMiscFlag.GenerateMips;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscShared)) result |= ResourceMiscFlag.Shared;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscTexturecube)) result |= ResourceMiscFlag.TextureCube;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscDrawindirectArgs)) result |= ResourceMiscFlag.DrawIndirectArguments;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscBufferAllowRawViews)) result |= ResourceMiscFlag.BufferAllowRawViews;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscBufferStructured)) result |= ResourceMiscFlag.BufferStructured;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscResourceClamp)) result |= ResourceMiscFlag.ResourceClamp;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscSharedKeyedmutex)) result |= ResourceMiscFlag.SharedKeyedMutex;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscGdiCompatible)) result |= ResourceMiscFlag.GdiCompatible;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscSharedNthandle)) result |= ResourceMiscFlag.SharedNTHandle;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscRestrictedContent)) result |= ResourceMiscFlag.RestrictedContent;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscRestrictSharedResource)) result |= ResourceMiscFlag.RestrictSharedResource;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscRestrictSharedResourceDriver)) result |= ResourceMiscFlag.RestrictSharedResourceDriver;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscGuarded)) result |= ResourceMiscFlag.Guarded;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscTilePool)) result |= ResourceMiscFlag.TilePool;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscTiled)) result |= ResourceMiscFlag.Tiled;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscHWProtected)) result |= ResourceMiscFlag.HardwareProtected;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscSharedDisplayable)) result |= ResourceMiscFlag.SharedDisplayable;
            if (flags.HasFlag(Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscSharedExclusiveWriter)) result |= ResourceMiscFlag.SharedExclusiveWriter;
            return result;
        }

        private static SampleDescription ConvertBack(Silk.NET.DXGI.SampleDesc sampleDesc)
        {
            return new()
            {
                Count = (int)sampleDesc.Count,
                Quality = (int)sampleDesc.Quality,
            };
        }

        private static Usage ConvertBack(Silk.NET.Direct3D11.Usage usage)
        {
            return usage switch
            {
                Silk.NET.Direct3D11.Usage.UsageDefault => Usage.Default,
                Silk.NET.Direct3D11.Usage.UsageImmutable => Usage.Immutable,
                Silk.NET.Direct3D11.Usage.UsageDynamic => Usage.Dynamic,
                Silk.NET.Direct3D11.Usage.UsageStaging => Usage.Staging,
                _ => throw new ArgumentOutOfRangeException(nameof(usage)),
            };
        }

        private static BindFlags ConvertBack(Silk.NET.Direct3D11.BindFlag flags)
        {
            BindFlags result = 0;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindVertexBuffer))
                result |= BindFlags.VertexBuffer;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindIndexBuffer))
                result |= BindFlags.IndexBuffer;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindConstantBuffer))
                result |= BindFlags.ConstantBuffer;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindShaderResource))
                result |= BindFlags.ShaderResource;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindStreamOutput))
                result |= BindFlags.StreamOutput;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindRenderTarget))
                result |= BindFlags.RenderTarget;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindUnorderedAccess))
                result |= BindFlags.UnorderedAccess;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindDepthStencil))
                result |= BindFlags.DepthStencil;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindDecoder))
                result |= BindFlags.Decoder;
            if (flags.HasFlag(Silk.NET.Direct3D11.BindFlag.BindVideoEncoder))
                result |= BindFlags.VideoEncoder;
            return result;
        }

        public static Silk.NET.Direct3D11.SamplerDesc Convert(SamplerDescription description)
        {
            Silk.NET.Direct3D11.SamplerDesc result = new()
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

        public static Silk.NET.Direct3D11.Filter Convert(Filter filter)
        {
            return filter switch
            {
                Filter.MinMagMipPoint => Silk.NET.Direct3D11.Filter.FilterMinMagMipPoint,
                Filter.MinMagPointMipLinear => Silk.NET.Direct3D11.Filter.FilterMinMagPointMipLinear,
                Filter.MinPointMagLinearMipPoint => Silk.NET.Direct3D11.Filter.FilterMinPointMagLinearMipPoint,
                Filter.MinPointMagMipLinear => Silk.NET.Direct3D11.Filter.FilterMinPointMagMipLinear,
                Filter.MinLinearMagMipPoint => Silk.NET.Direct3D11.Filter.FilterMinLinearMagMipPoint,
                Filter.MinLinearMagPointMipLinear => Silk.NET.Direct3D11.Filter.FilterMinLinearMagPointMipLinear,
                Filter.MinMagLinearMipPoint => Silk.NET.Direct3D11.Filter.FilterMinMagLinearMipPoint,
                Filter.MinMagMipLinear => Silk.NET.Direct3D11.Filter.FilterMinMagMipLinear,
                Filter.Anisotropic => Silk.NET.Direct3D11.Filter.FilterAnisotropic,
                Filter.ComparisonMinMagMipPoint => Silk.NET.Direct3D11.Filter.FilterComparisonMinMagMipPoint,
                Filter.ComparisonMinMagPointMipLinear => Silk.NET.Direct3D11.Filter.FilterComparisonMinMagPointMipLinear,
                Filter.ComparisonMinPointMagLinearMipPoint => Silk.NET.Direct3D11.Filter.FilterComparisonMinPointMagLinearMipPoint,
                Filter.ComparisonMinPointMagMipLinear => Silk.NET.Direct3D11.Filter.FilterComparisonMinPointMagMipLinear,
                Filter.ComparisonMinLinearMagMipPoint => Silk.NET.Direct3D11.Filter.FilterComparisonMinLinearMagMipPoint,
                Filter.ComparisonMinLinearMagPointMipLinear => Silk.NET.Direct3D11.Filter.FilterComparisonMinLinearMagPointMipLinear,
                Filter.ComparisonMinMagLinearMipPoint => Silk.NET.Direct3D11.Filter.FilterComparisonMinMagLinearMipPoint,
                Filter.ComparisonMinMagMipLinear => Silk.NET.Direct3D11.Filter.FilterComparisonMinMagMipLinear,
                Filter.ComparisonAnisotropic => Silk.NET.Direct3D11.Filter.FilterComparisonAnisotropic,
                Filter.MinimumMinMagMipPoint => Silk.NET.Direct3D11.Filter.FilterMinimumMinMagMipPoint,
                Filter.MinimumMinMagPointMipLinear => Silk.NET.Direct3D11.Filter.FilterMinimumMinMagPointMipLinear,
                Filter.MinimumMinPointMagLinearMipPoint => Silk.NET.Direct3D11.Filter.FilterMinimumMinPointMagLinearMipPoint,
                Filter.MinimumMinPointMagMipLinear => Silk.NET.Direct3D11.Filter.FilterMinimumMinPointMagMipLinear,
                Filter.MinimumMinLinearMagMipPoint => Silk.NET.Direct3D11.Filter.FilterMinimumMinLinearMagMipPoint,
                Filter.MinimumMinLinearMagPointMipLinear => Silk.NET.Direct3D11.Filter.FilterMinimumMinLinearMagPointMipLinear,
                Filter.MinimumMinMagLinearMipPoint => Silk.NET.Direct3D11.Filter.FilterMinimumMinMagLinearMipPoint,
                Filter.MinimumMinMagMipLinear => Silk.NET.Direct3D11.Filter.FilterMinimumMinMagMipLinear,
                Filter.MinimumAnisotropic => Silk.NET.Direct3D11.Filter.FilterMinimumAnisotropic,
                Filter.MaximumMinMagMipPoint => Silk.NET.Direct3D11.Filter.FilterMaximumMinMagMipPoint,
                Filter.MaximumMinMagPointMipLinear => Silk.NET.Direct3D11.Filter.FilterMaximumMinMagPointMipLinear,
                Filter.MaximumMinPointMagLinearMipPoint => Silk.NET.Direct3D11.Filter.FilterMaximumMinPointMagLinearMipPoint,
                Filter.MaximumMinPointMagMipLinear => Silk.NET.Direct3D11.Filter.FilterMaximumMinPointMagMipLinear,
                Filter.MaximumMinLinearMagMipPoint => Silk.NET.Direct3D11.Filter.FilterMaximumMinLinearMagMipPoint,
                Filter.MaximumMinLinearMagPointMipLinear => Silk.NET.Direct3D11.Filter.FilterMaximumMinLinearMagPointMipLinear,
                Filter.MaximumMinMagLinearMipPoint => Silk.NET.Direct3D11.Filter.FilterMaximumMinMagLinearMipPoint,
                Filter.MaximumMinMagMipLinear => Silk.NET.Direct3D11.Filter.FilterMaximumMinMagMipLinear,
                Filter.MaximumAnisotropic => Silk.NET.Direct3D11.Filter.FilterMaximumAnisotropic,
                _ => throw new ArgumentOutOfRangeException(nameof(filter)),
            };
        }

        public static Silk.NET.Direct3D11.TextureAddressMode Convert(TextureAddressMode address)
        {
            return address switch
            {
                TextureAddressMode.Wrap => Silk.NET.Direct3D11.TextureAddressMode.TextureAddressWrap,
                TextureAddressMode.Mirror => Silk.NET.Direct3D11.TextureAddressMode.TextureAddressMirror,
                TextureAddressMode.Clamp => Silk.NET.Direct3D11.TextureAddressMode.TextureAddressClamp,
                TextureAddressMode.Border => Silk.NET.Direct3D11.TextureAddressMode.TextureAddressBorder,
                TextureAddressMode.MirrorOnce => Silk.NET.Direct3D11.TextureAddressMode.TextureAddressMirrorOnce,
                _ => throw new ArgumentOutOfRangeException(nameof(address)),
            };
        }

        public static Silk.NET.Direct3D11.RenderTargetViewDesc Convert(RenderTargetViewDescription description)
        {
            return new()
            {
                Anonymous = Convert(description, description.ViewDimension),
                Format = Convert(description.Format),
                ViewDimension = Convert(description.ViewDimension),
            };
        }

        public static Silk.NET.Direct3D11.RtvDimension Convert(RenderTargetViewDimension viewDimension)
        {
            return viewDimension switch
            {
                RenderTargetViewDimension.Buffer => Silk.NET.Direct3D11.RtvDimension.RtvDimensionBuffer,
                RenderTargetViewDimension.Texture1D => Silk.NET.Direct3D11.RtvDimension.RtvDimensionTexture1D,
                RenderTargetViewDimension.Texture1DArray => Silk.NET.Direct3D11.RtvDimension.RtvDimensionTexture1Darray,
                RenderTargetViewDimension.Texture2D => Silk.NET.Direct3D11.RtvDimension.RtvDimensionTexture2D,
                RenderTargetViewDimension.Texture2DArray => Silk.NET.Direct3D11.RtvDimension.RtvDimensionTexture2Darray,
                RenderTargetViewDimension.Texture2DMultisampled => Silk.NET.Direct3D11.RtvDimension.RtvDimensionTexture2Dms,
                RenderTargetViewDimension.Texture2DMultisampledArray => Silk.NET.Direct3D11.RtvDimension.RtvDimensionTexture2Dmsarray,
                RenderTargetViewDimension.Texture3D => Silk.NET.Direct3D11.RtvDimension.RtvDimensionTexture3D,
                _ => throw new NotImplementedException(),
            };
        }

        public static Silk.NET.Direct3D11.RenderTargetViewDescUnion Convert(RenderTargetViewDescription description, RenderTargetViewDimension viewDimension)
        {
            return viewDimension switch
            {
                RenderTargetViewDimension.Buffer => new Silk.NET.Direct3D11.RenderTargetViewDescUnion(buffer: Convert(description.Buffer)),
                RenderTargetViewDimension.Texture1D => new Silk.NET.Direct3D11.RenderTargetViewDescUnion(texture1D: Convert(description.Texture1D)),
                RenderTargetViewDimension.Texture1DArray => new Silk.NET.Direct3D11.RenderTargetViewDescUnion(texture1DArray: Convert(description.Texture1DArray)),
                RenderTargetViewDimension.Texture2D => new Silk.NET.Direct3D11.RenderTargetViewDescUnion(texture2D: Convert(description.Texture2D)),
                RenderTargetViewDimension.Texture2DArray => new Silk.NET.Direct3D11.RenderTargetViewDescUnion(texture2DArray: Convert(description.Texture2DArray)),
                RenderTargetViewDimension.Texture2DMultisampled => new Silk.NET.Direct3D11.RenderTargetViewDescUnion(texture2DMS: Convert(description.Texture2DMS)),
                RenderTargetViewDimension.Texture2DMultisampledArray => new Silk.NET.Direct3D11.RenderTargetViewDescUnion(texture2DMSArray: Convert(description.Texture2DMSArray)),
                RenderTargetViewDimension.Texture3D => new Silk.NET.Direct3D11.RenderTargetViewDescUnion(texture3D: Convert(description.Texture3D)),
                _ => throw new NotImplementedException(),
            };
        }

        public static Silk.NET.Direct3D11.Tex3DRtv Convert(Texture3DRenderTargetView texture3D)
        {
            return new Silk.NET.Direct3D11.Tex3DRtv((uint)texture3D.MipSlice, (uint)texture3D.FirstWSlice, (uint)texture3D.WSize);
        }

        public static Silk.NET.Direct3D11.Tex2DmsArrayRtv Convert(Texture2DMultisampledArrayRenderTargetView texture2DMSArray)
        {
            return new Silk.NET.Direct3D11.Tex2DmsArrayRtv((uint)texture2DMSArray.FirstArraySlice, (uint)texture2DMSArray.ArraySize);
        }

        public static Silk.NET.Direct3D11.Tex2DmsRtv Convert(Texture2DMultisampledRenderTargetView texture2DMultisampled)
        {
            return new Silk.NET.Direct3D11.Tex2DmsRtv((uint)texture2DMultisampled.UnusedFieldNothingToDefine);
        }

        public static Silk.NET.Direct3D11.Tex2DArrayRtv Convert(Texture2DArrayRenderTargetView texture2DArray)
        {
            return new Silk.NET.Direct3D11.Tex2DArrayRtv((uint)texture2DArray.MipSlice, (uint)texture2DArray.FirstArraySlice, (uint)texture2DArray.ArraySize);
        }

        public static Silk.NET.Direct3D11.Tex2DRtv Convert(Texture2DRenderTargetView texture2D)
        {
            return new Silk.NET.Direct3D11.Tex2DRtv((uint)texture2D.MipSlice);
        }

        public static Silk.NET.Direct3D11.Tex1DArrayRtv Convert(Texture1DArrayRenderTargetView texture1DArray)
        {
            return new Silk.NET.Direct3D11.Tex1DArrayRtv((uint)texture1DArray.MipSlice, (uint)texture1DArray.FirstArraySlice, (uint)texture1DArray.ArraySize);
        }

        public static Silk.NET.Direct3D11.Tex1DRtv Convert(Texture1DRenderTargetView texture1D)
        {
            return new Silk.NET.Direct3D11.Tex1DRtv((uint)texture1D.MipSlice);
        }

        public static Silk.NET.Direct3D11.BufferRtv Convert(BufferRenderTargetView buffer)
        {
            return new()
            {
                Anonymous1 = new()
                {
                    ElementOffset = (uint)buffer.ElementOffset,
                    FirstElement = (uint)buffer.FirstElement,
                },
                Anonymous2 = new()
                {
                    ElementWidth = (uint)buffer.ElementWidth,
                    NumElements = (uint)buffer.NumElements,
                }
            };
        }

        public static Silk.NET.Direct3D11.DepthStencilViewDesc Convert(DepthStencilViewDescription description)
        {
            return new()
            {
                Format = Convert(description.Format),
                Anonymous = Convert(description, description.ViewDimension),
                Flags = (uint)Convert(description.Flags),
                ViewDimension = Convert(description.ViewDimension)
            };
        }

        public static Silk.NET.Direct3D11.DsvDimension Convert(DepthStencilViewDimension viewDimension)
        {
            return viewDimension switch
            {
                DepthStencilViewDimension.Texture1D => Silk.NET.Direct3D11.DsvDimension.DsvDimensionTexture1D,
                DepthStencilViewDimension.Texture1DArray => Silk.NET.Direct3D11.DsvDimension.DsvDimensionTexture1Darray,
                DepthStencilViewDimension.Texture2D => Silk.NET.Direct3D11.DsvDimension.DsvDimensionTexture2D,
                DepthStencilViewDimension.Texture2DArray => Silk.NET.Direct3D11.DsvDimension.DsvDimensionTexture2Darray,
                DepthStencilViewDimension.Texture2DMultisampled => Silk.NET.Direct3D11.DsvDimension.DsvDimensionTexture2Dms,
                DepthStencilViewDimension.Texture2DMultisampledArray => Silk.NET.Direct3D11.DsvDimension.DsvDimensionTexture2Dmsarray,
                _ => throw new NotImplementedException(),
            };
        }

        public static Silk.NET.Direct3D11.DsvFlag Convert(DepthStencilViewFlags flags)
        {
            Silk.NET.Direct3D11.DsvFlag result = 0;
            if (flags == DepthStencilViewFlags.None)
                return 0;
            if (flags.HasFlag(DepthStencilViewFlags.ReadOnlyDepth))
                result |= Silk.NET.Direct3D11.DsvFlag.DsvReadOnlyDepth;
            if (flags.HasFlag(DepthStencilViewFlags.ReadOnlyStencil))
                result |= Silk.NET.Direct3D11.DsvFlag.DsvReadOnlyStencil;
            return result;
        }

        public static Silk.NET.Direct3D11.DepthStencilViewDescUnion Convert(DepthStencilViewDescription description, DepthStencilViewDimension viewDimension)
        {
            return viewDimension switch
            {
                DepthStencilViewDimension.Texture1D => new Silk.NET.Direct3D11.DepthStencilViewDescUnion(texture1D: Convert(description.Texture1D)),
                DepthStencilViewDimension.Texture1DArray => new Silk.NET.Direct3D11.DepthStencilViewDescUnion(texture1DArray: Convert(description.Texture1DArray)),
                DepthStencilViewDimension.Texture2D => new Silk.NET.Direct3D11.DepthStencilViewDescUnion(texture2D: Convert(description.Texture2D)),
                DepthStencilViewDimension.Texture2DArray => new Silk.NET.Direct3D11.DepthStencilViewDescUnion(texture2DArray: Convert(description.Texture2DArray)),
                DepthStencilViewDimension.Texture2DMultisampled => new Silk.NET.Direct3D11.DepthStencilViewDescUnion(texture2DMS: Convert(description.Texture2DMS)),
                DepthStencilViewDimension.Texture2DMultisampledArray => new Silk.NET.Direct3D11.DepthStencilViewDescUnion(texture2DMSArray: Convert(description.Texture2DMSArray)),
                _ => throw new NotImplementedException(),
            };
        }

        public static Silk.NET.Direct3D11.Tex2DmsArrayDsv Convert(Texture2DMultisampledArrayDepthStencilView texture2DMSArray)
        {
            return new Silk.NET.Direct3D11.Tex2DmsArrayDsv((uint)texture2DMSArray.FirstArraySlice, (uint)texture2DMSArray.ArraySize);
        }

        public static Silk.NET.Direct3D11.Tex2DmsDsv Convert(Texture2DMultisampledDepthStencilView texture2DMS)
        {
            return new Silk.NET.Direct3D11.Tex2DmsDsv((uint)texture2DMS.UnusedFieldNothingToDefine);
        }

        public static Silk.NET.Direct3D11.Tex2DArrayDsv Convert(Texture2DArrayDepthStencilView texture2DArray)
        {
            return new Silk.NET.Direct3D11.Tex2DArrayDsv((uint)texture2DArray.MipSlice, (uint)texture2DArray.FirstArraySlice, (uint)texture2DArray.ArraySize);
        }

        public static Silk.NET.Direct3D11.Tex2DDsv Convert(Texture2DDepthStencilView texture2D)
        {
            return new Silk.NET.Direct3D11.Tex2DDsv((uint)texture2D.MipSlice);
        }

        public static Silk.NET.Direct3D11.Tex1DArrayDsv Convert(Texture1DArrayDepthStencilView texture1DArray)
        {
            return new Silk.NET.Direct3D11.Tex1DArrayDsv((uint)texture1DArray.MipSlice, (uint)texture1DArray.FirstArraySlice, (uint)texture1DArray.ArraySize);
        }

        public static Silk.NET.Direct3D11.Tex1DDsv Convert(Texture1DDepthStencilView texture1D)
        {
            return new Silk.NET.Direct3D11.Tex1DDsv((uint)texture1D.MipSlice);
        }

        public static Silk.NET.Direct3D11.ShaderResourceViewDesc Convert(ShaderResourceViewDescription description)
        {
            return new()
            {
                Format = Convert(description.Format),
                ViewDimension = Convert(description.ViewDimension),
                Anonymous = Convert(description, description.ViewDimension),
            };
        }

        public static Silk.NET.Direct3D11.ShaderResourceViewDescUnion Convert(ShaderResourceViewDescription description, ShaderResourceViewDimension dimension)
        {
            return dimension switch
            {
                ShaderResourceViewDimension.Buffer => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(buffer: Convert(description.Buffer)),
                ShaderResourceViewDimension.Texture1D => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(texture1D: Convert(description.Texture1D)),
                ShaderResourceViewDimension.Texture1DArray => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(texture1DArray: Convert(description.Texture1DArray)),
                ShaderResourceViewDimension.Texture2D => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(texture2D: Convert(description.Texture2D)),
                ShaderResourceViewDimension.Texture2DArray => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(texture2DArray: Convert(description.Texture2DArray)),
                ShaderResourceViewDimension.Texture2DMultisampled => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(texture2DMS: Convert(description.Texture2DMS)),
                ShaderResourceViewDimension.Texture2DMultisampledArray => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(texture2DMSArray: Convert(description.Texture2DMSArray)),
                ShaderResourceViewDimension.Texture3D => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(texture3D: Convert(description.Texture3D)),
                ShaderResourceViewDimension.TextureCube => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(textureCube: Convert(description.TextureCube)),
                ShaderResourceViewDimension.TextureCubeArray => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(textureCubeArray: Convert(description.TextureCubeArray)),
                ShaderResourceViewDimension.BufferExtended => new Silk.NET.Direct3D11.ShaderResourceViewDescUnion(bufferEx: Convert(description.BufferEx)),
                _ => throw new NotImplementedException(),
            };
        }

        public static Silk.NET.Direct3D11.BufferexSrv Convert(BufferExtendedShaderResourceView bufferEx)
        {
            return new Silk.NET.Direct3D11.BufferexSrv((uint)bufferEx.FirstElement, (uint)bufferEx.NumElements, (uint)Convert(bufferEx.Flags));
        }

        public static Silk.NET.Direct3D11.BufferexSrvFlag Convert(BufferExtendedShaderResourceViewFlags flags)
        {
            if (flags.HasFlag(BufferExtendedShaderResourceViewFlags.Raw))
                return Silk.NET.Direct3D11.BufferexSrvFlag.BufferexSrvFlagRaw;
            else
                return 0;
        }

        public static Silk.NET.Direct3D11.TexcubeArraySrv Convert(TextureCubeArrayShaderResourceView textureCubeArray)
        {
            return new Silk.NET.Direct3D11.TexcubeArraySrv((uint?)textureCubeArray.MostDetailedMip, (uint?)textureCubeArray.MipLevels, (uint?)textureCubeArray.First2DArrayFace, (uint?)textureCubeArray.NumCubes);
        }

        public static Silk.NET.Direct3D11.TexcubeSrv Convert(TextureCubeShaderResourceView textureCube)
        {
            return new Silk.NET.Direct3D11.TexcubeSrv((uint)textureCube.MostDetailedMip, (uint)textureCube.MipLevels);
        }

        public static Silk.NET.Direct3D11.Tex3DSrv Convert(Texture3DShaderResourceView texture3D)
        {
            return new Silk.NET.Direct3D11.Tex3DSrv((uint)texture3D.MostDetailedMip, (uint)texture3D.MipLevels);
        }

        public static Silk.NET.Direct3D11.Tex2DmsArraySrv Convert(Texture2DMultisampledArrayShaderResourceView texture2DMSArray)
        {
            return new()
            {
                ArraySize = (uint)texture2DMSArray.ArraySize,
                FirstArraySlice = (uint)texture2DMSArray.FirstArraySlice
            };
        }

        public static Silk.NET.Direct3D11.Tex2DmsSrv Convert(Texture2DMultisampledShaderResourceView texture2DMS)
        {
            return new() { UnusedFieldNothingToDefine = (uint)texture2DMS.UnusedFieldNothingToDefine };
        }

        public static Silk.NET.Direct3D11.Tex2DArraySrv Convert(Texture2DArrayShaderResourceView texture2DArray)
        {
            return new()
            {
                ArraySize = (uint)texture2DArray.ArraySize,
                FirstArraySlice = (uint)texture2DArray.FirstArraySlice,
                MipLevels = (uint)texture2DArray.MipLevels,
                MostDetailedMip = (uint)texture2DArray.MostDetailedMip
            };
        }

        public static Silk.NET.Direct3D11.Tex2DSrv Convert(Texture2DShaderResourceView texture2D)
        {
            return new()
            {
                MipLevels = (uint)texture2D.MipLevels,
                MostDetailedMip = (uint)texture2D.MostDetailedMip
            };
        }

        public static Silk.NET.Direct3D11.Tex1DArraySrv Convert(Texture1DArrayShaderResourceView texture1DArray)
        {
            return new()
            {
                ArraySize = (uint)texture1DArray.ArraySize,
                FirstArraySlice = (uint)texture1DArray.FirstArraySlice,
                MipLevels = (uint)texture1DArray.MipLevels,
                MostDetailedMip = (uint)texture1DArray.MostDetailedMip
            };
        }

        public static Silk.NET.Direct3D11.Tex1DSrv Convert(Texture1DShaderResourceView texture1D)
        {
            return new()
            {
                MipLevels = (uint)texture1D.MipLevels,
                MostDetailedMip = (uint)texture1D.MostDetailedMip,
            };
        }

        public static Silk.NET.Direct3D11.BufferSrv Convert(BufferShaderResourceView buffer)
        {
            return new()
            {
                Anonymous1 = new()
                {
                    ElementOffset = (uint)buffer.ElementOffset,
                    FirstElement = (uint)buffer.FirstElement
                },
                Anonymous2 = new()
                {
                    NumElements = (uint)buffer.NumElements,
                    ElementWidth = (uint)buffer.ElementWidth
                }
            };
        }

        public static Silk.NET.Core.Native.D3DSrvDimension Convert(ShaderResourceViewDimension viewDimension)
        {
            return viewDimension switch
            {
                ShaderResourceViewDimension.Unknown => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionUnknown,
                ShaderResourceViewDimension.Buffer => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionBuffer,
                ShaderResourceViewDimension.Texture1D => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexture1D,
                ShaderResourceViewDimension.Texture1DArray => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexture1Darray,
                ShaderResourceViewDimension.Texture2D => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexture2D,
                ShaderResourceViewDimension.Texture2DArray => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexture2Darray,
                ShaderResourceViewDimension.Texture2DMultisampled => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexture2Dms,
                ShaderResourceViewDimension.Texture2DMultisampledArray => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexture2Dmsarray,
                ShaderResourceViewDimension.Texture3D => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexture3D,
                ShaderResourceViewDimension.TextureCube => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexturecube,
                ShaderResourceViewDimension.TextureCubeArray => Silk.NET.Core.Native.D3DSrvDimension.D3D101SrvDimensionTexturecubearray,
                ShaderResourceViewDimension.BufferExtended => Silk.NET.Core.Native.D3DSrvDimension.D3D11SrvDimensionBufferex,
                _ => 0
            };
        }

        public static Silk.NET.Direct3D11.SubresourceData[] Convert(SubresourceData[] datas)
        {
            Silk.NET.Direct3D11.SubresourceData[] subresourceDatas = new Silk.NET.Direct3D11.SubresourceData[datas.Length];
            for (int i = 0; i < datas.Length; i++)
            {
                subresourceDatas[i] = Convert(datas[i]);
            }
            return subresourceDatas;
        }

        public static Silk.NET.Direct3D11.SubresourceData Convert(SubresourceData data)
        {
            return new()
            {
                PSysMem = data.DataPointer.ToPointer(),
                SysMemPitch = (uint)data.RowPitch,
                SysMemSlicePitch = (uint)data.SlicePitch,
            };
        }

        public static Silk.NET.Direct3D11.Texture3DDesc Convert(Texture3DDescription description)
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

        public static Silk.NET.Direct3D11.Texture2DDesc Convert(Texture2DDescription description)
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

        public static Silk.NET.DXGI.SampleDesc Convert(SampleDescription sampleDescription)
        {
            return new()
            {
                Count = (uint)sampleDescription.Count,
                Quality = (uint)sampleDescription.Quality
            };
        }

        public static Silk.NET.Direct3D11.Texture1DDesc Convert(Texture1DDescription description)
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

        public static Silk.NET.Direct3D11.DepthStencilDesc Convert(DepthStencilDescription description)
        {
            return new()
            {
                DepthEnable = description.DepthEnable ? 1 : 0,
                DepthFunc = Convert(description.DepthFunc),
                BackFace = Convert(description.BackFace),
                DepthWriteMask = Convert(description.DepthWriteMask),
                FrontFace = Convert(description.FrontFace),
                StencilEnable = description.StencilEnable ? 1 : 0,
                StencilReadMask = description.StencilReadMask,
                StencilWriteMask = description.StencilWriteMask
            };
        }

        public static Silk.NET.Direct3D11.DepthStencilopDesc Convert(DepthStencilOperationDescription description)
        {
            return new()
            {
                StencilDepthFailOp = Convert(description.StencilDepthFailOp),
                StencilFailOp = Convert(description.StencilFailOp),
                StencilFunc = Convert(description.StencilFunc),
                StencilPassOp = Convert(description.StencilPassOp),
            };
        }

        public static Silk.NET.Direct3D11.StencilOp Convert(StencilOperation operation)
        {
            return operation switch
            {
                StencilOperation.Keep => Silk.NET.Direct3D11.StencilOp.StencilOpKeep,
                StencilOperation.Zero => Silk.NET.Direct3D11.StencilOp.StencilOpZero,
                StencilOperation.Replace => Silk.NET.Direct3D11.StencilOp.StencilOpReplace,
                StencilOperation.IncrementSaturate => Silk.NET.Direct3D11.StencilOp.StencilOpIncrSat,
                StencilOperation.DecrementSaturate => Silk.NET.Direct3D11.StencilOp.StencilOpDecrSat,
                StencilOperation.Invert => Silk.NET.Direct3D11.StencilOp.StencilOpInvert,
                StencilOperation.Increment => Silk.NET.Direct3D11.StencilOp.StencilOpIncr,
                StencilOperation.Decrement => Silk.NET.Direct3D11.StencilOp.StencilOpDecr,
                _ => 0,
            };
        }

        public static Silk.NET.Direct3D11.ComparisonFunc Convert(ComparisonFunction function)
        {
            return function switch
            {
                ComparisonFunction.Never => Silk.NET.Direct3D11.ComparisonFunc.ComparisonNever,
                ComparisonFunction.Less => Silk.NET.Direct3D11.ComparisonFunc.ComparisonLess,
                ComparisonFunction.Equal => Silk.NET.Direct3D11.ComparisonFunc.ComparisonEqual,
                ComparisonFunction.LessEqual => Silk.NET.Direct3D11.ComparisonFunc.ComparisonLessEqual,
                ComparisonFunction.Greater => Silk.NET.Direct3D11.ComparisonFunc.ComparisonGreater,
                ComparisonFunction.NotEqual => Silk.NET.Direct3D11.ComparisonFunc.ComparisonNotEqual,
                ComparisonFunction.GreaterEqual => Silk.NET.Direct3D11.ComparisonFunc.ComparisonGreaterEqual,
                ComparisonFunction.Always => Silk.NET.Direct3D11.ComparisonFunc.ComparisonAlways,
                _ => 0
            };
        }

        public static Silk.NET.Direct3D11.DepthWriteMask Convert(DepthWriteMask mask)
        {
            return mask switch
            {
                DepthWriteMask.Zero => Silk.NET.Direct3D11.DepthWriteMask.DepthWriteMaskZero,
                DepthWriteMask.All => Silk.NET.Direct3D11.DepthWriteMask.DepthWriteMaskAll,
                _ => 0
            };
        }

        public static Silk.NET.Direct3D11.RasterizerDesc Convert(RasterizerDescription description)
        {
            return new()
            {
                AntialiasedLineEnable = description.AntialiasedLineEnable ? 1 : 0,
                CullMode = Convert(description.CullMode),
                DepthBias = description.DepthBias,
                DepthBiasClamp = description.DepthBiasClamp,
                DepthClipEnable = description.DepthClipEnable ? 1 : 0,
                FillMode = Convert(description.FillMode),
                FrontCounterClockwise = description.FrontCounterClockwise ? 1 : 0,
                MultisampleEnable = description.MultisampleEnable ? 1 : 0,
                ScissorEnable = description.ScissorEnable ? 1 : 0,
                SlopeScaledDepthBias = description.SlopeScaledDepthBias
            };
        }

        public static Silk.NET.Direct3D11.FillMode Convert(FillMode mode)
        {
            return mode switch
            {
                FillMode.Solid => Silk.NET.Direct3D11.FillMode.FillSolid,
                FillMode.Wireframe => Silk.NET.Direct3D11.FillMode.FillWireframe,
                _ => 0
            };
        }

        public static Silk.NET.Direct3D11.CullMode Convert(CullMode mode)
        {
            return mode switch
            {
                CullMode.None => Silk.NET.Direct3D11.CullMode.CullNone,
                CullMode.Front => Silk.NET.Direct3D11.CullMode.CullFront,
                CullMode.Back => Silk.NET.Direct3D11.CullMode.CullBack,
                _ => 0
            };
        }

        public static Silk.NET.Direct3D11.BlendDesc Convert(BlendDescription description)
        {
            return new()
            {
                AlphaToCoverageEnable = description.AlphaToCoverageEnable ? 1 : 0,
                IndependentBlendEnable = description.IndependentBlendEnable ? 1 : 0,
                RenderTarget = Convert(description.RenderTarget),
            };
        }

        public static Silk.NET.Direct3D11.BlendDesc.RenderTargetBuffer Convert(RenderTargetBlendDescription[] descriptions)
        {
            return new()
            {
                Element0 = Convert(descriptions[0]),
                Element1 = Convert(descriptions[1]),
                Element2 = Convert(descriptions[2]),
                Element3 = Convert(descriptions[3]),
                Element4 = Convert(descriptions[4]),
                Element5 = Convert(descriptions[5]),
                Element6 = Convert(descriptions[6]),
                Element7 = Convert(descriptions[7]),
            };
        }

        public static Silk.NET.Direct3D11.RenderTargetBlendDesc Convert(RenderTargetBlendDescription description)
        {
            return new()
            {
                BlendEnable = description.IsBlendEnabled ? 1 : 0,
                BlendOp = Convert(description.BlendOperation),
                BlendOpAlpha = Convert(description.BlendOperationAlpha),
                DestBlend = Convert(description.DestinationBlend),
                DestBlendAlpha = Convert(description.DestinationBlendAlpha),
                RenderTargetWriteMask = (byte)Convert(description.RenderTargetWriteMask),
                SrcBlend = Convert(description.SourceBlend),
                SrcBlendAlpha = Convert(description.SourceBlendAlpha),
            };
        }

        public static Silk.NET.Direct3D11.ColorWriteEnable Convert(ColorWriteEnable flags)
        {
            Silk.NET.Direct3D11.ColorWriteEnable result = 0;
            if (flags == ColorWriteEnable.All)
                return Silk.NET.Direct3D11.ColorWriteEnable.ColorWriteEnableAll;
            if (flags.HasFlag(ColorWriteEnable.Red))
                result |= Silk.NET.Direct3D11.ColorWriteEnable.ColorWriteEnableRed;
            if (flags.HasFlag(ColorWriteEnable.Green))
                result |= Silk.NET.Direct3D11.ColorWriteEnable.ColorWriteEnableGreen;
            if (flags.HasFlag(ColorWriteEnable.Blue))
                result |= Silk.NET.Direct3D11.ColorWriteEnable.ColorWriteEnableBlue;
            if (flags.HasFlag(ColorWriteEnable.Alpha))
                result |= Silk.NET.Direct3D11.ColorWriteEnable.ColorWriteEnableAlpha;
            return result;
        }

        public static Silk.NET.Direct3D11.Blend Convert(Blend blend)
        {
            return blend switch
            {
                Blend.Zero => Silk.NET.Direct3D11.Blend.BlendZero,
                Blend.One => Silk.NET.Direct3D11.Blend.BlendOne,
                Blend.SourceColor => Silk.NET.Direct3D11.Blend.BlendSrcColor,
                Blend.InverseSourceColor => Silk.NET.Direct3D11.Blend.BlendInvSrcColor,
                Blend.SourceAlpha => Silk.NET.Direct3D11.Blend.BlendSrcAlpha,
                Blend.InverseSourceAlpha => Silk.NET.Direct3D11.Blend.BlendInvSrcAlpha,
                Blend.DestinationAlpha => Silk.NET.Direct3D11.Blend.BlendDestAlpha,
                Blend.InverseDestinationAlpha => Silk.NET.Direct3D11.Blend.BlendInvDestAlpha,
                Blend.DestinationColor => Silk.NET.Direct3D11.Blend.BlendDestColor,
                Blend.InverseDestinationColor => Silk.NET.Direct3D11.Blend.BlendInvDestColor,
                Blend.SourceAlphaSaturate => Silk.NET.Direct3D11.Blend.BlendSrcAlphaSat,
                Blend.BlendFactor => Silk.NET.Direct3D11.Blend.BlendBlendFactor,
                Blend.InverseBlendFactor => Silk.NET.Direct3D11.Blend.BlendInvBlendFactor,
                Blend.Source1Color => Silk.NET.Direct3D11.Blend.BlendSrc1Color,
                Blend.InverseSource1Color => Silk.NET.Direct3D11.Blend.BlendInvSrc1Color,
                Blend.Source1Alpha => Silk.NET.Direct3D11.Blend.BlendSrc1Alpha,
                Blend.InverseSource1Alpha => Silk.NET.Direct3D11.Blend.BlendInvSrc1Alpha,
                _ => 0
            };
        }

        public static Silk.NET.Direct3D11.BlendOp Convert(BlendOperation operation)
        {
            return operation switch
            {
                BlendOperation.Add => Silk.NET.Direct3D11.BlendOp.BlendOpAdd,
                BlendOperation.Subtract => Silk.NET.Direct3D11.BlendOp.BlendOpSubtract,
                BlendOperation.ReverseSubtract => Silk.NET.Direct3D11.BlendOp.BlendOpRevSubtract,
                BlendOperation.Min => Silk.NET.Direct3D11.BlendOp.BlendOpMin,
                BlendOperation.Max => Silk.NET.Direct3D11.BlendOp.BlendOpMax,
                _ => 0
            };
        }

        public static Silk.NET.Direct3D11.BufferDesc Convert(BufferDescription description)
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

        public static Silk.NET.Direct3D11.CpuAccessFlag Convert(CpuAccessFlags flags)
        {
            Silk.NET.Direct3D11.CpuAccessFlag result = 0;
            if (flags.HasFlag(CpuAccessFlags.Write))
                result |= Silk.NET.Direct3D11.CpuAccessFlag.CpuAccessWrite;
            if (flags.HasFlag(CpuAccessFlags.Read))
                result |= Silk.NET.Direct3D11.CpuAccessFlag.CpuAccessRead;
            return result;
        }

        public static Silk.NET.Direct3D11.ResourceMiscFlag Convert(ResourceMiscFlag flags)
        {
            Silk.NET.Direct3D11.ResourceMiscFlag result = 0;
            if (flags.HasFlag(ResourceMiscFlag.GenerateMips))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscGenerateMips;
            if (flags.HasFlag(ResourceMiscFlag.Shared))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscShared;
            if (flags.HasFlag(ResourceMiscFlag.TextureCube))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscTexturecube;
            if (flags.HasFlag(ResourceMiscFlag.DrawIndirectArguments))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscDrawindirectArgs;
            if (flags.HasFlag(ResourceMiscFlag.BufferAllowRawViews))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscBufferAllowRawViews;
            if (flags.HasFlag(ResourceMiscFlag.BufferStructured))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscBufferStructured;
            if (flags.HasFlag(ResourceMiscFlag.ResourceClamp))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscResourceClamp;
            if (flags.HasFlag(ResourceMiscFlag.SharedKeyedMutex))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscSharedKeyedmutex;
            if (flags.HasFlag(ResourceMiscFlag.GdiCompatible))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscGdiCompatible;
            if (flags.HasFlag(ResourceMiscFlag.SharedNTHandle))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscSharedNthandle;
            if (flags.HasFlag(ResourceMiscFlag.RestrictedContent))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscRestrictedContent;
            if (flags.HasFlag(ResourceMiscFlag.RestrictSharedResource))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscRestrictSharedResource;
            if (flags.HasFlag(ResourceMiscFlag.RestrictSharedResourceDriver))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscRestrictSharedResourceDriver;
            if (flags.HasFlag(ResourceMiscFlag.Guarded))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscGuarded;
            if (flags.HasFlag(ResourceMiscFlag.TilePool))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscTilePool;
            if (flags.HasFlag(ResourceMiscFlag.Tiled))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscTiled;
            if (flags.HasFlag(ResourceMiscFlag.HardwareProtected))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscHWProtected;
            if (flags.HasFlag(ResourceMiscFlag.SharedDisplayable))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscSharedDisplayable;
            if (flags.HasFlag(ResourceMiscFlag.SharedExclusiveWriter))
                result |= Silk.NET.Direct3D11.ResourceMiscFlag.ResourceMiscSharedExclusiveWriter;
            if (flags.HasFlag(ResourceMiscFlag.None))
                result |= 0;
            return result;
        }

        public static Silk.NET.Direct3D11.Usage Convert(Usage usage)
        {
            return usage switch
            {
                Usage.Default => Silk.NET.Direct3D11.Usage.UsageDefault,
                Usage.Immutable => Silk.NET.Direct3D11.Usage.UsageImmutable,
                Usage.Dynamic => Silk.NET.Direct3D11.Usage.UsageDynamic,
                Usage.Staging => Silk.NET.Direct3D11.Usage.UsageStaging,
                _ => throw new ArgumentOutOfRangeException(nameof(usage))
            };
        }

        public static Silk.NET.Direct3D11.BindFlag Convert(BindFlags flags)
        {
            Silk.NET.Direct3D11.BindFlag result = 0;
            if (flags.HasFlag(BindFlags.VertexBuffer))
                result |= Silk.NET.Direct3D11.BindFlag.BindVertexBuffer;
            if (flags.HasFlag(BindFlags.IndexBuffer))
                result |= Silk.NET.Direct3D11.BindFlag.BindIndexBuffer;
            if (flags.HasFlag(BindFlags.ConstantBuffer))
                result |= Silk.NET.Direct3D11.BindFlag.BindConstantBuffer;
            if (flags.HasFlag(BindFlags.ShaderResource))
                result |= Silk.NET.Direct3D11.BindFlag.BindShaderResource;
            if (flags.HasFlag(BindFlags.StreamOutput))
                result |= Silk.NET.Direct3D11.BindFlag.BindStreamOutput;
            if (flags.HasFlag(BindFlags.RenderTarget))
                result |= Silk.NET.Direct3D11.BindFlag.BindRenderTarget;
            if (flags.HasFlag(BindFlags.DepthStencil))
                result |= Silk.NET.Direct3D11.BindFlag.BindDepthStencil;
            if (flags.HasFlag(BindFlags.UnorderedAccess))
                result |= Silk.NET.Direct3D11.BindFlag.BindUnorderedAccess;
            if (flags.HasFlag(BindFlags.Decoder))
                result |= Silk.NET.Direct3D11.BindFlag.BindDecoder;
            if (flags.HasFlag(BindFlags.VideoEncoder))
                result |= Silk.NET.Direct3D11.BindFlag.BindVideoEncoder;
            return result;
        }

        public static Silk.NET.Direct3D11.InputElementDesc[] Convert(InputElementDescription[] inputElements)
        {
            Silk.NET.Direct3D11.InputElementDesc[] descs = new Silk.NET.Direct3D11.InputElementDesc[inputElements.Length];
            for (int i = 0; i < inputElements.Length; i++)
            {
                descs[i] = Convert(inputElements[i]);
            }
            return descs;
        }

        public static Silk.NET.Direct3D11.InputElementDesc Convert(InputElementDescription description)
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

        public static Silk.NET.Direct3D11.InputClassification Convert(InputClassification classification)
        {
            return classification switch
            {
                InputClassification.PerVertexData => Silk.NET.Direct3D11.InputClassification.InputPerVertexData,
                InputClassification.PerInstanceData => Silk.NET.Direct3D11.InputClassification.InputPerInstanceData,
                _ => throw new NotImplementedException()
            };
        }

        public static Silk.NET.DXGI.Format Convert(Format format)
        {
            return format switch
            {
                Format.R8UNorm => Silk.NET.DXGI.Format.FormatR8Unorm,
                Format.R8SNorm => Silk.NET.DXGI.Format.FormatR8SNorm,
                Format.R8UInt => Silk.NET.DXGI.Format.FormatR8Uint,
                Format.R8SInt => Silk.NET.DXGI.Format.FormatR8Sint,
                Format.R16UNorm => Silk.NET.DXGI.Format.FormatR16Unorm,
                Format.R16SNorm => Silk.NET.DXGI.Format.FormatR16SNorm,
                Format.R16UInt => Silk.NET.DXGI.Format.FormatR16Uint,
                Format.R16SInt => Silk.NET.DXGI.Format.FormatR16Sint,
                Format.R16Float => Silk.NET.DXGI.Format.FormatR16Float,
                Format.RG8UNorm => Silk.NET.DXGI.Format.FormatR8G8Unorm,
                Format.RG8SNorm => Silk.NET.DXGI.Format.FormatR8G8SNorm,
                Format.RG8UInt => Silk.NET.DXGI.Format.FormatR8G8Uint,
                Format.RG8SInt => Silk.NET.DXGI.Format.FormatR8G8Sint,
                Format.R32UInt => Silk.NET.DXGI.Format.FormatR32Uint,
                Format.R32SInt => Silk.NET.DXGI.Format.FormatR32Sint,
                Format.R32Float => Silk.NET.DXGI.Format.FormatR32Float,
                Format.RG16UNorm => Silk.NET.DXGI.Format.FormatR16G16Unorm,
                Format.RG16SNorm => Silk.NET.DXGI.Format.FormatR16G16SNorm,
                Format.RG16UInt => Silk.NET.DXGI.Format.FormatR16G16Uint,
                Format.RG16SInt => Silk.NET.DXGI.Format.FormatR16G16Sint,
                Format.RG16Float => Silk.NET.DXGI.Format.FormatR16G16Float,
                Format.RGBA8UNorm => Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
                Format.RGBA8UNormSrgb => Silk.NET.DXGI.Format.FormatR8G8B8A8UnormSrgb,
                Format.RGBA8SNorm => Silk.NET.DXGI.Format.FormatR8G8B8A8SNorm,
                Format.RGBA8UInt => Silk.NET.DXGI.Format.FormatR8G8B8A8Uint,
                Format.RGBA8SInt => Silk.NET.DXGI.Format.FormatR8G8B8A8Sint,
                Format.BGRA8UNorm => Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm,
                Format.BGRA8UNormSrgb => Silk.NET.DXGI.Format.FormatB8G8R8A8UnormSrgb,
                Format.RGB10A2UNorm => Silk.NET.DXGI.Format.FormatR10G10B10A2Unorm,
                Format.RG11B10Float => Silk.NET.DXGI.Format.FormatR11G11B10Float,
                Format.RGB9E5Float => Silk.NET.DXGI.Format.FormatR9G9B9E5Sharedexp,
                Format.RG32UInt => Silk.NET.DXGI.Format.FormatR32G32Uint,
                Format.RG32SInt => Silk.NET.DXGI.Format.FormatR32G32Sint,
                Format.RG32Float => Silk.NET.DXGI.Format.FormatR32G32Float,
                Format.RGBA16UNorm => Silk.NET.DXGI.Format.FormatR16G16B16A16Unorm,
                Format.RGBA16SNorm => Silk.NET.DXGI.Format.FormatR16G16B16A16SNorm,
                Format.RGBA16UInt => Silk.NET.DXGI.Format.FormatR16G16B16A16Uint,
                Format.RGBA16SInt => Silk.NET.DXGI.Format.FormatR16G16B16A16Sint,
                Format.RGBA16Float => Silk.NET.DXGI.Format.FormatR16G16B16A16Float,
                Format.RGBA32UInt => Silk.NET.DXGI.Format.FormatR32G32B32A32Uint,
                Format.RGBA32SInt => Silk.NET.DXGI.Format.FormatR32G32B32A32Sint,
                Format.RGBA32Float => Silk.NET.DXGI.Format.FormatR32G32B32A32Float,
                Format.Depth16UNorm => Silk.NET.DXGI.Format.FormatD16Unorm,
                Format.Depth32Float => Silk.NET.DXGI.Format.FormatD32Float,
                Format.Depth24UNormStencil8 => Silk.NET.DXGI.Format.FormatD24UnormS8Uint,
                Format.Depth32FloatStencil8 => Silk.NET.DXGI.Format.FormatD32FloatS8X24Uint,
                Format.BC1RGBAUNorm => Silk.NET.DXGI.Format.FormatBC1Unorm,
                Format.BC1RGBAUNormSrgb => Silk.NET.DXGI.Format.FormatBC1UnormSrgb,
                Format.BC2RGBAUNorm => Silk.NET.DXGI.Format.FormatBC2Unorm,
                Format.BC2RGBAUNormSrgb => Silk.NET.DXGI.Format.FormatBC2UnormSrgb,
                Format.BC3RGBAUNorm => Silk.NET.DXGI.Format.FormatBC3Unorm,
                Format.BC3RGBAUNormSrgb => Silk.NET.DXGI.Format.FormatBC3UnormSrgb,
                Format.BC4RUNorm => Silk.NET.DXGI.Format.FormatBC4Unorm,
                Format.BC4RSNorm => Silk.NET.DXGI.Format.FormatBC4SNorm,
                Format.BC5RGUNorm => Silk.NET.DXGI.Format.FormatBC5Unorm,
                Format.BC5RGSNorm => Silk.NET.DXGI.Format.FormatBC5SNorm,
                Format.BC6HRGBUFloat => Silk.NET.DXGI.Format.FormatBC6HUF16,
                Format.BC6HRGBFloat => Silk.NET.DXGI.Format.FormatBC6HSF16,
                Format.BC7RGBAUNorm => Silk.NET.DXGI.Format.FormatBC7Unorm,
                Format.BC7RGBAUNormSrgb => Silk.NET.DXGI.Format.FormatBC7UnormSrgb,
                Format.R32Typeless => Silk.NET.DXGI.Format.FormatR32Typeless,
                _ => Silk.NET.DXGI.Format.FormatUnknown,
            };
        }

        public static Format ConvertBack(Silk.NET.DXGI.Format format)
        {
            return format switch
            {
                Silk.NET.DXGI.Format.FormatR8Unorm => Format.R8UNorm,
                Silk.NET.DXGI.Format.FormatR8SNorm => Format.R8SNorm,
                Silk.NET.DXGI.Format.FormatR8Uint => Format.R8UInt,
                Silk.NET.DXGI.Format.FormatR8Sint => Format.R8SInt,
                Silk.NET.DXGI.Format.FormatR16Unorm => Format.R16UNorm,
                Silk.NET.DXGI.Format.FormatR16SNorm => Format.R16SNorm,
                Silk.NET.DXGI.Format.FormatR16Uint => Format.R16UInt,
                Silk.NET.DXGI.Format.FormatR16Sint => Format.R16SInt,
                Silk.NET.DXGI.Format.FormatR16Float => Format.R16Float,
                Silk.NET.DXGI.Format.FormatR8G8Unorm => Format.RG8UNorm,
                Silk.NET.DXGI.Format.FormatR8G8SNorm => Format.RG8SNorm,
                Silk.NET.DXGI.Format.FormatR8G8Uint => Format.RG8UInt,
                Silk.NET.DXGI.Format.FormatR8G8Sint => Format.RG8SInt,
                Silk.NET.DXGI.Format.FormatR32Uint => Format.R32UInt,
                Silk.NET.DXGI.Format.FormatR32Sint => Format.R32SInt,
                Silk.NET.DXGI.Format.FormatR32Float => Format.R32Float,
                Silk.NET.DXGI.Format.FormatR16G16Unorm => Format.RG16UNorm,
                Silk.NET.DXGI.Format.FormatR16G16SNorm => Format.RG16SNorm,
                Silk.NET.DXGI.Format.FormatR16G16Uint => Format.RG16UInt,
                Silk.NET.DXGI.Format.FormatR16G16Sint => Format.RG16SInt,
                Silk.NET.DXGI.Format.FormatR16G16Float => Format.RG16Float,
                Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm => Format.RGBA8UNorm,
                Silk.NET.DXGI.Format.FormatR8G8B8A8UnormSrgb => Format.RGBA8UNormSrgb,
                Silk.NET.DXGI.Format.FormatR8G8B8A8SNorm => Format.RGBA8SNorm,
                Silk.NET.DXGI.Format.FormatR8G8B8A8Uint => Format.RGBA8UInt,
                Silk.NET.DXGI.Format.FormatR8G8B8A8Sint => Format.RGBA8SInt,
                Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm => Format.BGRA8UNorm,
                Silk.NET.DXGI.Format.FormatB8G8R8A8UnormSrgb => Format.BGRA8UNormSrgb,
                Silk.NET.DXGI.Format.FormatR10G10B10A2Unorm => Format.RGB10A2UNorm,
                Silk.NET.DXGI.Format.FormatR11G11B10Float => Format.RG11B10Float,
                Silk.NET.DXGI.Format.FormatR9G9B9E5Sharedexp => Format.RGB9E5Float,
                Silk.NET.DXGI.Format.FormatR32G32Uint => Format.RG32UInt,
                Silk.NET.DXGI.Format.FormatR32G32Sint => Format.RG32SInt,
                Silk.NET.DXGI.Format.FormatR32G32Float => Format.RG32Float,
                Silk.NET.DXGI.Format.FormatR16G16B16A16Unorm => Format.RGBA16UNorm,
                Silk.NET.DXGI.Format.FormatR16G16B16A16SNorm => Format.RGBA16SNorm,
                Silk.NET.DXGI.Format.FormatR16G16B16A16Uint => Format.RGBA16UInt,
                Silk.NET.DXGI.Format.FormatR16G16B16A16Sint => Format.RGBA16SInt,
                Silk.NET.DXGI.Format.FormatR16G16B16A16Float => Format.RGBA16Float,
                Silk.NET.DXGI.Format.FormatR32G32B32A32Uint => Format.RGBA32UInt,
                Silk.NET.DXGI.Format.FormatR32G32B32A32Sint => Format.RGBA32SInt,
                Silk.NET.DXGI.Format.FormatR32G32B32A32Float => Format.RGBA32Float,
                Silk.NET.DXGI.Format.FormatD16Unorm => Format.Depth16UNorm,
                Silk.NET.DXGI.Format.FormatD32Float => Format.Depth32Float,
                Silk.NET.DXGI.Format.FormatD24UnormS8Uint => Format.Depth24UNormStencil8,
                Silk.NET.DXGI.Format.FormatD32FloatS8X24Uint => Format.Depth32FloatStencil8,
                Silk.NET.DXGI.Format.FormatBC1Unorm => Format.BC1RGBAUNorm,
                Silk.NET.DXGI.Format.FormatBC1UnormSrgb => Format.BC1RGBAUNormSrgb,
                Silk.NET.DXGI.Format.FormatBC2Unorm => Format.BC2RGBAUNorm,
                Silk.NET.DXGI.Format.FormatBC2UnormSrgb => Format.BC2RGBAUNormSrgb,
                Silk.NET.DXGI.Format.FormatBC3Unorm => Format.BC3RGBAUNorm,
                Silk.NET.DXGI.Format.FormatBC3UnormSrgb => Format.BC3RGBAUNormSrgb,
                Silk.NET.DXGI.Format.FormatBC4Unorm => Format.BC4RUNorm,
                Silk.NET.DXGI.Format.FormatBC4SNorm => Format.BC4RSNorm,
                Silk.NET.DXGI.Format.FormatBC5Unorm => Format.BC5RGUNorm,
                Silk.NET.DXGI.Format.FormatBC5SNorm => Format.BC5RGSNorm,
                Silk.NET.DXGI.Format.FormatBC6HUF16 => Format.BC6HRGBUFloat,
                Silk.NET.DXGI.Format.FormatBC6HSF16 => Format.BC6HRGBFloat,
                Silk.NET.DXGI.Format.FormatBC7Unorm => Format.BC7RGBAUNorm,
                Silk.NET.DXGI.Format.FormatBC7UnormSrgb => Format.BC7RGBAUNormSrgb,
                Silk.NET.DXGI.Format.FormatR32Typeless => Format.R32Typeless,
                _ => Format.Unknown,
            };
        }
    }
}