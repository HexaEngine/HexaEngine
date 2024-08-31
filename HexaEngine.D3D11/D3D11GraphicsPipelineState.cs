namespace HexaEngine.D3D11
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using System.Numerics;

    public abstract class D3D11PipelineState : DisposableBase
    {
        internal abstract void SetState(ComPtr<ID3D11DeviceContext3> context);

        internal abstract void UnsetState(ComPtr<ID3D11DeviceContext3> context);
    }

    public unsafe class D3D11GraphicsPipelineState : D3D11PipelineState, IGraphicsPipelineState
    {
        private readonly D3D11GraphicsDevice device;
        private readonly D3D11GraphicsPipeline pipeline;
        private readonly D3D11ResourceBindingList? resourceBindingList;
        private readonly string dbgName;

        private ComPtr<ID3D11VertexShader> vs;
        private ComPtr<ID3D11HullShader> hs;
        private ComPtr<ID3D11DomainShader> ds;
        private ComPtr<ID3D11GeometryShader> gs;
        private ComPtr<ID3D11PixelShader> ps;

        private ComPtr<ID3D11InputLayout> layout;
        private ComPtr<ID3D11RasterizerState2> RasterizerState;
        private ComPtr<ID3D11DepthStencilState> DepthStencilState;
        private ComPtr<ID3D11BlendState1> BlendState;

        private GraphicsPipelineStateDesc desc = GraphicsPipelineStateDesc.Default;
        private bool isValid = false;

        private D3DPrimitiveTopology primitiveTopology;

        public D3D11GraphicsPipelineState(D3D11GraphicsDevice device, D3D11GraphicsPipeline pipeline, GraphicsPipelineStateDesc desc, string dbgName = "")
        {
            pipeline.AddRef();
            this.desc = desc;
            this.device = device;
            this.pipeline = pipeline;
            this.dbgName = dbgName;

            PipelineStateManager.Register(this);

            {
                pipeline.OnCompile += OnPipelineCompile;
                pipeline.OnCreateLayout += CreateLayout;
                vs = pipeline.vs;
                hs = pipeline.hs;
                ds = pipeline.ds;
                gs = pipeline.gs;
                ps = pipeline.ps;
            }

            if (pipeline.signature == null)
            {
                LoggerFactory.GetLogger(nameof(D3D11)).Error("Failed to create input layout, signature was null.");
            }
            else
            {
                CreateLayout(pipeline, pipeline.inputElements, pipeline.signature);
                isValid = true;
            }

            ComPtr<ID3D11RasterizerState2> rasterizerState;
            var rsDesc = Helper.Convert(desc.Rasterizer);
            device.Device.CreateRasterizerState2(&rsDesc, &rasterizerState.Handle);
            RasterizerState = rasterizerState;

            /*  if (!result.IsSuccess)
              {
                  Logger.Error($"Failed to create ID3D11RasterizerState2, {result.GetMessage()}");
                  isValid = false;
              }
            */

            ComPtr<ID3D11DepthStencilState> depthStencilState;
            var dsDesc = Helper.Convert(desc.DepthStencil);
            device.Device.CreateDepthStencilState(&dsDesc, &depthStencilState.Handle);
            DepthStencilState = depthStencilState;

            /*if (!result.IsSuccess)
            {
                Logger.Error($"Failed to create ID3D11DepthStencilState, {result.GetMessage()}");
                isValid = false;
            }*/

            ComPtr<ID3D11BlendState1> blendState;
            var bsDesc = Helper.Convert(desc.Blend);
            device.Device.CreateBlendState1(&bsDesc, &blendState.Handle);
            BlendState = blendState;

            /*  if (!result.IsSuccess)
              {
                  Logger.Error($"Failed to create ID3D11BlendState1, {result.GetMessage()}");
                  isValid = false;
              }*/

            resourceBindingList = new(pipeline);
            primitiveTopology = Helper.Convert(desc.Topology);
        }

        private static bool CanSkipLayout(InputElementDescription[]? inputElements)
        {
            ArgumentNullException.ThrowIfNull(inputElements, nameof(inputElements));

            for (int i = 0; i < inputElements.Length; i++)
            {
                var inputElement = inputElements[i];
                if (inputElement.SemanticName is not "SV_VertexID" and not "SV_InstanceID")
                {
                    return false;
                }
            }

            return true;
        }

        private void CreateLayout(IGraphicsPipeline pipe, InputElementDescription[]? defaultInputElements, Blob signature)
        {
            isValid = false;
            if (layout.Handle != null)
            {
                layout.Release();
                layout = default;
            }

            var inputElements = desc.InputElements;
            inputElements ??= defaultInputElements;

            if (inputElements == null)
            {
                LoggerFactory.GetLogger(nameof(D3D11)).Error("Failed to create input layout, InputElements was null or Reflection failed.");
                return;
            }

            if (!CanSkipLayout(inputElements))
            {
                ComPtr<ID3D11InputLayout> il;
                InputElementDesc* descs = AllocT<InputElementDesc>(inputElements.Length);
                Helper.Convert(inputElements, descs);
                device.Device.CreateInputLayout(descs, (uint)inputElements.Length, (void*)signature.BufferPointer, signature.PointerSize, &il.Handle);
                Helper.Free(descs, inputElements.Length);
                Free(descs);
                layout = il;

                Utils.SetDebugName(layout, $"{dbgName}.{nameof(layout)}");
            }
            else
            {
                layout = default;
            }

            isValid = true;
        }

        private void OnPipelineCompile(IPipeline pipe)
        {
            D3D11GraphicsPipeline pipeline = (D3D11GraphicsPipeline)pipe;
            vs = pipeline.vs;
            hs = pipeline.hs;
            ds = pipeline.ds;
            gs = pipeline.gs;
            ps = pipeline.ps;
        }

        public IGraphicsPipeline Pipeline => pipeline;

        public GraphicsPipelineStateDesc Description => desc;

        public PrimitiveTopology Topology
        {
            get => desc.Topology;
            set
            {
                desc.Topology = value;
                primitiveTopology = Helper.Convert(value);
            }
        }

        public Vector4 BlendFactor
        {
            get => desc.BlendFactor;
            set => desc.BlendFactor = value;
        }

        public uint StencilRef
        {
            get => desc.StencilRef;
            set => desc.StencilRef = value;
        }

        public bool IsValid => pipeline.IsValid && isValid;

        public bool IsInitialized => pipeline.IsInitialized;

        public IResourceBindingList Bindings => resourceBindingList;

        public string DebugName => dbgName;

        internal override void SetState(ComPtr<ID3D11DeviceContext3> context)
        {
            context.VSSetShader(vs, null, 0);
            context.HSSetShader(hs, null, 0);
            context.DSSetShader(ds, null, 0);
            context.GSSetShader(gs, null, 0);
            context.PSSetShader(ps, null, 0);

            context.RSSetState(RasterizerState);

            var factor = desc.BlendFactor;
            float* fac = (float*)&factor;

            context.OMSetBlendState(BlendState, fac, uint.MaxValue);
            context.OMSetDepthStencilState(DepthStencilState, desc.StencilRef);
            context.IASetInputLayout(layout);
            context.IASetPrimitiveTopology(primitiveTopology);

            resourceBindingList?.BindGraphics(context);
        }

        internal override void UnsetState(ComPtr<ID3D11DeviceContext3> context)
        {
            context.VSSetShader((ID3D11VertexShader*)null, null, 0);
            context.HSSetShader((ID3D11HullShader*)null, null, 0);
            context.DSSetShader((ID3D11DomainShader*)null, null, 0);
            context.GSSetShader((ID3D11GeometryShader*)null, null, 0);
            context.PSSetShader((ID3D11PixelShader*)null, null, 0);

            context.RSSetState((ID3D11RasterizerState*)null);
            context.OMSetBlendState((ID3D11BlendState*)null, (float*)null, uint.MaxValue);
            context.OMSetDepthStencilState((ID3D11DepthStencilState*)null, 0);
            context.IASetInputLayout((ID3D11InputLayout*)null);
            context.IASetPrimitiveTopology(0);

            resourceBindingList?.UnbindGraphics(context);
        }

        protected override void DisposeCore()
        {
            PipelineStateManager.Unregister(this);

            pipeline.OnCompile -= OnPipelineCompile;
            pipeline.Dispose();

            resourceBindingList?.Dispose();

            if (layout.Handle != null)
            {
                layout.Release();
                layout = default;
            }

            if (RasterizerState.Handle != null)
            {
                RasterizerState.Release();
                RasterizerState = default;
            }

            if (DepthStencilState.Handle != null)
            {
                DepthStencilState.Release();
                DepthStencilState = default;
            }

            if (BlendState.Handle != null)
            {
                BlendState.Release();
                BlendState = default;
            }
        }
    }
}