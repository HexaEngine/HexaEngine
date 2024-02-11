namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using System;
    using System.Resources;
    using System.Runtime.CompilerServices;

    public class PostFxPass : IPostFxPass
    {
        private readonly IGraphicsPipelineState pso;

        private readonly ISamplerState? pointWrap;
        private readonly ISamplerState? pointClamp;
        private readonly ISamplerState? linearClamp;
        private readonly ISamplerState? linearWrap;
        private readonly ISamplerState? linearBorder;
        private readonly ISamplerState? anisotropicWrap;
        private readonly ISamplerState? anisotropicClamp;

        private IShaderResourceView? Input;
        private ITexture2D? InputResource;
        private IRenderTargetView? Output;
        private ITexture2D? OutputResource;
        private Viewport Viewport;

        private readonly List<ResourceRef<Texture2D>> texture2DBindings = new();

        public PostFxPass(PostFxGraphResourceBuilder creator, GraphicsPipelineStateDescEx desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            pso = creator.Device.CreateGraphicsPipelineState(desc, filename, line);
            if (pso.Bindings != null)
            {
                pointWrap = creator.CreateSamplerState("PointWrap", SamplerStateDescription.PointWrap, ResourceCreationFlags.Shared).Value;
                pointClamp = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp, ResourceCreationFlags.Shared).Value;
                linearClamp = creator.CreateSamplerState("LinearClamp", SamplerStateDescription.LinearClamp, ResourceCreationFlags.Shared).Value;
                linearWrap = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap, ResourceCreationFlags.Shared).Value;
                linearBorder = creator.CreateSamplerState("LinearBorder", SamplerStateDescription.LinearBorder, ResourceCreationFlags.Shared).Value;
                anisotropicWrap = creator.CreateSamplerState("AnisotropicWrap", SamplerStateDescription.AnisotropicWrap, ResourceCreationFlags.Shared).Value;
                anisotropicClamp = creator.CreateSamplerState("AnisotropicClamp", SamplerStateDescription.AnisotropicClamp, ResourceCreationFlags.Shared).Value;
                pso.Bindings.SetSampler("PointWrapSampler", pointWrap);
                pso.Bindings.SetSampler("PointClampSampler", pointClamp);
                pso.Bindings.SetSampler("LinearClampSampler", linearClamp);
                pso.Bindings.SetSampler("LinearWrapSampler", linearWrap);
                pso.Bindings.SetSampler("LinearBorderSampler", linearBorder);
                pso.Bindings.SetSampler("AnisotropicWrapSampler", anisotropicWrap);
                pso.Bindings.SetSampler("AnisotropicClampSampler", anisotropicClamp);
            }
        }

        public IGraphicsPipelineState State => pso;

        public IResourceBindingList Bindings => pso.Bindings;

        public virtual void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            OutputResource = resource;
            Viewport = viewport;
        }

        public virtual void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
            InputResource = resource;
            pso.Bindings?.SetSRV("InputTex", view);
        }

        public virtual void Execute(IGraphicsContext context)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetPipelineState(pso);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        public void AddSRVBinding(ResourceRef<Texture2D> resourceRef)
        {
            texture2DBindings.Add(resourceRef);
            resourceRef.ValueChanged += Texture2DValueChanged;
            pso.Bindings.SetSRV(resourceRef.Name, resourceRef.Value);
        }

        private void Texture2DValueChanged(ResourceRef<Texture2D> resourceRef, Texture2D? texture)
        {
            pso.Bindings.SetSRV(resourceRef.Name, resourceRef.Value);
        }

        public void Dispose()
        {
            for (int i = 0; i < texture2DBindings.Count; i++)
            {
                texture2DBindings[i].ValueChanged -= Texture2DValueChanged;
            }
            pso.Dispose();
        }
    }
}