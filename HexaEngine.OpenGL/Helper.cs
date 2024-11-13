namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.OpenGL;
    using System;

    public static class Helper
    {
        public static GLTriangleFace Convert(CullMode mode)
        {
            return mode switch
            {
                CullMode.Back => GLTriangleFace.Back,
                CullMode.Front => GLTriangleFace.Front,
                CullMode.None => GLTriangleFace.FrontAndBack,
                _ => throw new NotSupportedException()
            };
        }

        public static GLPolygonMode Convert(FillMode mode)
        {
            return mode switch
            {
                FillMode.Wireframe => GLPolygonMode.Line,
                FillMode.Solid => GLPolygonMode.Fill,
                _ => throw new NotSupportedException()
            };
        }

        public static GLDepthFunction Convert(ComparisonFunction function)
        {
            return function switch
            {
                ComparisonFunction.Never => GLDepthFunction.Never,
                ComparisonFunction.Less => GLDepthFunction.Less,
                ComparisonFunction.Equal => GLDepthFunction.Equal,
                ComparisonFunction.LessEqual => GLDepthFunction.Lequal,
                ComparisonFunction.Greater => GLDepthFunction.Greater,
                ComparisonFunction.NotEqual => GLDepthFunction.Notequal,
                ComparisonFunction.GreaterEqual => GLDepthFunction.Gequal,
                ComparisonFunction.Always => GLDepthFunction.Always,
            };
        }

        public static GLStencilFunction Convert2(ComparisonFunction function)
        {
            return function switch
            {
                ComparisonFunction.Never => GLStencilFunction.Never,
                ComparisonFunction.Less => GLStencilFunction.Less,
                ComparisonFunction.Equal => GLStencilFunction.Equal,
                ComparisonFunction.LessEqual => GLStencilFunction.Lequal,
                ComparisonFunction.Greater => GLStencilFunction.Greater,
                ComparisonFunction.NotEqual => GLStencilFunction.Notequal,
                ComparisonFunction.GreaterEqual => GLStencilFunction.Gequal,
                ComparisonFunction.Always => GLStencilFunction.Always,
            };
        }

        public static GLStencilOp Convert(StencilOperation operation)
        {
            return operation switch
            {
                StencilOperation.Keep => GLStencilOp.Keep,
                StencilOperation.Zero => GLStencilOp.Zero,
                StencilOperation.Replace => GLStencilOp.Replace,
                StencilOperation.IncrementSaturate => GLStencilOp.IncrWrap,
                StencilOperation.DecrementSaturate => GLStencilOp.DecrWrap,
                StencilOperation.Invert => GLStencilOp.Invert,
                StencilOperation.Increment => GLStencilOp.Incr,
                StencilOperation.Decrement => GLStencilOp.Decr,
            };
        }

        public static GLBlendingFactor Convert(Blend blend)
        {
            return blend switch
            {
                Blend.Zero => GLBlendingFactor.Zero,
                Blend.One => GLBlendingFactor.One,
                Blend.SourceColor => GLBlendingFactor.SrcColor,
                Blend.InverseSourceColor => GLBlendingFactor.OneMinusSrcColor,
                Blend.SourceAlpha => GLBlendingFactor.SrcAlpha,
                Blend.InverseSourceAlpha => GLBlendingFactor.OneMinusSrcAlpha,
                Blend.DestinationAlpha => GLBlendingFactor.DstAlpha,
                Blend.InverseDestinationAlpha => GLBlendingFactor.OneMinusDstAlpha,
                Blend.DestinationColor => GLBlendingFactor.DstColor,
                Blend.InverseDestinationColor => GLBlendingFactor.OneMinusDstColor,
                Blend.SourceAlphaSaturate => GLBlendingFactor.SrcAlphaSaturate,
                Blend.BlendFactor => GLBlendingFactor.ConstantColor,
                Blend.InverseBlendFactor => GLBlendingFactor.OneMinusConstantColor,
                Blend.Source1Color => GLBlendingFactor.Src1Color,
                Blend.InverseSource1Color => GLBlendingFactor.OneMinusSrc1Color,
                Blend.Source1Alpha => GLBlendingFactor.Src1Alpha,
                Blend.InverseSource1Alpha => GLBlendingFactor.OneMinusSrc1Alpha,
            };
        }

        public static GLBlendEquationModeEXT Convert(BlendOperation operation)
        {
            return operation switch
            {
                BlendOperation.Add => GLBlendEquationModeEXT.FuncAdd,
                BlendOperation.Subtract => GLBlendEquationModeEXT.FuncSubtract,
                BlendOperation.ReverseSubtract => GLBlendEquationModeEXT.FuncReverseSubtract,
                BlendOperation.Min => GLBlendEquationModeEXT.Min,
                BlendOperation.Max => GLBlendEquationModeEXT.Max,
            };
        }

        public static GLBufferTargetARB ConvertBufferTarget(BindFlags bindFlags, ResourceMiscFlag resourceMiscFlag)
        {
            GLBufferTargetARB target = bindFlags switch
            {
                BindFlags.IndexBuffer => GLBufferTargetARB.ElementArrayBuffer,
                BindFlags.ConstantBuffer => GLBufferTargetARB.UniformBuffer,
                BindFlags.ShaderResource => GLBufferTargetARB.ShaderStorageBuffer,
                BindFlags.VertexBuffer => GLBufferTargetARB.ArrayBuffer,
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
                ResourceMiscFlag.DrawIndirectArguments => GLBufferTargetARB.DrawIndirectBuffer | GLBufferTargetARB.DispatchIndirectBuffer,
                ResourceMiscFlag.BufferAllowRawViews => throw new NotImplementedException(),
                ResourceMiscFlag.BufferStructured => GLBufferTargetARB.ShaderStorageBuffer,
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

        public static GLBufferUsageARB Convert(Usage usage)
        {
            return usage switch
            {
                Usage.Default => GLBufferUsageARB.DynamicDraw,
                Usage.Immutable => GLBufferUsageARB.StaticDraw,
                Usage.Dynamic => GLBufferUsageARB.StreamDraw,
                Usage.Staging => GLBufferUsageARB.DynamicDraw,
                _ => throw new NotImplementedException(),
            };
        }
    }
}