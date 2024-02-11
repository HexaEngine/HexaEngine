namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using System.Collections.Generic;

    public struct ShaderParameter
    {
        public uint Index;
        public uint Size;
        public ShaderStage Stage;
        public ShaderParameterType Type;
    }

    public enum ShaderParameterType
    {
        SRV,
        UAV,
        CBV,
        Sampler,
    }

    public unsafe class D3D11ResourceBindingList : DisposableBase, IResourceBindingList
    {
        private readonly D3D11GraphicsPipeline pipeline;
        private readonly List<ShaderParameter> shaderParameters = [];
        private readonly Dictionary<string, ShaderParameter> shaderParametersByName = [];
        private readonly List<D3D11DescriptorRange> rangesSRVs = [];
        private readonly List<D3D11DescriptorRange> rangesUAVs = [];
        private readonly List<D3D11DescriptorRange> rangesCBVs = [];
        private readonly List<D3D11DescriptorRange> rangesSamplers = [];

        public D3D11ResourceBindingList(D3D11GraphicsPipeline pipeline)
        {
            pipeline.AddRef();
            pipeline.OnCompile += OnPipelineCompile;
            this.pipeline = pipeline;

            Reflect(pipeline.vertexShaderBlob, ShaderStage.Vertex);
            Reflect(pipeline.hullShaderBlob, ShaderStage.Hull);
            Reflect(pipeline.domainShaderBlob, ShaderStage.Domain);
            Reflect(pipeline.geometryShaderBlob, ShaderStage.Geometry);
            Reflect(pipeline.pixelShaderBlob, ShaderStage.Pixel);
        }

        private void OnPipelineCompile(IGraphicsPipeline pipe)
        {
            D3D11GraphicsPipeline pipeline = (D3D11GraphicsPipeline)pipe;
            Clear();
            Reflect(pipeline.vertexShaderBlob, ShaderStage.Vertex);
            Reflect(pipeline.hullShaderBlob, ShaderStage.Hull);
            Reflect(pipeline.domainShaderBlob, ShaderStage.Domain);
            Reflect(pipeline.geometryShaderBlob, ShaderStage.Geometry);
            Reflect(pipeline.pixelShaderBlob, ShaderStage.Pixel);
        }

        public void SetSRV(string name, IShaderResourceView srv)
        {
            for (int i = 0; i < rangesSRVs.Count; i++)
            {
                rangesSRVs[i].TrySetByName(name, (void*)srv.NativePointer);
            }
        }

        public void SetUAV(string name, IUnorderedAccessView uav)
        {
            for (int i = 0; i < rangesUAVs.Count; i++)
            {
                rangesUAVs[i].TrySetByName(name, (void*)uav.NativePointer);
            }
        }

        public void SetCBV(string name, IBuffer cbv)
        {
            for (int i = 0; i < rangesCBVs.Count; i++)
            {
                rangesCBVs[i].TrySetByName(name, (void*)cbv.NativePointer);
            }
        }

        public void SetSampler(string name, ISamplerState sampler)
        {
            for (int i = 0; i < rangesSamplers.Count; i++)
            {
                rangesSamplers[i].TrySetByName(name, (void*)sampler.NativePointer);
            }
        }

        public void SetSRV(string name, ShaderStage stage, IShaderResourceView srv)
        {
            rangesSRVs[(int)stage].TrySetByName(name, (void*)srv.NativePointer);
        }

        public void SetUAV(string name, ShaderStage stage, IUnorderedAccessView uav)
        {
            rangesUAVs[(int)stage].TrySetByName(name, (void*)uav.NativePointer);
        }

        public void SetCBV(string name, ShaderStage stage, IBuffer cbv)
        {
            rangesCBVs[(int)stage].TrySetByName(name, (void*)cbv.NativePointer);
        }

        public void SetSampler(string name, ShaderStage stage, ISamplerState sampler)
        {
            rangesSamplers[(int)stage].TrySetByName(name, (void*)sampler.NativePointer);
        }

        private class D3D11DescriptorRange
        {
            public D3D11DescriptorRange(ShaderStage stage, ShaderParameterType type, List<ShaderParameter> parameters, Dictionary<string, ShaderParameter> shaderParametersByName)
            {
                Stage = stage;
                Type = type;
                Parameters = parameters;
                ShaderParametersByName = shaderParametersByName;
                uint startSlot = uint.MaxValue;
                uint count = 0;
                for (int i = 0; i < parameters.Count; i++)
                {
                    ShaderParameter parameter = parameters[i];
                    startSlot = Math.Min(startSlot, parameter.Index);
                    count += parameter.Size;
                }

                StartSlot = startSlot;
                Count = count;

                if (count == 0)
                {
                    return;
                }
                Resources = AllocArrayAndZero(count);
            }

            public readonly ShaderStage Stage;
            public readonly ShaderParameterType Type;
            public readonly uint StartSlot;
            public readonly uint Count;
            public readonly void** Resources;

            public readonly List<ShaderParameter> Parameters;
            public readonly Dictionary<string, ShaderParameter> ShaderParametersByName;

            public void SetByName(string name, void* resource)
            {
                var parameter = ShaderParametersByName[name];
                Resources[parameter.Index] = resource;
            }

            public bool TrySetByName(string name, void* resource)
            {
                if (ShaderParametersByName.TryGetValue(name, out var parameter))
                {
                    Resources[parameter.Index] = resource;
                    return true;
                }
                return false;
            }

            public void Release()
            {
                if (Resources != null)
                {
                    Free(Resources);
                }
            }
        }

        private void Reflect(Shader* shader, ShaderStage stage)
        {
            if (shader == null)
            {
                rangesSRVs.Add(new(stage, ShaderParameterType.SRV, new(), new()));
                rangesUAVs.Add(new(stage, ShaderParameterType.UAV, new(), new()));
                rangesCBVs.Add(new(stage, ShaderParameterType.CBV, new(), new()));
                rangesSamplers.Add(new(stage, ShaderParameterType.Sampler, new(), new()));
                return;
            }

            ShaderCompiler.Reflect<ID3D11ShaderReflection>(shader, out var reflection);

            ShaderDesc shaderDesc;
            reflection.GetDesc(&shaderDesc);

            List<ShaderParameter> shaderParametersInStage = new();
            Dictionary<string, ShaderParameter> shaderParametersByNameInStage = new();

            for (uint i = 0; i < shaderDesc.BoundResources; i++)
            {
                ShaderInputBindDesc shaderInputBindDesc;
                reflection.GetResourceBindingDesc(i, &shaderInputBindDesc);
                ShaderParameter parameter;
                parameter.Index = shaderInputBindDesc.BindPoint;
                parameter.Size = shaderInputBindDesc.BindCount;
                parameter.Stage = stage;
                parameter.Type = shaderInputBindDesc.Type switch
                {
                    D3DShaderInputType.D3DSitCbuffer => ShaderParameterType.CBV,
                    D3DShaderInputType.D3DSitTbuffer => ShaderParameterType.SRV,
                    D3DShaderInputType.D3DSitTexture => ShaderParameterType.SRV,
                    D3DShaderInputType.D3DSitSampler => ShaderParameterType.Sampler,
                    D3DShaderInputType.D3DSitUavRwtyped => ShaderParameterType.UAV,
                    D3DShaderInputType.D3DSitStructured => ShaderParameterType.SRV,
                    D3DShaderInputType.D3DSitUavRwstructured => ShaderParameterType.UAV,
                    D3DShaderInputType.D3DSitByteaddress => ShaderParameterType.SRV,
                    D3DShaderInputType.D3DSitUavRwbyteaddress => ShaderParameterType.UAV,
                    D3DShaderInputType.D3DSitUavAppendStructured => ShaderParameterType.UAV,
                    D3DShaderInputType.D3DSitUavConsumeStructured => ShaderParameterType.UAV,
                    D3DShaderInputType.D3DSitUavRwstructuredWithCounter => ShaderParameterType.UAV,
                    D3DShaderInputType.D3DSitRtaccelerationstructure => throw new NotSupportedException("Ray tracing is not supported in D3D11!"),
                    D3DShaderInputType.D3DSitUavFeedbacktexture => ShaderParameterType.UAV,
                    _ => throw new NotSupportedException($"ShaderInputType ({shaderInputBindDesc.Type}) is not supported!"),
                };
                shaderParameters.Add(parameter);

                string name = ToStringFromUTF8(shaderInputBindDesc.Name) ?? throw new Exception("Name cannot be null, check your shader code ensure all resources are named!");
                shaderParametersByName.Add(name, parameter);

                shaderParametersInStage.Add(parameter);
                shaderParametersByNameInStage.Add(name, parameter);
            }

            {
                rangesSRVs.Add(new(stage, ShaderParameterType.SRV, shaderParametersInStage.Where(x => x.Type == ShaderParameterType.SRV).ToList(), shaderParametersByNameInStage.Where(x => x.Value.Type == ShaderParameterType.SRV).ToDictionary()));
                rangesUAVs.Add(new(stage, ShaderParameterType.UAV, shaderParametersInStage.Where(x => x.Type == ShaderParameterType.UAV).ToList(), shaderParametersByNameInStage.Where(x => x.Value.Type == ShaderParameterType.UAV).ToDictionary()));
                rangesCBVs.Add(new(stage, ShaderParameterType.CBV, shaderParametersInStage.Where(x => x.Type == ShaderParameterType.CBV).ToList(), shaderParametersByNameInStage.Where(x => x.Value.Type == ShaderParameterType.CBV).ToDictionary()));
                rangesSamplers.Add(new(stage, ShaderParameterType.Sampler, shaderParametersInStage.Where(x => x.Type == ShaderParameterType.Sampler).ToList(), shaderParametersByNameInStage.Where(x => x.Value.Type == ShaderParameterType.Sampler).ToDictionary()));
            }

            reflection.Release();
        }

        private void Clear()
        {
            shaderParameters.Clear();
            shaderParametersByName.Clear();
            for (int i = 0; i < rangesSRVs.Count; i++)
            {
                rangesSRVs[i].Release();
            }
            rangesSRVs.Clear();
            for (int i = 0; i < rangesUAVs.Count; i++)
            {
                rangesUAVs[i].Release();
            }
            rangesUAVs.Clear();
            for (int i = 0; i < rangesCBVs.Count; i++)
            {
                rangesCBVs[i].Release();
            }
            rangesCBVs.Clear();
            for (int i = 0; i < rangesSamplers.Count; i++)
            {
                rangesSamplers[i].Release();
            }
            rangesSamplers.Clear();
        }

        public void BindGraphics(ComPtr<ID3D11DeviceContext3> context)
        {
            var vertexStageSRVRange = rangesSRVs[(int)ShaderStage.Vertex];
            if (vertexStageSRVRange.Count > 0)
            {
                context.VSSetShaderResources(vertexStageSRVRange.StartSlot, vertexStageSRVRange.Count, (ID3D11ShaderResourceView**)vertexStageSRVRange.Resources);
            }

            var hullStageSRVRange = rangesSRVs[(int)ShaderStage.Hull];
            if (hullStageSRVRange.Count > 0)
            {
                context.HSSetShaderResources(hullStageSRVRange.StartSlot, hullStageSRVRange.Count, (ID3D11ShaderResourceView**)hullStageSRVRange.Resources);
            }

            var domainStageSRVRange = rangesSRVs[(int)ShaderStage.Domain];
            if (domainStageSRVRange.Count > 0)
            {
                context.DSSetShaderResources(domainStageSRVRange.StartSlot, domainStageSRVRange.Count, (ID3D11ShaderResourceView**)domainStageSRVRange.Resources);
            }

            var geometryStageSRVRange = rangesSRVs[(int)ShaderStage.Geometry];
            if (geometryStageSRVRange.Count > 0)
            {
                context.GSSetShaderResources(geometryStageSRVRange.StartSlot, geometryStageSRVRange.Count, (ID3D11ShaderResourceView**)geometryStageSRVRange.Resources);
            }

            var pixelStageSRVRange = rangesSRVs[(int)ShaderStage.Pixel];
            if (pixelStageSRVRange.Count > 0)
            {
                context.PSSetShaderResources(pixelStageSRVRange.StartSlot, pixelStageSRVRange.Count, (ID3D11ShaderResourceView**)pixelStageSRVRange.Resources);
            }

            // CBV

            var vertexStageCBVRange = rangesCBVs[(int)ShaderStage.Vertex];
            if (vertexStageCBVRange.Count > 0)
            {
                context.VSSetConstantBuffers(vertexStageCBVRange.StartSlot, vertexStageCBVRange.Count, (ID3D11Buffer**)vertexStageCBVRange.Resources);
            }

            var hullStageCBVRange = rangesCBVs[(int)ShaderStage.Hull];
            if (hullStageCBVRange.Count > 0)
            {
                context.HSSetConstantBuffers(hullStageCBVRange.StartSlot, hullStageCBVRange.Count, (ID3D11Buffer**)hullStageCBVRange.Resources);
            }

            var domainStageCBVRange = rangesCBVs[(int)ShaderStage.Domain];
            if (domainStageCBVRange.Count > 0)
            {
                context.DSSetConstantBuffers(domainStageCBVRange.StartSlot, domainStageCBVRange.Count, (ID3D11Buffer**)domainStageCBVRange.Resources);
            }

            var geometryStageCBVRange = rangesCBVs[(int)ShaderStage.Geometry];
            if (geometryStageCBVRange.Count > 0)
            {
                context.GSSetConstantBuffers(geometryStageCBVRange.StartSlot, geometryStageCBVRange.Count, (ID3D11Buffer**)geometryStageCBVRange.Resources);
            }

            var pixelStageCBVRange = rangesCBVs[(int)ShaderStage.Pixel];
            if (pixelStageCBVRange.Count > 0)
            {
                context.PSSetConstantBuffers(pixelStageCBVRange.StartSlot, pixelStageCBVRange.Count, (ID3D11Buffer**)pixelStageCBVRange.Resources);
            }

            // Sampler

            var vertexStageSamplerRange = rangesSamplers[(int)ShaderStage.Vertex];
            if (vertexStageSamplerRange.Count > 0)
            {
                context.VSSetSamplers(vertexStageSamplerRange.StartSlot, vertexStageSamplerRange.Count, (ID3D11SamplerState**)vertexStageSamplerRange.Resources);
            }

            var hullStageSamplerRange = rangesSamplers[(int)ShaderStage.Hull];
            if (hullStageSamplerRange.Count > 0)
            {
                context.HSSetSamplers(hullStageSamplerRange.StartSlot, hullStageSamplerRange.Count, (ID3D11SamplerState**)hullStageSamplerRange.Resources);
            }

            var domainStageSamplerRange = rangesSamplers[(int)ShaderStage.Domain];
            if (domainStageSamplerRange.Count > 0)
            {
                context.DSSetSamplers(domainStageSamplerRange.StartSlot, domainStageSamplerRange.Count, (ID3D11SamplerState**)domainStageSamplerRange.Resources);
            }

            var geometryStageSamplerRange = rangesSamplers[(int)ShaderStage.Geometry];
            if (geometryStageSamplerRange.Count > 0)
            {
                context.GSSetSamplers(geometryStageSamplerRange.StartSlot, geometryStageSamplerRange.Count, (ID3D11SamplerState**)geometryStageSamplerRange.Resources);
            }

            var pixelStageSamplerRange = rangesSamplers[(int)ShaderStage.Pixel];
            if (pixelStageSamplerRange.Count > 0)
            {
                context.PSSetSamplers(pixelStageSamplerRange.StartSlot, pixelStageSamplerRange.Count, (ID3D11SamplerState**)pixelStageSamplerRange.Resources);
            }
        }

        public void UnbindGraphics(ComPtr<ID3D11DeviceContext3> context)
        {
            var vertexStageSRVRange = rangesSRVs[(int)ShaderStage.Vertex];
            if (vertexStageSRVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)vertexStageSRVRange.Count];
                context.VSSetShaderResources(vertexStageSRVRange.StartSlot, vertexStageSRVRange.Count, (ID3D11ShaderResourceView**)empty);
            }

            var hullStageSRVRange = rangesSRVs[(int)ShaderStage.Hull];
            if (hullStageSRVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)hullStageSRVRange.Count];
                context.HSSetShaderResources(hullStageSRVRange.StartSlot, hullStageSRVRange.Count, (ID3D11ShaderResourceView**)empty);
            }

            var domainStageSRVRange = rangesSRVs[(int)ShaderStage.Domain];
            if (domainStageSRVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)domainStageSRVRange.Count];
                context.DSSetShaderResources(domainStageSRVRange.StartSlot, domainStageSRVRange.Count, (ID3D11ShaderResourceView**)empty);
            }

            var geometryStageSRVRange = rangesSRVs[(int)ShaderStage.Geometry];
            if (geometryStageSRVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)geometryStageSRVRange.Count];
                context.GSSetShaderResources(geometryStageSRVRange.StartSlot, geometryStageSRVRange.Count, (ID3D11ShaderResourceView**)empty);
            }

            var pixelStageSRVRange = rangesSRVs[(int)ShaderStage.Pixel];
            if (pixelStageSRVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)pixelStageSRVRange.Count];
                context.PSSetShaderResources(pixelStageSRVRange.StartSlot, pixelStageSRVRange.Count, (ID3D11ShaderResourceView**)empty);
            }

            // CBV

            var vertexStageCBVRange = rangesCBVs[(int)ShaderStage.Vertex];
            if (vertexStageCBVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)vertexStageCBVRange.Count];
                context.VSSetConstantBuffers(vertexStageCBVRange.StartSlot, vertexStageCBVRange.Count, (ID3D11Buffer**)empty);
            }

            var hullStageCBVRange = rangesCBVs[(int)ShaderStage.Hull];
            if (hullStageCBVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)hullStageCBVRange.Count];
                context.HSSetConstantBuffers(hullStageCBVRange.StartSlot, hullStageCBVRange.Count, (ID3D11Buffer**)empty);
            }

            var domainStageCBVRange = rangesCBVs[(int)ShaderStage.Domain];
            if (domainStageCBVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)domainStageCBVRange.Count];
                context.DSSetConstantBuffers(domainStageCBVRange.StartSlot, domainStageCBVRange.Count, (ID3D11Buffer**)empty);
            }

            var geometryStageCBVRange = rangesCBVs[(int)ShaderStage.Geometry];
            if (geometryStageCBVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)geometryStageCBVRange.Count];
                context.GSSetConstantBuffers(geometryStageCBVRange.StartSlot, geometryStageCBVRange.Count, (ID3D11Buffer**)empty);
            }

            var pixelStageCBVRange = rangesCBVs[(int)ShaderStage.Pixel];
            if (pixelStageCBVRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)pixelStageCBVRange.Count];
                context.PSSetConstantBuffers(pixelStageCBVRange.StartSlot, pixelStageCBVRange.Count, (ID3D11Buffer**)empty);
            }

            // Sampler

            var vertexStageSamplerRange = rangesSamplers[(int)ShaderStage.Vertex];
            if (vertexStageSamplerRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)vertexStageSamplerRange.Count];
                context.VSSetSamplers(vertexStageSamplerRange.StartSlot, vertexStageSamplerRange.Count, (ID3D11SamplerState**)empty);
            }

            var hullStageSamplerRange = rangesSamplers[(int)ShaderStage.Hull];
            if (hullStageSamplerRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)hullStageSamplerRange.Count];
                context.HSSetSamplers(hullStageSamplerRange.StartSlot, hullStageSamplerRange.Count, (ID3D11SamplerState**)empty);
            }

            var domainStageSamplerRange = rangesSamplers[(int)ShaderStage.Domain];
            if (domainStageSamplerRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)domainStageSamplerRange.Count];
                context.DSSetSamplers(domainStageSamplerRange.StartSlot, domainStageSamplerRange.Count, (ID3D11SamplerState**)empty);
            }

            var geometryStageSamplerRange = rangesSamplers[(int)ShaderStage.Geometry];
            if (geometryStageSamplerRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)geometryStageSamplerRange.Count];
                context.GSSetSamplers(geometryStageSamplerRange.StartSlot, geometryStageSamplerRange.Count, (ID3D11SamplerState**)empty);
            }

            var pixelStageSamplerRange = rangesSamplers[(int)ShaderStage.Pixel];
            if (pixelStageSamplerRange.Count > 0)
            {
                nint* empty = stackalloc nint[(int)pixelStageSamplerRange.Count];
                context.PSSetSamplers(pixelStageSamplerRange.StartSlot, pixelStageSamplerRange.Count, (ID3D11SamplerState**)empty);
            }
        }

        protected override void DisposeCore()
        {
            pipeline.OnCompile -= OnPipelineCompile;
            pipeline.Dispose();

            Clear();
        }
    }
}