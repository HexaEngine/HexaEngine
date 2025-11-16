namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public unsafe struct D3D11DescriptorRange : IEnumerable<BindingValuePair>
    {
        public D3D11DescriptorRange(ShaderStage stage, ShaderParameterType type, D3D11ShaderParameter[] parameters)
        {
            Stage = stage;
            Type = type;

            bucketCount = parameters.Length;
            if (bucketCount > 0) // just allocate if we really need it.
            {
                buckets = AllocT<D3D11ShaderParameter>(bucketCount);
                ZeroMemoryT(buckets, bucketCount);
            }

            uint startSlot = uint.MaxValue;
            uint maxSlot = 0;
            foreach (D3D11ShaderParameter parameter in parameters)
            {
                startSlot = Math.Min(startSlot, parameter.Index);
                maxSlot = Math.Max(maxSlot, parameter.Index);
                var param = Find(buckets, parameters.Length, parameter.Hash, parameter.Name);
                *param = parameter;
            }

            uint rangeWidth = parameters.Length == 0 ? 0 : (maxSlot + 1) - startSlot;

            StartSlot = startSlot;
            Count = rangeWidth;

            if (bucketCount == 0)
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

        private readonly D3D11ShaderParameter* Find(D3D11ShaderParameter* buckets, int capacity, uint hash, char* key)
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
        private D3D11ShaderParameter* buckets;
        private int bucketCount;

        public struct ResourceRange
        {
            public void** Start;
            public int Length;
        }

        private readonly D3D11ShaderParameter GetByName(string name)
        {
            if (bucketCount == 0)
            {
                // special case, check ahead to avoid unnecessary steps.
                throw new KeyNotFoundException();
            }

            uint hash = (uint)name.GetHashCode();
            fixed (char* pName = name)
            {
                var pEntry = Find(buckets, bucketCount, hash, pName);
                if (pEntry != null && pEntry->Hash == hash)
                {
                    return *pEntry;
                }
                throw new KeyNotFoundException();
            }
        }

        private readonly bool TryGetByName(string name, out D3D11ShaderParameter parameter)
        {
            if (bucketCount == 0)
            {
                // special case, check ahead to avoid unnecessary steps.
                parameter = default;
                return false;
            }

            uint hash = (uint)name.GetHashCode();
            fixed (char* pName = name)
            {
                var pEntry = Find(buckets, bucketCount, hash, pName);
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

        public void UpdateByName(string name, void* oldState, void* state, uint initialValue = unchecked((uint)-1))
        {
            if (TryGetByName(name, out var parameter))
            {
                var index = parameter.Index - StartSlot;
                var old = Resources[index];

                if (old != oldState) // indicates that the state was overwritten locally so don't update.
                {
                    return;
                }

                Resources[index] = state;
                if (InitialCounts != null)
                {
                    InitialCounts[index] = initialValue;
                }
                if (old != null ^ state != null)
                {
                    UpdateRanges(index, state == null);
                }
            }
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
                    context.CSSetUnorderedAccessViews(StartSlot + start, (uint)range.Length, (ID3D11UnorderedAccessView**)resources, InitialCounts);
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
                for (int i = 0; i < bucketCount; i++)
                {
                    Free(buckets[i].Name);
                }
                Free(buckets);
                buckets = null;
                bucketCount = 0;
            }
            Ranges.Release();
        }

        public readonly IEnumerator<BindingValuePair> GetEnumerator()
        {
            return new Enumerator(this);
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<BindingValuePair>
        {
            private readonly D3D11DescriptorRange descriptorRange;
            private BindingValuePair current;
            private D3D11ShaderParameter* currentBucket;

            public Enumerator(D3D11DescriptorRange descriptorRange)
            {
                this.descriptorRange = descriptorRange;
            }

            public readonly BindingValuePair Current => current;

            readonly object IEnumerator.Current => Current;

            public void Dispose()
            {
                // Nothing to do here.
            }

            public bool MoveNext()
            {
                if (currentBucket == null)
                {
                    currentBucket = descriptorRange.buckets;
                    if (currentBucket == null) return false;
                    ReadFromBucket();
                    return true;
                }

                var index = currentBucket - descriptorRange.buckets;
                if (index == descriptorRange.bucketCount - 1)
                {
                    return false;
                }
                currentBucket++;
                ReadFromBucket();
                return true;
            }

            private void ReadFromBucket()
            {
                current.Name = new(currentBucket->Name);
                current.Stage = descriptorRange.Stage;
                current.Type = currentBucket->Type;
                current.Value = descriptorRange.Resources[currentBucket->Index];
            }

            public void Reset()
            {
                currentBucket = null;
            }
        }
    }
}