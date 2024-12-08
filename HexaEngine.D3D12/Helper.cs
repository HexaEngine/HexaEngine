namespace HexaEngine.D3D12
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Runtime.CompilerServices;

    public static unsafe class Helper
    {
        public static ShaderBytecode ToShaderBytecode(this Shader shader)
        {
            return new ShaderBytecode(shader.Bytecode, shader.Length);
        }

        public static BlendDesc Convert(BlendDescription blend)
        {
            var result = new BlendDesc()
            {
                AlphaToCoverageEnable = blend.AlphaToCoverageEnable,
                IndependentBlendEnable = blend.IndependentBlendEnable,
            };

            Convert(blend.RenderTarget, &result.RenderTarget_0);

            return result;
        }

        public static void Convert(RenderTargetBlendDescription[] renderTarget, RenderTargetBlendDesc* output)
        {
            output[0] = Convert(renderTarget[0]);
            output[1] = Convert(renderTarget[1]);
            output[2] = Convert(renderTarget[2]);
            output[3] = Convert(renderTarget[3]);
            output[4] = Convert(renderTarget[4]);
            output[5] = Convert(renderTarget[5]);
            output[6] = Convert(renderTarget[6]);
            output[7] = Convert(renderTarget[7]);
        }

        public static RenderTargetBlendDesc Convert(RenderTargetBlendDescription renderTargetBlendDescription)
        {
            return new RenderTargetBlendDesc()
            {
                BlendEnable = renderTargetBlendDescription.IsBlendEnabled,
                BlendOp = Convert(renderTargetBlendDescription.BlendOperation),
                BlendOpAlpha = Convert(renderTargetBlendDescription.BlendOperationAlpha),
                DestBlend = Convert(renderTargetBlendDescription.DestinationBlend),
                DestBlendAlpha = Convert(renderTargetBlendDescription.DestinationBlendAlpha),
                LogicOp = Convert(renderTargetBlendDescription.LogicOperation),
                LogicOpEnable = renderTargetBlendDescription.IsLogicOpEnabled,
                RenderTargetWriteMask = (byte)Convert(renderTargetBlendDescription.RenderTargetWriteMask),
                SrcBlend = Convert(renderTargetBlendDescription.SourceBlend),
                SrcBlendAlpha = Convert(renderTargetBlendDescription.SourceBlendAlpha)
            };
        }

        private static Hexa.NET.D3D12.ColorWriteEnable Convert(ColorWriteEnable renderTargetWriteMask)
        {
            Hexa.NET.D3D12.ColorWriteEnable result = 0;
            if ((renderTargetWriteMask & ColorWriteEnable.Red) != 0)
            {
                result |= Hexa.NET.D3D12.ColorWriteEnable.Red;
            }
            if ((renderTargetWriteMask & ColorWriteEnable.Green) != 0)
            {
                result |= Hexa.NET.D3D12.ColorWriteEnable.Green;
            }
            if ((renderTargetWriteMask & ColorWriteEnable.Blue) != 0)
            {
                result |= Hexa.NET.D3D12.ColorWriteEnable.Blue;
            }
            if ((renderTargetWriteMask & ColorWriteEnable.Alpha) != 0)
            {
                result |= Hexa.NET.D3D12.ColorWriteEnable.Alpha;
            }
            return result;
        }

        private static LogicOp Convert(LogicOperation logicOperation)
        {
            return logicOperation switch
            {
                LogicOperation.Clear => LogicOp.Clear,
                LogicOperation.Set => LogicOp.Set,
                LogicOperation.Copy => LogicOp.Copy,
                LogicOperation.CopyInverted => LogicOp.CopyInverted,
                LogicOperation.Noop => LogicOp.Noop,
                LogicOperation.Invert => LogicOp.Invert,
                LogicOperation.And => LogicOp.And,
                LogicOperation.Nand => LogicOp.Nand,
                LogicOperation.Or => LogicOp.Or,
                LogicOperation.Nor => LogicOp.Nor,
                LogicOperation.Xor => LogicOp.Xor,
                LogicOperation.Equiv => LogicOp.Equiv,
                LogicOperation.AndReverse => LogicOp.AndReverse,
                LogicOperation.AndInverted => LogicOp.AndInverted,
                LogicOperation.OrReverse => LogicOp.OrReverse,
                LogicOperation.OrInverted => LogicOp.OrInverted,
                _ => throw new NotSupportedException(),
            };
        }

        private static Hexa.NET.D3D12.Blend Convert(Blend blend)
        {
            return blend switch
            {
                Blend.Zero => Hexa.NET.D3D12.Blend.Zero,
                Blend.One => Hexa.NET.D3D12.Blend.One,
                Blend.SourceColor => Hexa.NET.D3D12.Blend.SrcColor,
                Blend.InverseSourceColor => Hexa.NET.D3D12.Blend.InvSrcColor,
                Blend.SourceAlpha => Hexa.NET.D3D12.Blend.SrcAlpha,
                Blend.InverseSourceAlpha => Hexa.NET.D3D12.Blend.InvSrcAlpha,
                Blend.DestinationAlpha => Hexa.NET.D3D12.Blend.DestAlpha,
                Blend.InverseDestinationAlpha => Hexa.NET.D3D12.Blend.InvDestAlpha,
                Blend.DestinationColor => Hexa.NET.D3D12.Blend.DestColor,
                Blend.InverseDestinationColor => Hexa.NET.D3D12.Blend.InvDestColor,
                Blend.SourceAlphaSaturate => Hexa.NET.D3D12.Blend.SrcAlphaSat,
                Blend.BlendFactor => Hexa.NET.D3D12.Blend.Factor,
                Blend.InverseBlendFactor => Hexa.NET.D3D12.Blend.InvBlendFactor,
                Blend.Source1Color => Hexa.NET.D3D12.Blend.Src1Color,
                Blend.InverseSource1Color => Hexa.NET.D3D12.Blend.InvSrc1Color,
                Blend.Source1Alpha => Hexa.NET.D3D12.Blend.Src1Alpha,
                Blend.InverseSource1Alpha => Hexa.NET.D3D12.Blend.InvSrc1Alpha,
                _ => throw new NotSupportedException(),
            };
        }

        private static BlendOp Convert(BlendOperation blendOperation)
        {
            return blendOperation switch
            {
                BlendOperation.Add => BlendOp.Add,
                BlendOperation.Subtract => BlendOp.Subtract,
                BlendOperation.ReverseSubtract => BlendOp.RevSubtract,
                BlendOperation.Min => BlendOp.Min,
                BlendOperation.Max => BlendOp.Max,
                _ => throw new NotSupportedException(),
            };
        }

        internal static RasterizerDesc Convert(RasterizerDescription rasterizer)
        {
            return new RasterizerDesc()
            {
                AntialiasedLineEnable = rasterizer.AntialiasedLineEnable,
                ConservativeRaster = Convert(rasterizer.ConservativeRaster),
                CullMode = Convert(rasterizer.CullMode),
                DepthBias = rasterizer.DepthBias,
                DepthBiasClamp = rasterizer.DepthBiasClamp,
                DepthClipEnable = rasterizer.DepthClipEnable,
                FillMode = Convert(rasterizer.FillMode),
                ForcedSampleCount = rasterizer.ForcedSampleCount,
                FrontCounterClockwise = rasterizer.FrontCounterClockwise,
                MultisampleEnable = rasterizer.MultisampleEnable,
                SlopeScaledDepthBias = rasterizer.SlopeScaledDepthBias,
            };
        }

        private static Hexa.NET.D3D12.FillMode Convert(FillMode fillMode)
        {
            return fillMode switch
            {
                FillMode.Wireframe => Hexa.NET.D3D12.FillMode.Wireframe,
                FillMode.Solid => Hexa.NET.D3D12.FillMode.Solid,
                _ => throw new NotSupportedException(),
            };
        }

        private static Hexa.NET.D3D12.ConservativeRasterizationMode Convert(ConservativeRasterizationMode conservativeRaster)
        {
            return conservativeRaster switch
            {
                ConservativeRasterizationMode.Off => Hexa.NET.D3D12.ConservativeRasterizationMode.Off,
                ConservativeRasterizationMode.On => Hexa.NET.D3D12.ConservativeRasterizationMode.On,
                _ => throw new NotSupportedException(),
            };
        }

        private static Hexa.NET.D3D12.CullMode Convert(CullMode cullMode)
        {
            return cullMode switch
            {
                CullMode.None => Hexa.NET.D3D12.CullMode.None,
                CullMode.Front => Hexa.NET.D3D12.CullMode.Front,
                CullMode.Back => Hexa.NET.D3D12.CullMode.Back,
                _ => throw new NotSupportedException(),
            };
        }

        internal static DepthStencilDesc Convert(DepthStencilDescription depthStencil)
        {
            return new DepthStencilDesc()
            {
                BackFace = Convert(depthStencil.BackFace),
                FrontFace = Convert(depthStencil.FrontFace),
                DepthEnable = depthStencil.DepthEnable,
                DepthFunc = Convert(depthStencil.DepthFunc),
                DepthWriteMask = Convert(depthStencil.DepthWriteMask),
                StencilEnable = depthStencil.StencilEnable,
                StencilReadMask = depthStencil.StencilReadMask,
                StencilWriteMask = depthStencil.StencilWriteMask,
            };
        }

        private static Hexa.NET.D3D12.DepthWriteMask Convert(DepthWriteMask depthWriteMask)
        {
            Hexa.NET.D3D12.DepthWriteMask result = 0;
            if ((depthWriteMask & DepthWriteMask.All) != 0)
            {
                result |= Hexa.NET.D3D12.DepthWriteMask.All;
            }
            return result;
        }

        private static ComparisonFunc Convert(ComparisonFunction comparisonFunction)
        {
            return comparisonFunction switch
            {
                ComparisonFunction.Never => ComparisonFunc.Never,
                ComparisonFunction.Less => ComparisonFunc.Less,
                ComparisonFunction.Equal => ComparisonFunc.Equal,
                ComparisonFunction.LessEqual => ComparisonFunc.LessEqual,
                ComparisonFunction.Greater => ComparisonFunc.Greater,
                ComparisonFunction.NotEqual => ComparisonFunc.NotEqual,
                ComparisonFunction.GreaterEqual => ComparisonFunc.GreaterEqual,
                ComparisonFunction.Always => ComparisonFunc.Always,
                _ => throw new NotSupportedException(),
            };
        }

        private static DepthStencilopDesc Convert(DepthStencilOperationDescription stencilOperationDescription)
        {
            return new DepthStencilopDesc()
            {
                StencilDepthFailOp = Convert(stencilOperationDescription.StencilDepthFailOp),
                StencilFailOp = Convert(stencilOperationDescription.StencilFailOp),
                StencilFunc = Convert(stencilOperationDescription.StencilFunc),
                StencilPassOp = Convert(stencilOperationDescription.StencilPassOp)
            };
        }

        private static StencilOp Convert(StencilOperation stencilOperation)
        {
            return stencilOperation switch
            {
                StencilOperation.Keep => StencilOp.Keep,
                StencilOperation.Zero => StencilOp.Zero,
                StencilOperation.Replace => StencilOp.Replace,
                StencilOperation.IncrementSaturate => StencilOp.IncrSat,
                StencilOperation.DecrementSaturate => StencilOp.DecrSat,
                StencilOperation.Invert => StencilOp.Invert,
                StencilOperation.Increment => StencilOp.Incr,
                StencilOperation.Decrement => StencilOp.Decr,
                _ => throw new NotSupportedException(),
            };
        }

        internal static PrimitiveTopologyType ConvertType(PrimitiveTopology topology)
        {
            return topology switch
            {
                PrimitiveTopology.Undefined => PrimitiveTopologyType.Undefined,
                PrimitiveTopology.PointList => PrimitiveTopologyType.Point,
                PrimitiveTopology.LineList => PrimitiveTopologyType.Line,
                PrimitiveTopology.LineStrip => PrimitiveTopologyType.Line,
                PrimitiveTopology.TriangleList => PrimitiveTopologyType.Triangle,
                PrimitiveTopology.TriangleStrip => PrimitiveTopologyType.Triangle,
                PrimitiveTopology.LineListAdjacency => PrimitiveTopologyType.Line,
                PrimitiveTopology.LineStripAdjacency => PrimitiveTopologyType.Line,
                PrimitiveTopology.TriangleListAdjacency => PrimitiveTopologyType.Triangle,
                PrimitiveTopology.TriangleStripAdjacency => PrimitiveTopologyType.Triangle,
                PrimitiveTopology.PatchListWith1ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith2ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith3ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith4ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith5ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith6ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith7ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith8ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith9ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith10ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith11ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith12ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith13ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith14ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith15ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith16ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith17ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith18ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith19ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith20ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith21ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith22ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith23ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith24ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith25ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith26ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith27ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith28ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith29ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith30ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith31ControlPoints => PrimitiveTopologyType.Patch,
                PrimitiveTopology.PatchListWith32ControlPoints => PrimitiveTopologyType.Patch,
                _ => throw new ArgumentOutOfRangeException(nameof(topology)),
            };
        }

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

        internal static ResourceFlags Convert(ResourceMiscFlag miscFlags)
        {
            throw new NotImplementedException();
        }
    }
}