namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public unsafe struct ShaderParameter
    {
        public char* Name;
        public uint Hash;
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

    public unsafe struct D3D11DescriptorRange
    {
        public D3D11DescriptorRange(ShaderStage stage, ShaderParameterType type, List<ShaderParameter> parameters)
        {
            Stage = stage;
            Type = type;

            count = parameters.Count;
            if (count > 0) // just allocate if we really need it.
            {
                buckets = AllocT<ShaderParameter>(count);
                ZeroMemoryT(buckets, count);
            }

            uint startSlot = uint.MaxValue;
            uint maxSlot = 0;
            foreach (ShaderParameter parameter in parameters)
            {
                startSlot = Math.Min(startSlot, parameter.Index);
                maxSlot = Math.Max(maxSlot, parameter.Index);
                var param = Find(buckets, parameters.Count, parameter.Hash, parameter.Name);
                *param = parameter;
            }

            uint rangeWidth = parameters.Count == 0 ? 0 : (maxSlot + 1) - startSlot;

            StartSlot = startSlot;
            Count = rangeWidth;

            if (count == 0)
            {
                return;
            }
            Resources = (void**)AllocT<nint>(rangeWidth);
            ZeroMemory(Resources, rangeWidth * sizeof(nint));

            if (type == ShaderParameterType.UAV)
            {
                InitialCounts = (uint*)Alloc(rangeWidth * sizeof(uint));
                for (uint i = 0; i < rangeWidth; i++)
                {
                    InitialCounts[i] = uint.MaxValue;
                }
            }
        }

        private readonly ShaderParameter* Find(ShaderParameter* buckets, int capacity, uint hash, char* key)
        {
            uint index = (uint)(hash % capacity);
            bool exit = false;
            while (true)
            {
                var entry = &buckets[index];
                if (entry->Hash == 0)
                {
                    return entry;
                }
                else if (entry->Hash == hash && StrCmp(key, entry->Name) == 0)
                {
                    return entry;
                }

                index++;
                if (index == capacity)
                {
                    if (exit)
                    {
                        break;
                    }

                    index = 0;
                    exit = true;
                }
            }

            return null; // return null means not found and full.
        }

        private readonly ShaderStage Stage;
        private readonly ShaderParameterType Type;
        private readonly uint StartSlot;
        private readonly uint Count;
        private void** Resources;
        private uint* InitialCounts;

        private UnsafeList<ResourceRange> Ranges;
        private ShaderParameter* buckets;
        private int count;

        public struct ResourceRange
        {
            public void** Start;
            public int Length;
        }

        private readonly ShaderParameter GetByName(string name)
        {
            if (count == 0)
            {
                // special case, check ahead to avoid unnecessary steps.
                throw new KeyNotFoundException();
            }

            uint hash = (uint)name.GetHashCode();
            fixed (char* pName = name)
            {
                var pEntry = Find(buckets, count, hash, pName);
                if (pEntry != null && pEntry->Hash == hash)
                {
                    return *pEntry;
                }
                throw new KeyNotFoundException();
            }
        }

        private readonly bool TryGetByName(string name, out ShaderParameter parameter)
        {
            if (count == 0)
            {
                // special case, check ahead to avoid unnecessary steps.
                parameter = default;
                return false;
            }

            uint hash = (uint)name.GetHashCode();
            fixed (char* pName = name)
            {
                var pEntry = Find(buckets, count, hash, pName);
                if (pEntry != null && pEntry->Hash == hash)
                {
                    parameter = *pEntry;
                    return true;
                }
                parameter = default;
                return false;
            }
        }

        public void SetByName(string name, void* resource)
        {
            var parameter = GetByName(name);
            var old = Resources[parameter.Index - StartSlot];
            Resources[parameter.Index - StartSlot] = resource;
            if (old != null ^ resource != null)
            {
                UpdateRanges(parameter.Index - StartSlot, resource == null);
            }
        }

        public bool TrySetByName(string name, void* resource, uint initialValue = unchecked((uint)-1))
        {
            if (TryGetByName(name, out var parameter))
            {
                var index = parameter.Index - StartSlot;
                var old = Resources[index];
                Resources[index] = resource;
                if (InitialCounts != null)
                {
                    InitialCounts[index] = initialValue;
                }
                if (old != null ^ resource != null)
                {
                    UpdateRanges(index, resource == null);
                }

                return true;
            }
            return false;
        }

        private void UpdateRanges(uint idx, bool clear)
        {
            if (clear)
            {
                // Resource is set to null; remove or adjust the range
                for (int i = 0; i < Ranges.Count; i++)
                {
                    ResourceRange* range = Ranges.GetPointer(i);
                    int rangeStart = (int)(range->Start - Resources);
                    int rangeEnd = rangeStart + range->Length - 1;

                    if (idx >= rangeStart && idx <= rangeEnd)
                    {
                        // The null resource falls within this range
                        if (range->Length == 1)
                        {
                            // The range has only this single entry; remove the range
                            Ranges.RemoveAt(i);
                        }
                        else if (idx == rangeStart)
                        {
                            // Adjust the range to start after the null entry
                            range->Start++;
                            range->Length--;
                        }
                        else if (idx == rangeEnd)
                        {
                            // Adjust the range to end before the null entry
                            range->Length--;
                        }
                        else
                        {
                            // Split the range into two separate ranges
                            var newRange = new ResourceRange
                            {
                                Start = range->Start + (idx - rangeStart + 1),
                                Length = rangeEnd - (int)idx
                            };

                            range->Length = (int)(idx - rangeStart);
                            Ranges.Insert(i + 1, newRange);
                        }
                        return;
                    }
                }
            }
            else
            {
                // Handle setting a non-null resource
                for (int i = 0; i < Ranges.Count; i++)
                {
                    ResourceRange* range = Ranges.GetPointer(i);
                    int rangeStart = (int)(range->Start - Resources);
                    int rangeEnd = rangeStart + range->Length - 1;

                    if (idx == rangeStart - 1)
                    {
                        // Extend the range at the beginning
                        range->Start--;
                        range->Length++;

                        // Check if we need to merge with the previous range
                        if (i > 0)
                        {
                            ResourceRange* previousRange = Ranges.GetPointer(i - 1);
                            if (previousRange->Start + previousRange->Length == range->Start)
                            {
                                previousRange->Length += range->Length;
                                Ranges.RemoveAt(i);
                            }
                        }
                        return;
                    }
                    else if (idx == rangeEnd + 1)
                    {
                        // Extend the range at the end
                        range->Length++;

                        // Check if we need to merge with the next range
                        if (i < Ranges.Count - 1)
                        {
                            ResourceRange* nextRange = Ranges.GetPointer(i + 1);
                            if (range->Start + range->Length == nextRange->Start)
                            {
                                range->Length += nextRange->Length;
                                Ranges.RemoveAt(i + 1);
                            }
                        }
                        return;
                    }
                    else if (idx < rangeStart)
                    {
                        // Insert a new range before this one
                        var newRange = new ResourceRange { Start = Resources + idx, Length = 1 };
                        Ranges.Insert(i, newRange);

                        // Check if we need to merge with the next range
                        if (i < Ranges.Count - 1)
                        {
                            ResourceRange* nextRange = Ranges.GetPointer(i + 1);
                            if (newRange.Start + newRange.Length == nextRange->Start)
                            {
                                newRange.Length += nextRange->Length;
                                Ranges.RemoveAt(i + 1);
                            }
                        }
                        return;
                    }
                }

                // If no existing range is found, add a new one at the end
                var endRange = new ResourceRange { Start = Resources + idx, Length = 1 };
                Ranges.Add(endRange);

                // Check if we need to merge with the previous range
                if (Ranges.Count > 1)
                {
                    ResourceRange* previousRange = Ranges.GetPointer(Ranges.Count - 2);
                    if (previousRange->Start + previousRange->Length == endRange.Start)
                    {
                        previousRange->Length += endRange.Length;
                        Ranges.RemoveAt(Ranges.Count - 1);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(ComPtr<ID3D11DeviceContext3> context, void* func)
        {
            var function = (delegate*<ID3D11DeviceContext3*, uint, uint, void**, void>)func;
            for (int i = 0; i < Ranges.Count; i++)
            {
                ResourceRange range = Ranges[i];

                if (range.Length > 0)
                {
                    void** resources = range.Start;
                    var start = (uint)(range.Start - Resources);
                    function(context, StartSlot + start, (uint)range.Length, resources);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindUAV(ComPtr<ID3D11DeviceContext3> context)
        {
            for (int i = 0; i < Ranges.Count; i++)
            {
                ResourceRange range = Ranges[i];

                if (range.Length > 0)
                {
                    void** resources = range.Start;
                    var start = (uint)(range.Start - Resources);
                    context.CSSetUnorderedAccessViews(StartSlot + start, (uint)range.Length, (ID3D11UnorderedAccessView**)resources, (uint*)null);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unbind(ComPtr<ID3D11DeviceContext3> context, void* func)
        {
            nint* resources = stackalloc nint[256];
            var function = (delegate*<ID3D11DeviceContext3*, uint, uint, void**, void>)func;
            for (int i = 0; i < Ranges.Count; i++)
            {
                ResourceRange range = Ranges[i];

                if (range.Length > 0)
                {
                    var start = (uint)(range.Start - Resources);
                    function(context, StartSlot + start, (uint)range.Length, (void**)resources);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnbindUAV(ComPtr<ID3D11DeviceContext3> context)
        {
            nint* resources = stackalloc nint[256];
            for (int i = 0; i < Ranges.Count; i++)
            {
                ResourceRange range = Ranges[i];

                if (range.Length > 0)
                {
                    var start = (uint)(range.Start - Resources);
                    context.CSSetUnorderedAccessViews(StartSlot + start, (uint)range.Length, (ID3D11UnorderedAccessView**)resources, InitialCounts + start);
                }
            }
        }

        public void Release()
        {
            if (Resources != null)
            {
                Free(Resources);
                Resources = null;
            }
            if (InitialCounts != null)
            {
                Free(InitialCounts);
                InitialCounts = null;
            }
            if (buckets != null)
            {
                for (int i = 0; i < count; i++)
                {
                    Free(buckets[i].Name);
                }
                Free(buckets);
                buckets = null;
                count = 0;
            }
            Ranges.Release();
        }
    }

    public unsafe class D3D11ResourceBindingList : DisposableBase, IResourceBindingList
    {
        private readonly IPipeline pipeline;
        private UnsafeList<D3D11DescriptorRange> rangesSRVs;
        private UnsafeList<D3D11DescriptorRange> rangesUAVs;
        private UnsafeList<D3D11DescriptorRange> rangesCBVs;
        private UnsafeList<D3D11DescriptorRange> rangesSamplers;

        public D3D11ResourceBindingList(D3D11GraphicsPipeline pipeline)
        {
            pipeline.AddRef();
            pipeline.OnCompile += OnPipelineCompile;
            this.pipeline = pipeline;
            OnPipelineCompile(pipeline);
        }

        public D3D11ResourceBindingList(D3D11ComputePipeline pipeline)
        {
            pipeline.AddRef();
            pipeline.OnCompile += OnPipelineCompile;
            this.pipeline = pipeline;
            OnPipelineCompile(pipeline);
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
        }

        public void SetSRV(string name, IShaderResourceView? srv)
        {
            var p = srv?.NativePointer ?? 0;
            for (int i = 0; i < rangesSRVs.Count; i++)
            {
                rangesSRVs.GetPointer(i)->TrySetByName(name, (void*)p);
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

        public void SetCBV(string name, IBuffer? cbv)
        {
            var p = cbv?.NativePointer ?? 0;
            for (int i = 0; i < rangesCBVs.Count; i++)
            {
                rangesCBVs.GetPointer(i)->TrySetByName(name, (void*)p);
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

        private void Reflect(Shader* shader, ShaderStage stage)
        {
            if (shader == null)
            {
                rangesSRVs.Add(new(stage, ShaderParameterType.SRV, new()));
                rangesUAVs.Add(new(stage, ShaderParameterType.UAV, new()));
                rangesCBVs.Add(new(stage, ShaderParameterType.CBV, new()));
                rangesSamplers.Add(new(stage, ShaderParameterType.Sampler, new()));
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

                string name = ToStringFromUTF8(shaderInputBindDesc.Name) ?? throw new Exception("Name cannot be null, check your shader code ensure all resources are named!");

                parameter.Hash = (uint)name.GetHashCode();
                parameter.Name = name.ToUTF16Ptr();

                shaderParametersInStage.Add(parameter);
                shaderParametersByNameInStage.Add(name, parameter);
            }

            rangesSRVs.PushBack(new(stage, ShaderParameterType.SRV, shaderParametersInStage.Where(x => x.Type == ShaderParameterType.SRV).ToList()));
            rangesUAVs.PushBack(new(stage, ShaderParameterType.UAV, shaderParametersInStage.Where(x => x.Type == ShaderParameterType.UAV).ToList()));
            rangesCBVs.PushBack(new(stage, ShaderParameterType.CBV, shaderParametersInStage.Where(x => x.Type == ShaderParameterType.CBV).ToList()));
            rangesSamplers.PushBack(new(stage, ShaderParameterType.Sampler, shaderParametersInStage.Where(x => x.Type == ShaderParameterType.Sampler).ToList()));

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
            pipeline.OnCompile -= OnPipelineCompile;
            pipeline.Dispose();

            Clear();
        }
    }
}