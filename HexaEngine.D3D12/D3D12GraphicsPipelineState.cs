namespace HexaEngine.D3D12
{
    using Hexa.NET.D3D12;
    using Hexa.NET.D3DCommon;
    using Format = Hexa.NET.DXGI.Format;
    using GraphicsPipelineStateDesc = Core.Graphics.GraphicsPipelineStateDesc;

    public unsafe class D3D12GraphicsPipelineState : DisposableBase
    {
        private ComPtr<ID3D10Blob> cachedBlob;
        private UnsafeDictionary<PSOOutput, ComPtr<ID3D12PipelineState>> states = [];
        private Hexa.NET.D3D12.GraphicsPipelineStateDesc stateDesc;

        public D3D12GraphicsPipelineState(ComPtr<ID3D12Device10> device, D3D12GraphicsPipeline pipeline, GraphicsPipelineStateDesc desc)
        {
            Hexa.NET.D3D12.GraphicsPipelineStateDesc stateDesc = new()
            {
                PRootSignature = pipeline.rootSignature,
                VS = pipeline.vertexShaderBlob->ToShaderBytecode(),
                HS = pipeline.hullShaderBlob->ToShaderBytecode(),
                DS = pipeline.domainShaderBlob->ToShaderBytecode(),
                GS = pipeline.geometryShaderBlob->ToShaderBytecode(),
                PS = pipeline.pixelShaderBlob->ToShaderBytecode(),
                BlendState = Helper.Convert(desc.Blend),
                RasterizerState = Helper.Convert(desc.Rasterizer),
                DepthStencilState = Helper.Convert(desc.DepthStencil),
                PrimitiveTopologyType = Helper.ConvertType(desc.Topology),
                SampleMask = desc.SampleMask,
                InputLayout = pipeline.inputLayoutDesc,
            };
            this.stateDesc = stateDesc;

            device.CreateGraphicsPipelineState(&stateDesc, out ComPtr<ID3D12PipelineState> cachedPSO).ThrowIf();

            cachedPSO.GetCachedBlob(out cachedBlob).ThrowIf();

            cachedPSO.Release();
        }

        public ComPtr<ID3D12PipelineState> GetPSO(ComPtr<ID3D12Device10> device, PSOOutput output)
        {
            if (states.TryGetValue(output, out var pso))
            {
                return pso;
            }

            var stateDesc = this.stateDesc;

            stateDesc.CachedPSO = new(cachedBlob.GetBufferPointer(), cachedBlob.GetBufferSize());

            uint num = stateDesc.NumRenderTargets = output.NumRenderTargets;

            Format* src = &output.RTVFormats_0;
            Format* dst = &stateDesc.RTVFormats_0;

            while (num > 0)
            {
                *dst++ = *src++;
                num--;
            }

            stateDesc.DSVFormat = output.DSVFormat;
            stateDesc.SampleDesc = output.SampleDesc;

            device.CreateGraphicsPipelineState(&stateDesc, out pso);

            states.Add(output, pso);

            return pso;
        }

        protected override void DisposeCore()
        {
            foreach (var state in states)
            {
                state.Value.Release();
            }
            states.Clear();
            states.Release();

            if (cachedBlob.Handle != null)
            {
                cachedBlob.Release();
                cachedBlob = default;
            }
        }
    }
}