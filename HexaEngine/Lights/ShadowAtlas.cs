using HexaEngine.Core;

namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;
    using System.Runtime.CompilerServices;

    public struct ShadowAtlasDescription
    {
        public Format Format;
        public int Size;
        public int Layers;

        public ShadowAtlasDescription(Format format, int size, int layers)
        {
            Format = format;
            Size = size;
            Layers = layers;
        }
    }

    public struct ShadowAtlasRangeHandle
    {
        private ShadowAtlas atlas;
        private readonly ShadowAtlasAllocation[] allocations;
        private bool valid;

        public ShadowAtlasRangeHandle(ShadowAtlas atlas, ShadowAtlasAllocation[] allocations)
        {
            this.atlas = atlas;
            this.allocations = allocations;
            valid = true;
        }

        public readonly ShadowAtlas Atlas => atlas;

        public readonly ShadowAtlasAllocation[] Allocations => allocations;

        public bool IsValid { readonly get => valid; internal set => valid = value; }

        public void Release()
        {
            if (valid)
                atlas.FreeRange(ref this);
            atlas = null;
        }
    }

    public struct ShadowAtlasHandle
    {
        private ShadowAtlas atlas;
        private ShadowAtlasAllocation allocation;
        private bool valid;

        public ShadowAtlasHandle(ShadowAtlas atlas, ShadowAtlasAllocation allocation)
        {
            this.atlas = atlas;
            this.allocation = allocation;
            valid = true;
        }

        public readonly ShadowAtlas Atlas => atlas;

        public readonly ShadowAtlasAllocation Allocation => allocation;

        public bool IsValid { readonly get => valid; internal set => valid = value; }

        public void Release()
        {
            if (valid)
                atlas.Free(ref this);
            atlas = null;
        }
    }

    public unsafe class ShadowAtlas : IDisposable
    {
        private readonly string dbgName;
        private readonly int size = unchecked(8192);
        private readonly int layerCount = 8;

        private readonly Mutex mutex = new();
        private readonly DepthStencil texture;
        private readonly Layer* layers;
        private readonly Dictionary<Pointer<Layer>, List<Pointer<MemoryBlock>>> layerFreeBlocks = new();
        private readonly Dictionary<Pointer<Layer>, List<Pointer<MemoryBlock>>> layerAllocatedBlocks = new();
        private readonly Dictionary<uint, Pointer<Layer>> sizeToLayer = new();
        private int freeBlockCounts = 0;
        private bool disposedValue;

        public ShadowAtlas(IGraphicsDevice device, ShadowAtlasDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"ShadowAtlas: {Path.GetFileName(filename)}, Line: {lineNumber}";
            size = description.Size;
            layerCount = description.Layers;
            texture = new(device, description.Size, description.Size, 1, description.Format, ResourceMiscFlag.None, BindFlags.ShaderResource | BindFlags.DepthStencil, filename, lineNumber);
            texture.DebugName = dbgName;

            layers = AllocT<Layer>(description.Layers);

            uint subSize = (uint)(description.Size / 2);

            for (int i = 0; i < description.Layers; i++)
            {
                uint subCount = (uint)(description.Size / subSize);
                Layer* layer = &layers[i];
                layer->Size = new(subSize);
                layer->Count = subCount * subCount;
                layer->RowWidth = subCount;
                layer->Blocks = AllocT<MemoryBlock>(layer->Count);

                layerFreeBlocks.Add(layer, new());
                layerAllocatedBlocks.Add(layer, new());
                sizeToLayer.Add(subSize, layer);

                for (uint j = 0; j < layer->Count; j++)
                {
                    uint x = j % subCount;
                    uint y = j / subCount;
                    MemoryBlock* block = &layer->Blocks[j];
                    block->Layer = &layers[i];
                    block->Offset = new(x, y);

                    layerFreeBlocks[layer].Add(block);
                    freeBlockCounts++;
                }

                subSize /= 2;
            }
        }

        public ShadowAtlas(IGraphicsDevice device, int size = 8192, int layerCount = 8, Format format = Format.D32Float, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"ShadowAtlas: {Path.GetFileName(filename)}, Line: {lineNumber}";
            this.size = size;
            this.layerCount = layerCount;

            texture = new(device, size, size, 1, format, ResourceMiscFlag.None, BindFlags.ShaderResource | BindFlags.DepthStencil, filename, lineNumber);
            texture.DebugName = "Shadow Atlas";

            layers = AllocT<Layer>(layerCount);

            uint subSize = (uint)(size / 2);

            for (int i = 0; i < layerCount; i++)
            {
                uint subCount = (uint)(size / subSize);
                Layer* layer = &layers[i];
                layer->Size = new(subSize);
                layer->Count = subCount * subCount;
                layer->RowWidth = subCount;
                layer->Blocks = AllocT<MemoryBlock>(layer->Count);

                layerFreeBlocks.Add(layer, new());
                layerAllocatedBlocks.Add(layer, new());
                sizeToLayer.Add(subSize, layer);

                for (uint j = 0; j < layer->Count; j++)
                {
                    uint x = j % subCount;
                    uint y = j / subCount;
                    MemoryBlock* block = &layer->Blocks[j];
                    block->Layer = &layers[i];
                    block->Offset = new(x, y);

                    layerFreeBlocks[layer].Add(block);
                    freeBlockCounts++;
                }

                subSize /= 2;
            }
        }

        public IDepthStencilView DSV => texture.DSV;

        public IShaderResourceView SRV => texture.SRV;

        public string DebugName => dbgName;

        public Viewport Viewport => texture.Viewport;

        public int LayerCount => layerCount;

        public float Size => size;

        public int SizeToIndex(int size)
        {
            for (int i = 0; i < layerCount; i++)
            {
                if (layers[i].Size.X == size)
                    return i;
            }
            return -1;
        }

        private struct Layer
        {
            public UPoint2 Size;
            public uint Count;
            public uint RowWidth;
            public MemoryBlock* Blocks;

            public readonly uint PointToIndex(UPoint2 point)
            {
                return point.X + point.Y * RowWidth;
            }
        }

        private struct MemoryBlock
        {
            public Layer* Layer;
            public UPoint2 Offset;
        }

        private void AllocAboveAndBelow(MemoryBlock* block, int layerIndex)
        {
            if (layerIndex == 0)
                return;

            Layer* layer = block->Layer;

            Layer* currentLayer = layer;
            MemoryBlock* currentBlock = block;

            for (int i = layerIndex - 1; i >= 0; i--)
            {
                Layer* above = &layers[i];
                var freeAboveBlocks = layerFreeBlocks[above];
                var allocatedAboveBlocks = layerAllocatedBlocks[above];

                var relOffset = currentBlock->Offset / above->RowWidth;
                uint index = above->PointToIndex(relOffset);

                MemoryBlock* aboveBlock = &above->Blocks[index];
                if (!allocatedAboveBlocks.Contains(aboveBlock))
                {
                    freeAboveBlocks.Remove(aboveBlock);
                    allocatedAboveBlocks.Add(aboveBlock);
                    freeBlockCounts--;

                    currentLayer = above;
                    currentBlock = aboveBlock;
                }
                else
                {
                    break;
                }
            }

            for (int i = layerIndex + 1; i < layerCount; i++)
            {
                Layer* below = &layers[i];
                var freeBelowBlocks = layerFreeBlocks[below];
                var allocatedBelowBlocks = layerAllocatedBlocks[below];

                var relSize = layer->Size / below->Size;
                var relOffset = block->Offset * relSize;

                for (uint x = 0; x < relSize.X; x++)
                {
                    for (uint y = 0; y < relSize.Y; y++)
                    {
                        var pos = relOffset + new UPoint2(x, y);
                        uint index = below->PointToIndex(pos);

                        MemoryBlock* belowBlock = &below->Blocks[index];
                        freeBelowBlocks.Remove(belowBlock);
                        allocatedBelowBlocks.Add(belowBlock);
                        freeBlockCounts--;
                    }
                }
            }
        }

        private void FreeAboveAndBelow(MemoryBlock* block, int layerIndex)
        {
            if (layerIndex == 0)
                return;

            Layer* layer = block->Layer;

            for (int i = layerIndex + 1; i < layerCount; i++)
            {
                Layer* below = &layers[i];
                var freeBelowBlocks = layerFreeBlocks[below];
                var allocatedBelowBlocks = layerAllocatedBlocks[below];

                var relSize = layer->Size / below->Size;
                var relOffset = block->Offset * relSize;

                for (uint x = 0; x < relSize.X; x++)
                {
                    for (uint y = 0; y < relSize.Y; y++)
                    {
                        var pos = relOffset + new UPoint2(x, y);
                        uint index = below->PointToIndex(pos);

                        MemoryBlock* belowBlock = &below->Blocks[index];
                        freeBelowBlocks.Add(belowBlock);
                        allocatedBelowBlocks.Remove(belowBlock);
                        freeBlockCounts++;
                    }
                }
            }

            Layer* currentLayer = layer;
            MemoryBlock* currentBlock = block;

            for (int i = layerIndex - 1; i >= 0; i--)
            {
                var freeCurrentBlocks = layerFreeBlocks[currentLayer];
                var allocatedCurrentBlocks = layerAllocatedBlocks[currentLayer];
                Layer* above = &layers[i];

                var relSize = currentLayer->RowWidth / above->RowWidth;
                var relOffset = currentBlock->Offset / above->RowWidth * currentLayer->RowWidth;

                bool allFree = true;
                for (uint x = 0; x < relSize; x++)
                {
                    for (uint y = 0; y < relSize; y++)
                    {
                        var pos = relOffset + new UPoint2(x, y);
                        uint index = currentLayer->PointToIndex(pos);
                        MemoryBlock* subBlock = &currentLayer->Blocks[index];
                        if (allocatedCurrentBlocks.Contains(subBlock))
                        {
                            allFree = false;
                        }
                    }
                }

                if (allFree)
                {
                    var freeAboveBlocks = layerFreeBlocks[above];
                    var allocatedAboveBlocks = layerAllocatedBlocks[above];

                    uint index = above->PointToIndex(relOffset);

                    MemoryBlock* aboveBlock = &above->Blocks[index];
                    freeAboveBlocks.Add(aboveBlock);
                    allocatedAboveBlocks.Remove(aboveBlock);
                    freeBlockCounts++;

                    currentLayer = above;
                    currentBlock = aboveBlock;
                }
                else
                {
                    break;
                }
            }
        }

        private MemoryBlock* AllocateFreeBlock(uint size, out Layer* layer, out int layerIndex)
        {
            var maxLayer = (Layer*)sizeToLayer[size];

            layerIndex = ArrayUtils.IndexOf(layers, maxLayer, layerCount);
            while (true)
            {
                layer = &layers[layerIndex];
                var freeBlocks = layerFreeBlocks[layer];

                if (freeBlocks.Count > 0)
                {
                    var freeBlock = (MemoryBlock*)freeBlocks[0];
                    freeBlocks.RemoveAt(0);
                    layerAllocatedBlocks[layer].Add(freeBlock);
                    freeBlockCounts--;
                    AllocAboveAndBelow(freeBlock, layerIndex);
                    return freeBlock;
                }

                layerIndex++;
                size /= 2;

                if (layerIndex == layerCount)
                    throw new OutOfMemoryException();
            }
        }

        public bool IsAvailable(uint size)
        {
            var maxLayer = (Layer*)sizeToLayer[size];

            var layerIndex = ArrayUtils.IndexOf(layers, maxLayer, layerCount);
            while (true)
            {
                var layer = &layers[layerIndex];
                var freeBlocks = layerFreeBlocks[layer];

                if (freeBlocks.Count > 0)
                {
                    return true;
                }

                layerIndex++;
                size /= 2;

                if (layerIndex == layerCount)
                    return false;
            }
        }

        public int BlocksAvailable()
        {
            return freeBlockCounts;
        }

        public void Clear()
        {
            mutex.WaitOne();
            for (int i = 0; i < layerCount; i++)
            {
                Layer* layer = &layers[i];

                var freeBlocks = layerFreeBlocks[layer];
                var allocatedBlocks = layerAllocatedBlocks[layer];
                allocatedBlocks.Clear();
                freeBlocks.Clear();
                for (uint j = 0; j < layer->Count; j++)
                {
                    MemoryBlock* block = &layer->Blocks[j];
                    freeBlocks.Add(block);
                    freeBlockCounts++;
                }
            }
            mutex.ReleaseMutex();
        }

        public ShadowAtlasHandle Alloc(uint desiredSize)
        {
            mutex.WaitOne();
            MemoryBlock* block = AllocateFreeBlock(desiredSize, out var layer, out var layerIndex);
            mutex.ReleaseMutex();
            ShadowAtlasAllocation allocation = new();
            allocation.Offset = block->Offset;
            allocation.Size = layer->Size;
            allocation.BlockHandle = (nint)block;
            allocation.LayerHandle = (nint)layer;
            allocation.LayerIndex = layerIndex;
            return new(this, allocation);
        }

        public ShadowAtlasRangeHandle AllocRange(uint desiredSize, int count)
        {
            ShadowAtlasAllocation[] allocations = new ShadowAtlasAllocation[count];
            for (uint i = 0; i < count; i++)
            {
                allocations[i] = Alloc(desiredSize).Allocation;
            }
            return new(this, allocations);
        }

        private void Free(ref ShadowAtlasAllocation allocation)
        {
            Layer* layer = (Layer*)allocation.LayerHandle;
            MemoryBlock* block = (MemoryBlock*)allocation.BlockHandle;
            mutex.WaitOne();
            layerFreeBlocks[layer].Add(block);
            layerAllocatedBlocks[layer].Remove(block);
            FreeAboveAndBelow(block, allocation.LayerIndex);
            allocation = default;
            mutex.ReleaseMutex();
        }

        public void Free(ref ShadowAtlasHandle handle)
        {
            var allocation = handle.Allocation;
            Free(ref allocation);
            handle.IsValid = false;
        }

        public void FreeRange(ref ShadowAtlasRangeHandle handle)
        {
            var allocations = handle.Allocations;
            for (uint i = 0; i < allocations.Length; i++)
            {
                Free(ref allocations[i]);
            }
            handle.IsValid = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                texture.Dispose();
                disposedValue = true;
            }
        }

        ~ShadowAtlas()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}