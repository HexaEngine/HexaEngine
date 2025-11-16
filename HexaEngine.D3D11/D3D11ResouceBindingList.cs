namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Collections.Generic;

    public unsafe struct D3D11VariableListRange
    {
        public ShaderStage Stage;
        public UnsafeList<D3D11VariableList> Lists;

        public D3D11VariableListRange(ShaderStage stage, UnsafeList<D3D11VariableList> lists)
        {
            Stage = stage;
            Lists = lists;
        }

        public void TrySetByName<T>(string name, in T value) where T : unmanaged
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists.GetPointer(i)->TrySetByName(name, in value);
            }
        }

        public void TrySetByName<T>(string name, T* values, uint count) where T : unmanaged
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists.GetPointer(i)->TrySetByName(name, values, count);
            }
        }

        public void TrySetByName<T>(string name, ReadOnlySpan<T> span) where T : unmanaged
        {
            fixed (T* ptr = span)
            {
                TrySetByName(name, ptr, (uint)span.Length);
            }
        }

        public void TrySetUnbound<T>(in T value) where T : unmanaged
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists.GetPointer(i)->TrySetUnbound(in value);
            }
        }

        public void Release()
        {
            for (int i = 0; i < Lists.Count; i++)
            {
                Lists[i].Release();
            }
            Lists.Release();
        }
    }

    public unsafe class D3D11ResourceBindingList : DisposableBase, IResourceBindingList
    {
        private readonly IPipeline pipeline;
        private readonly PipelineStateFlags flags;
        private UnsafeList<D3D11DescriptorRange> rangesSRVs;
        private UnsafeList<D3D11DescriptorRange> rangesUAVs;
        private UnsafeList<D3D11DescriptorRange> rangesCBVs;
        private UnsafeList<D3D11DescriptorRange> rangesSamplers;
        private UnsafeList<D3D11VariableListRange> rangesVariables;

        public D3D11ResourceBindingList(D3D11GraphicsPipeline pipeline, PipelineStateFlags flags)
        {
            pipeline.AddRef();
            pipeline.OnCompile += OnPipelineCompile;
            this.pipeline = pipeline;
            this.flags = flags;
            OnPipelineCompile(pipeline);
        }

        public D3D11ResourceBindingList(D3D11ComputePipeline pipeline, PipelineStateFlags flags)
        {
            pipeline.AddRef();
            pipeline.OnCompile += OnPipelineCompile;
            this.pipeline = pipeline;
            this.flags = flags;
            OnPipelineCompile(pipeline);
        }

        public IPipeline Pipeline => pipeline;

        public IEnumerable<BindingValuePair> SRVs => EnumerateRanges(rangesSRVs);

        public IEnumerable<BindingValuePair> CBVs => EnumerateRanges(rangesCBVs);

        public IEnumerable<BindingValuePair> UAVs => EnumerateRanges(rangesUAVs);

        public IEnumerable<BindingValuePair> Samplers => EnumerateRanges(rangesSamplers);

        internal void Hook()
        {
            D3D11GlobalResourceList.StateChanged += GlobalStateChanged;
        }

        private void GlobalStateChanged(string name, D3D11ShaderParameterState oldState, D3D11ShaderParameterState state)
        {
            switch (state.Type)
            {
                case ShaderParameterType.SRV:
                    for (int i = 0; i < rangesSRVs.Count; i++)
                    {
                        rangesSRVs.GetPointer(i)->UpdateByName(name, oldState.Resource, state.Resource);
                    }
                    break;

                case ShaderParameterType.UAV:
                    for (int i = 0; i < rangesUAVs.Count; i++)
                    {
                        rangesUAVs.GetPointer(i)->UpdateByName(name, oldState.Resource, state.Resource, state.InitialCount);
                    }
                    break;

                case ShaderParameterType.CBV:
                    for (int i = 0; i < rangesCBVs.Count; i++)
                    {
                        rangesCBVs.GetPointer(i)->UpdateByName(name, oldState.Resource, state.Resource);
                    }
                    break;

                case ShaderParameterType.Sampler:
                    for (int i = 0; i < rangesSamplers.Count; i++)
                    {
                        rangesSamplers.GetPointer(i)->UpdateByName(name, oldState.Resource, state.Resource);
                    }
                    break;
            }
        }

        private void OnPipelineCompile(IPipeline pipeline)
        {
            Clear();
            if (pipeline is D3D11GraphicsPipeline graphicsPipeline)
            {
                Reflect(graphicsPipeline.vertexShaderBlob, ShaderStage.Vertex);
                Reflect(graphicsPipeline.hullShaderBlob, ShaderStage.Hull);
                Reflect(graphicsPipeline.domainShaderBlob, ShaderStage.Domain);
                Reflect(graphicsPipeline.geometryShaderBlob, ShaderStage.Geometry);
                Reflect(graphicsPipeline.pixelShaderBlob, ShaderStage.Pixel);
            }

            if (pipeline is D3D11ComputePipeline computePipeline)
            {
                Reflect(computePipeline.computeShaderBlob, ShaderStage.Compute);
            }

            rangesSRVs.ShrinkToFit();
            rangesUAVs.ShrinkToFit();
            rangesCBVs.ShrinkToFit();
            rangesSamplers.ShrinkToFit();

            D3D11GlobalResourceList.SetState(this);
        }

        public void SetSRV(string name, IShaderResourceView? srv)
        {
            var p = srv?.NativePointer ?? 0;
            for (int i = 0; i < rangesSRVs.Count; i++)
            {
                rangesSRVs.GetPointer(i)->TrySetByName(name, (void*)p);
            }
        }

        internal void SetSRV(string name, void* srv)
        {
            for (int i = 0; i < rangesSRVs.Count; i++)
            {
                rangesSRVs.GetPointer(i)->TrySetByName(name, srv);
            }
        }

        public void SetUAV(string name, IUnorderedAccessView? uav, uint initialCount = unchecked((uint)-1))
        {
            var p = uav?.NativePointer ?? 0;
            for (int i = 0; i < rangesUAVs.Count; i++)
            {
                rangesUAVs.GetPointer(i)->TrySetByName(name, (void*)p, initialCount);
            }
        }

        public void SetUAV(string name, void* uav, uint initialCount = unchecked((uint)-1))
        {
            for (int i = 0; i < rangesUAVs.Count; i++)
            {
                rangesUAVs.GetPointer(i)->TrySetByName(name, uav, initialCount);
            }
        }

        public void SetCBV(string name, IBuffer? cbv)
        {
            var p = cbv?.NativePointer ?? 0;
            for (int i = 0; i < rangesCBVs.Count; i++)
            {
                rangesCBVs.GetPointer(i)->TrySetByName(name, (void*)p);
            }
        }

        public void SetCBV(string name, void* cbv)
        {
            for (int i = 0; i < rangesCBVs.Count; i++)
            {
                rangesCBVs.GetPointer(i)->TrySetByName(name, (void*)cbv);
            }
        }

        public void SetSampler(string name, ISamplerState? sampler)
        {
            var p = sampler?.NativePointer ?? 0;
            for (int i = 0; i < rangesSamplers.Count; i++)
            {
                rangesSamplers.GetPointer(i)->TrySetByName(name, (void*)p);
            }
        }

        public void SetSampler(string name, void* sampler)
        {
            for (int i = 0; i < rangesSamplers.Count; i++)
            {
                rangesSamplers.GetPointer(i)->TrySetByName(name, (void*)sampler);
            }
        }

        public void SetVariable<T>(string name, in T value) where T : unmanaged
        {
            for (int i = 0; i < rangesVariables.Count; i++)
            {
                rangesVariables.GetPointer(i)->TrySetByName(name, value);
            }
        }

        public void SetSRV(string name, ShaderStage stage, IShaderResourceView? srv)
        {
            var p = srv?.NativePointer ?? 0;
            rangesSRVs[(int)stage].TrySetByName(name, (void*)p);
        }

        public void SetUAV(string name, ShaderStage stage, IUnorderedAccessView? uav, uint initialCount = unchecked((uint)-1))
        {
            var p = uav?.NativePointer ?? 0;
            rangesUAVs[(int)stage].TrySetByName(name, (void*)p, initialCount);
        }

        public void SetCBV(string name, ShaderStage stage, IBuffer? cbv)
        {
            var p = cbv?.NativePointer ?? 0;
            rangesCBVs[(int)stage].TrySetByName(name, (void*)p);
        }

        public void SetSampler(string name, ShaderStage stage, ISamplerState? sampler)
        {
            var p = sampler?.NativePointer ?? 0;
            rangesSamplers[(int)stage].TrySetByName(name, (void*)p);
        }

        public void SetVariable<T>(string name, ShaderStage stage, in T value) where T : unmanaged
        {
            rangesVariables[(int)stage].TrySetByName(name, value);
        }

        private D3D11GraphicsDevice GetDevice()
        {
            if (pipeline is D3D11GraphicsPipeline graphicsPipeline)
            {
                return graphicsPipeline.Device;
            }
            else if (pipeline is D3D11ComputePipeline computePipeline)
            {
                return computePipeline.Device;
            }

            throw new InvalidOperationException("Pipeline is not a D3D11 pipeline.");
        }

        private void Reflect(Shader* shader, ShaderStage stage)
        {
            if (shader == null)
            {
                rangesSRVs.Add(new(stage, ShaderParameterType.SRV, []));
                rangesUAVs.Add(new(stage, ShaderParameterType.UAV, []));
                rangesCBVs.Add(new(stage, ShaderParameterType.CBV, []));
                rangesSamplers.Add(new(stage, ShaderParameterType.Sampler, []));
                rangesVariables.Add(new(stage, []));
                return;
            }

            ShaderCompiler.Reflect<ID3D11ShaderReflection>(shader, out var reflection);

            ShaderDesc shaderDesc;
            reflection.GetDesc(&shaderDesc).ThrowIf();

            List<D3D11ShaderParameter> shaderParametersInStage = [];
            Dictionary<string, D3D11ShaderParameter> shaderParametersByNameInStage = [];

            for (uint i = 0; i < shaderDesc.BoundResources; i++)
            {
                ShaderInputBindDesc shaderInputBindDesc;
                reflection.GetResourceBindingDesc(i, &shaderInputBindDesc);
                D3D11ShaderParameter parameter;

                parameter.Index = shaderInputBindDesc.BindPoint;
                parameter.Size = shaderInputBindDesc.BindCount;
                parameter.Stage = stage;
                parameter.Type = shaderInputBindDesc.Type switch
                {
                    ShaderInputType.SitCbuffer => ShaderParameterType.CBV,
                    ShaderInputType.SitTbuffer => ShaderParameterType.SRV,
                    ShaderInputType.SitTexture => ShaderParameterType.SRV,
                    ShaderInputType.SitSampler => ShaderParameterType.Sampler,
                    ShaderInputType.SitUavRwtyped => ShaderParameterType.UAV,
                    ShaderInputType.SitStructured => ShaderParameterType.SRV,
                    ShaderInputType.SitUavRwstructured => ShaderParameterType.UAV,
                    ShaderInputType.SitByteaddress => ShaderParameterType.SRV,
                    ShaderInputType.SitUavRwbyteaddress => ShaderParameterType.UAV,
                    ShaderInputType.SitUavAppendStructured => ShaderParameterType.UAV,
                    ShaderInputType.SitUavConsumeStructured => ShaderParameterType.UAV,
                    ShaderInputType.SitUavRwstructuredWithCounter => ShaderParameterType.UAV,
                    ShaderInputType.SitRtaccelerationstructure => throw new NotSupportedException("Ray tracing is not supported in D3D11!"),
                    ShaderInputType.SitUavFeedbacktexture => ShaderParameterType.UAV,
                    _ => throw new NotSupportedException($"ShaderInputType ({shaderInputBindDesc.Type}) is not supported!"),
                };

                string name = ToStringFromUTF8(shaderInputBindDesc.Name) ?? throw new Exception("Name cannot be null, check your shader code ensure all resources are named!");

                parameter.Hash = (uint)name.GetHashCode();
                parameter.Name = name.ToUTF16Ptr();

                shaderParametersInStage.Add(parameter);
                shaderParametersByNameInStage.Add(name, parameter);
            }

            rangesSRVs.PushBack(new(stage, ShaderParameterType.SRV, [.. shaderParametersInStage.Where(x => x.Type == ShaderParameterType.SRV)]));
            rangesUAVs.PushBack(new(stage, ShaderParameterType.UAV, [.. shaderParametersInStage.Where(x => x.Type == ShaderParameterType.UAV)]));
            rangesCBVs.PushBack(new(stage, ShaderParameterType.CBV, [.. shaderParametersInStage.Where(x => x.Type == ShaderParameterType.CBV)]));
            rangesSamplers.PushBack(new(stage, ShaderParameterType.Sampler, [.. shaderParametersInStage.Where(x => x.Type == ShaderParameterType.Sampler)]));

            if ((flags & PipelineStateFlags.ReflectVariables) != 0)
            {
                var device = GetDevice();
                var numCbs = shaderDesc.ConstantBuffers;
                UnsafeList<D3D11VariableList> variableLists = [];
                for (uint i = 0; i < numCbs; ++i)
                {
                    var cb = reflection.GetConstantBufferByIndex(i);
                    ShaderBufferDesc cbDesc;
                    cb->GetDesc(&cbDesc);
                    var name = ToStringFromUTF8(cbDesc.Name) ?? throw new Exception("Name cannot be null, check your shader code ensure all resources are named!");

                    if (name.StartsWith("Global"))
                    {
                        continue;
                    }

                    var list = D3D11VariableList.CreateFrom(device, stage, cb);
                    variableLists.PushBack(list);

                    SetCBV(name, list.Buffer.Handle);
                }

                rangesVariables.PushBack(new(stage, variableLists));
            }

            reflection.Release();
        }

        private void Clear()
        {
            for (int i = 0; i < rangesSRVs.Count; i++)
            {
                rangesSRVs[i].Release();
            }
            rangesSRVs.Release();
            for (int i = 0; i < rangesUAVs.Count; i++)
            {
                rangesUAVs[i].Release();
            }
            rangesUAVs.Release();
            for (int i = 0; i < rangesCBVs.Count; i++)
            {
                rangesCBVs[i].Release();
            }
            rangesCBVs.Release();
            for (int i = 0; i < rangesSamplers.Count; i++)
            {
                rangesSamplers[i].Release();
            }
            rangesSamplers.Release();
            for (int i = 0; i < rangesVariables.Count; i++)
            {
                rangesVariables[i].Release();
            }
            rangesVariables.Release();
        }

        public void BindGraphics(ComPtr<ID3D11DeviceContext3> context)
        {
            // SRV
            rangesSRVs[(int)ShaderStage.Vertex].Bind(context, context.Handle->LpVtbl[25]); // V-Table Index 25 VSSetShaderResources.
            rangesSRVs[(int)ShaderStage.Hull].Bind(context, context.Handle->LpVtbl[59]); // V-Table Index 59 HSSetShaderResources.
            rangesSRVs[(int)ShaderStage.Domain].Bind(context, context.Handle->LpVtbl[63]); // V-Table Index 63 DSSetShaderResources.
            rangesSRVs[(int)ShaderStage.Geometry].Bind(context, context.Handle->LpVtbl[31]); // V-Table Index 31 GSSetShaderResources.
            rangesSRVs[(int)ShaderStage.Pixel].Bind(context, context.Handle->LpVtbl[8]); // V-Table Index 8 PSSetShaderResources.

            // CBV
            rangesCBVs[(int)ShaderStage.Vertex].Bind(context, context.Handle->LpVtbl[7]); // V-Table Index 7 VSSetConstantBuffers.
            rangesCBVs[(int)ShaderStage.Hull].Bind(context, context.Handle->LpVtbl[62]); // V-Table Index 62 HSSetConstantBuffers.
            rangesCBVs[(int)ShaderStage.Domain].Bind(context, context.Handle->LpVtbl[66]); // V-Table Index 66 DSSetConstantBuffers.
            rangesCBVs[(int)ShaderStage.Geometry].Bind(context, context.Handle->LpVtbl[22]); // V-Table Index 22 GSSetConstantBuffers.
            rangesCBVs[(int)ShaderStage.Pixel].Bind(context, context.Handle->LpVtbl[16]); // V-Table Index 16 PSSetConstantBuffers.

            // Sampler
            rangesSamplers[(int)ShaderStage.Vertex].Bind(context, context.Handle->LpVtbl[26]); // V-Table Index 26 VSSetSamplers.
            rangesSamplers[(int)ShaderStage.Hull].Bind(context, context.Handle->LpVtbl[61]); // V-Table Index 61 HSSetSamplers.
            rangesSamplers[(int)ShaderStage.Domain].Bind(context, context.Handle->LpVtbl[65]); // V-Table Index 65 DSSetSamplers.
            rangesSamplers[(int)ShaderStage.Geometry].Bind(context, context.Handle->LpVtbl[32]); // V-Table Index 32 GSSetSamplers.
            rangesSamplers[(int)ShaderStage.Pixel].Bind(context, context.Handle->LpVtbl[10]); // V-Table Index 10 PSSetSamplers.
        }

        public void UnbindGraphics(ComPtr<ID3D11DeviceContext3> context)
        {
            // SRV
            rangesSRVs[(int)ShaderStage.Vertex].Unbind(context, context.Handle->LpVtbl[25]); // V-Table Index 25 VSSetShaderResources.
            rangesSRVs[(int)ShaderStage.Hull].Unbind(context, context.Handle->LpVtbl[59]); // V-Table Index 59 HSSetShaderResources.
            rangesSRVs[(int)ShaderStage.Domain].Unbind(context, context.Handle->LpVtbl[63]); // V-Table Index 63 DSSetShaderResources.
            rangesSRVs[(int)ShaderStage.Geometry].Unbind(context, context.Handle->LpVtbl[31]); // V-Table Index 31 GSSetShaderResources.
            rangesSRVs[(int)ShaderStage.Pixel].Unbind(context, context.Handle->LpVtbl[8]); // V-Table Index 8 PSSetShaderResources.

            // CBV
            rangesCBVs[(int)ShaderStage.Vertex].Unbind(context, context.Handle->LpVtbl[7]); // V-Table Index 7 VSSetConstantBuffers.
            rangesCBVs[(int)ShaderStage.Hull].Unbind(context, context.Handle->LpVtbl[62]); // V-Table Index 62 HSSetConstantBuffers.
            rangesCBVs[(int)ShaderStage.Domain].Unbind(context, context.Handle->LpVtbl[66]); // V-Table Index 66 DSSetConstantBuffers.
            rangesCBVs[(int)ShaderStage.Geometry].Unbind(context, context.Handle->LpVtbl[22]); // V-Table Index 22 GSSetConstantBuffers.
            rangesCBVs[(int)ShaderStage.Pixel].Unbind(context, context.Handle->LpVtbl[16]); // V-Table Index 16 PSSetConstantBuffers.

            // Sampler
            rangesSamplers[(int)ShaderStage.Vertex].Unbind(context, context.Handle->LpVtbl[26]); // V-Table Index 26 VSSetSamplers.
            rangesSamplers[(int)ShaderStage.Hull].Unbind(context, context.Handle->LpVtbl[61]); // V-Table Index 61 HSSetSamplers.
            rangesSamplers[(int)ShaderStage.Domain].Unbind(context, context.Handle->LpVtbl[65]); // V-Table Index 65 DSSetSamplers.
            rangesSamplers[(int)ShaderStage.Geometry].Unbind(context, context.Handle->LpVtbl[32]); // V-Table Index 32 GSSetSamplers.
            rangesSamplers[(int)ShaderStage.Pixel].Unbind(context, context.Handle->LpVtbl[10]); // V-Table Index 10 PSSetSamplers.
        }

        public void BindCompute(ComPtr<ID3D11DeviceContext3> context)
        {
            // UAV
            rangesUAVs[0].BindUAV(context); // V-Table Index 68 CSSetUnorderedAccessViews.

            // SRV
            rangesSRVs[0].Bind(context, context.Handle->LpVtbl[67]); // V-Table Index 67 CSSetShaderResources.

            // CBV
            rangesCBVs[0].Bind(context, context.Handle->LpVtbl[71]); // V-Table Index 71 CSSetConstantBuffers.

            // Sampler
            rangesSamplers[0].Bind(context, context.Handle->LpVtbl[70]); // V-Table Index 70 CSSetSamplers.
        }

        public void UnbindCompute(ComPtr<ID3D11DeviceContext3> context)
        {
            // UAV
            rangesUAVs[0].UnbindUAV(context); // V-Table Index 68 CSSetUnorderedAccessViews.

            // SRV
            rangesSRVs[0].Unbind(context, context.Handle->LpVtbl[67]); // V-Table Index 67 CSSetShaderResources.

            // CBV
            rangesCBVs[0].Unbind(context, context.Handle->LpVtbl[71]); // V-Table Index 71 CSSetConstantBuffers.

            // Sampler
            rangesSamplers[0].Unbind(context, context.Handle->LpVtbl[70]); // V-Table Index 70 CSSetSamplers.
        }

        protected override void DisposeCore()
        {
            D3D11GlobalResourceList.StateChanged -= GlobalStateChanged;

            pipeline.OnCompile -= OnPipelineCompile;
            pipeline.Dispose();

            Clear();
        }

        private static IEnumerable<BindingValuePair> EnumerateRanges(UnsafeList<D3D11DescriptorRange> ranges)
        {
            foreach (var range in ranges)
            {
                foreach (var pair in range)
                {
                    yield return pair;
                }
            }
        }
    }
}