using HexaEngine.Core;

namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System;

    public unsafe class ShadowAtlas : IDisposable
    {
        private readonly Mutex mutex = new();
        private readonly DepthStencil texture;
        private readonly Layer* layers;
        private readonly Dictionary<Pointer<Layer>, List<Pointer<MemoryBlock>>> layerFreeBlocks = new();
        private readonly Dictionary<Pointer<Layer>, List<Pointer<MemoryBlock>>> layerAllocatedBlocks = new();
        private readonly Dictionary<uint, Pointer<Layer>> sizeToLayer = new();
        private int freeBlockCounts = 0;
        private bool disposedValue;

        private const int size = unchecked(8192);
        private const int maxLayers = 8;

        public ShadowAtlas(IGraphicsDevice device)
        {
            texture = new(device, size, size, 1, Format.D32Float);
            texture.DebugName = "Shadow Atlas";

            layers = Alloc<Layer>(maxLayers);

            uint subSize = size / 2;

            for (int i = 0; i < maxLayers; i++)
            {
                uint subCount = size / subSize;
                Layer* layer = &layers[i];
                layer->Size = new(subSize);
                layer->Count = subCount * subCount;
                layer->RowWidth = subCount;
                layer->Blocks = Alloc<MemoryBlock>(layer->Count);

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

            for (int i = layerIndex + 1; i < maxLayers; i++)
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

            for (int i = layerIndex + 1; i < maxLayers; i++)
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

            layerIndex = ArrayUtils.IndexOf(layers, maxLayer, maxLayers);
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

                if (layerIndex == maxLayers)
                    throw new OutOfMemoryException();
            }
        }

        public bool IsAvailable(uint size)
        {
            var maxLayer = (Layer*)sizeToLayer[size];

            var layerIndex = ArrayUtils.IndexOf(layers, maxLayer, maxLayers);
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

                if (layerIndex == maxLayers)
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
            for (int i = 0; i < maxLayers; i++)
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

        public ShadowAtlasAllocation Alloc(uint desiredSize)
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
            return allocation;
        }

        public void AllocRange(uint desiredSize, ShadowAtlasAllocation[] allocations)
        {
            for (uint i = 0; i < allocations.Length; i++)
            {
                allocations[i] = Alloc(desiredSize);
            }
        }

        public void Free(ref ShadowAtlasAllocation allocation)
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

        public void FreeRange(ShadowAtlasAllocation[] allocations)
        {
            for (uint i = 0; i < allocations.Length; i++)
            {
                Free(ref allocations[i]);
            }
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