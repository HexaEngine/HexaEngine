namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class Helper
    {
        public static TriangleFace Convert(CullMode mode)
        {
            return mode switch
            {
                CullMode.Back => TriangleFace.Back,
                CullMode.Front => TriangleFace.Front,
                CullMode.None => TriangleFace.FrontAndBack,
                _ => throw new NotSupportedException()
            };
        }

        public static PolygonMode Convert(FillMode mode)
        {
            return mode switch
            {
                FillMode.Wireframe => PolygonMode.Line,
                FillMode.Solid => PolygonMode.Fill,
                _ => throw new NotSupportedException()
            };
        }

        public static DepthFunction Convert(ComparisonFunction function)
        {
            return function switch
            {
                ComparisonFunction.Never => DepthFunction.Never,
                ComparisonFunction.Less => DepthFunction.Less,
                ComparisonFunction.Equal => DepthFunction.Equal,
                ComparisonFunction.LessEqual => DepthFunction.Lequal,
                ComparisonFunction.Greater => DepthFunction.Greater,
                ComparisonFunction.NotEqual => DepthFunction.Notequal,
                ComparisonFunction.GreaterEqual => DepthFunction.Gequal,
                ComparisonFunction.Always => DepthFunction.Always,
            };
        }

        public static StencilFunction Convert2(ComparisonFunction function)
        {
            return function switch
            {
                ComparisonFunction.Never => StencilFunction.Never,
                ComparisonFunction.Less => StencilFunction.Less,
                ComparisonFunction.Equal => StencilFunction.Equal,
                ComparisonFunction.LessEqual => StencilFunction.Lequal,
                ComparisonFunction.Greater => StencilFunction.Greater,
                ComparisonFunction.NotEqual => StencilFunction.Notequal,
                ComparisonFunction.GreaterEqual => StencilFunction.Gequal,
                ComparisonFunction.Always => StencilFunction.Always,
            };
        }

        public static StencilOp Convert(StencilOperation operation)
        {
            return operation switch
            {
                StencilOperation.Keep => StencilOp.Keep,
                StencilOperation.Zero => StencilOp.Zero,
                StencilOperation.Replace => StencilOp.Replace,
                StencilOperation.IncrementSaturate => StencilOp.IncrWrap,
                StencilOperation.DecrementSaturate => StencilOp.DecrWrap,
                StencilOperation.Invert => StencilOp.Invert,
                StencilOperation.Increment => StencilOp.Incr,
                StencilOperation.Decrement => StencilOp.Decr,
            };
        }

        public static BlendingFactor Convert(Blend blend)
        {
            return blend switch
            {
                Blend.Zero => BlendingFactor.Zero,
                Blend.One => BlendingFactor.One,
                Blend.SourceColor => BlendingFactor.SrcColor,
                Blend.InverseSourceColor => BlendingFactor.OneMinusSrcColor,
                Blend.SourceAlpha => BlendingFactor.SrcAlpha,
                Blend.InverseSourceAlpha => BlendingFactor.OneMinusSrcAlpha,
                Blend.DestinationAlpha => BlendingFactor.DstAlpha,
                Blend.InverseDestinationAlpha => BlendingFactor.OneMinusDstAlpha,
                Blend.DestinationColor => BlendingFactor.DstColor,
                Blend.InverseDestinationColor => BlendingFactor.OneMinusDstColor,
                Blend.SourceAlphaSaturate => BlendingFactor.SrcAlphaSaturate,
                Blend.BlendFactor => BlendingFactor.ConstantColor,
                Blend.InverseBlendFactor => BlendingFactor.OneMinusConstantColor,
                Blend.Source1Color => BlendingFactor.Src1Color,
                Blend.InverseSource1Color => BlendingFactor.OneMinusSrc1Color,
                Blend.Source1Alpha => BlendingFactor.Src1Alpha,
                Blend.InverseSource1Alpha => BlendingFactor.OneMinusSrc1Alpha,
            };
        }

        public static BlendEquationModeEXT Convert(BlendOperation operation)
        {
            return operation switch
            {
                BlendOperation.Add => BlendEquationModeEXT.FuncAdd,
                BlendOperation.Subtract => BlendEquationModeEXT.FuncSubtract,
                BlendOperation.ReverseSubtract => BlendEquationModeEXT.FuncReverseSubtract,
                BlendOperation.Min => BlendEquationModeEXT.Min,
                BlendOperation.Max => BlendEquationModeEXT.Max,
            };
        }

        public static BufferTargetARB Convert(BindFlags bindFlags, ResourceMiscFlag resourceMiscFlag)
        {
            BufferTargetARB target = bindFlags switch
            {
                BindFlags.IndexBuffer => BufferTargetARB.ElementArrayBuffer,
                BindFlags.ConstantBuffer => BufferTargetARB.UniformBuffer,
                BindFlags.ShaderResource => BufferTargetARB.ShaderStorageBuffer,
                BindFlags.VertexBuffer => BufferTargetARB.ArrayBuffer,
                BindFlags.StreamOutput => throw new NotSupportedException(),
                BindFlags.RenderTarget => throw new NotSupportedException(),
                BindFlags.DepthStencil => throw new NotSupportedException(),
                BindFlags.UnorderedAccess => throw new NotSupportedException(),
                BindFlags.Decoder => throw new NotSupportedException(),
                BindFlags.VideoEncoder => throw new NotSupportedException(),
                BindFlags.None => throw new NotSupportedException(),
                _ => 0
            };

            if (target != 0)
            {
                return target;
            }

            return resourceMiscFlag switch
            {
                ResourceMiscFlag.GenerateMips => throw new NotSupportedException(),
                ResourceMiscFlag.Shared => throw new NotSupportedException(),
                ResourceMiscFlag.TextureCube => throw new NotSupportedException(),
                ResourceMiscFlag.DrawIndirectArguments => BufferTargetARB.DrawIndirectBuffer | BufferTargetARB.DispatchIndirectBuffer,
                ResourceMiscFlag.BufferAllowRawViews => throw new NotImplementedException(),
                ResourceMiscFlag.BufferStructured => BufferTargetARB.ShaderStorageBuffer,
                ResourceMiscFlag.ResourceClamp => throw new NotSupportedException(),
                ResourceMiscFlag.SharedKeyedMutex => throw new NotSupportedException(),
                ResourceMiscFlag.GdiCompatible => throw new NotSupportedException(),
                ResourceMiscFlag.SharedNTHandle => throw new NotSupportedException(),
                ResourceMiscFlag.RestrictedContent => throw new NotSupportedException(),
                ResourceMiscFlag.RestrictSharedResource => throw new NotSupportedException(),
                ResourceMiscFlag.RestrictSharedResourceDriver => throw new NotSupportedException(),
                ResourceMiscFlag.Guarded => throw new NotSupportedException(),
                ResourceMiscFlag.TilePool => throw new NotSupportedException(),
                ResourceMiscFlag.Tiled => throw new NotSupportedException(),
                ResourceMiscFlag.HardwareProtected => throw new NotSupportedException(),
                ResourceMiscFlag.SharedDisplayable => throw new NotSupportedException(),
                ResourceMiscFlag.SharedExclusiveWriter => throw new NotSupportedException(),
                ResourceMiscFlag.None => throw new NotSupportedException(),
            };
        }

        public static BufferUsageARB Convert(Usage usage)
        {
            return usage switch
            {
                Usage.Default => BufferUsageARB.DynamicDraw,
                Usage.Immutable => BufferUsageARB.StaticDraw,
                Usage.Dynamic => BufferUsageARB.StreamDraw,
                Usage.Staging => BufferUsageARB.DynamicDraw,
                _ => throw new NotImplementedException(),
            };
        }
    }
}