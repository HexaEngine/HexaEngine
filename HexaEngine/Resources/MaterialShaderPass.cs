﻿namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;

    public class MaterialShaderPass : IDisposable
    {
        private readonly string name;
        private readonly IGraphicsPipelineState pipelineState;

        public MaterialShaderPass(string name, IGraphicsDevice device, IGraphicsPipeline pipeline, GraphicsPipelineStateDesc desc)
        {
            this.name = name;
            pipelineState = device.CreateGraphicsPipelineState(pipeline, desc);
        }

        public MaterialShaderPass(string name, IGraphicsDevice device, GraphicsPipelineDesc pipelineDesc, GraphicsPipelineStateDesc desc)
        {
            this.name = name;
            pipelineState = device.CreateGraphicsPipelineState(pipelineDesc, desc);
        }

        public string Name => name;

        public bool BeginDraw(IGraphicsContext context)
        {
            if (!pipelineState.IsValid)
            {
                return false;
            }

            context.SetPipelineState(pipelineState);
            return true;
        }

        public void EndDraw(IGraphicsContext context)
        {
            context.SetPipelineState(null);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            pipelineState.Dispose();
        }

        public override string ToString()
        {
            return name;
        }
    }
}