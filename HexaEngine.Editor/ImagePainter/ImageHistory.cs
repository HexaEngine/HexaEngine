namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;

    /// <summary>
    /// TODO: FIX MEMORY LEAKS
    /// </summary>
    public unsafe class ImageHistory : IDisposable
    {
        private readonly ImageHistoryEntry[] undoHistory;
        private readonly ImageHistoryEntry[] redoHistory;
        private readonly ImageSource source;
        private readonly ITexture2D[] stagingTextures;
        private readonly int maxCount;
        private int undoHistoryCount;
        private int redoHistoryCount;
        private bool disposedValue;

        public ImageHistory(IGraphicsDevice device, ImageSource source, int maxCount)
        {
            this.source = source;
            this.maxCount = maxCount;

            stagingTextures = new ITexture2D[source.ImageCount];

            for (int i = 0; i < stagingTextures.Length; i++)
            {
                var desc = source.Textures[i].Description;
                desc.Usage = Usage.Staging;
                desc.BindFlags = BindFlags.None;
                desc.CPUAccessFlags = CpuAccessFlags.RW;
                stagingTextures[i] = device.CreateTexture2D(desc);
            }

            undoHistory = new ImageHistoryEntry[maxCount];
            redoHistory = new ImageHistoryEntry[maxCount];
        }

        public int UndoCount => undoHistoryCount;

        public int RedoCount => redoHistoryCount;

        public void Clear()
        {
            for (int i = 0; i < undoHistory.Length; i++)
            {
                undoHistory[i].Release();
            }
            undoHistoryCount = 0;
            for (int i = 0; i < redoHistory.Length; i++)
            {
                redoHistory[i].Release();
            }
            redoHistoryCount = 0;
        }

        public void UndoPush(IGraphicsContext context)
        {
            UndoPushInternal(context);
            for (int i = 0; i < redoHistoryCount; i++)
            {
                var index = maxCount - redoHistoryCount;
                Free(redoHistory[index].Data);
                redoHistory[index].Data = null;
            }
            redoHistoryCount = 0;
        }

        private void UndoPushInternal(IGraphicsContext context)
        {
            if (source == null || stagingTextures == null)
            {
                return;
            }

            var last = undoHistory[^1];
            for (int i = undoHistory.Length - 1; i != 0; i--)
            {
                undoHistory[i] = undoHistory[i - 1];
            }

            var index = source.Index;
            var staging = stagingTextures[index];

            context.CopyResource(staging, source.Texture);
            var mapped = context.Map(staging, 0, MapMode.Read, MapFlags.None);

            nint size = (nint)(mapped.RowPitch * staging.Description.Height);
            last.Index = index;
            if (last.Data != null)
            {
                Free(last.Data);
                last.Data = null;
            }
            last.Data = Alloc(size);
            last.Size = size;

            Memcpy(mapped.PData, last.Data, size);

            context.Unmap(staging, 0);

            undoHistory[0] = last;
            undoHistoryCount++;
            if (undoHistoryCount > maxCount)
            {
                undoHistoryCount = maxCount;
            }
        }

        private void RedoPushInternal(ImageHistoryEntry entry)
        {
            if (source == null || stagingTextures == null)
            {
                return;
            }

            var last = redoHistory[^1];
            for (int i = redoHistory.Length - 1; i != 0; i--)
            {
                redoHistory[i] = redoHistory[i - 1];
            }

            if (last.Data != null)
            {
                Free(last.Data);
                last.Data = null;
            }

            last.Index = entry.Index;
            last.Data = Alloc(entry.Size);
            last.Size = entry.Size;

            Memcpy(entry.Data, last.Data, last.Size);

            redoHistory[0] = last;
            redoHistoryCount++;
            if (redoHistoryCount > maxCount)
            {
                redoHistoryCount = maxCount;
            }
        }

        public void Undo(IGraphicsContext context)
        {
            if (undoHistoryCount == 0)
            {
                return;
            }

            if (source == null || stagingTextures == null)
            {
                return;
            }

            var first = undoHistory[0];
            for (int i = 0; i < undoHistory.Length - 1; i++)
            {
                undoHistory[i] = undoHistory[i + 1];
            }

            RedoPushInternal(first);

            var index = first.Index;
            var texture = source.Textures[index];
            var staging = stagingTextures[index];

            var mapped = context.Map(staging, index, MapMode.Write, MapFlags.None);

            var size = mapped.RowPitch * staging.Description.Height;

            Memcpy(first.Data, mapped.PData, size);

            context.Unmap(staging, index);
            context.CopyResource(texture, staging);

            if (first.Data != null)
            {
                Free(first.Data);
                first.Data = null;
            }

            undoHistory[^1] = first;
            undoHistoryCount--;
        }

        public void Redo(IGraphicsContext context)
        {
            if (redoHistoryCount == 0)
            {
                return;
            }

            if (source == null || stagingTextures == null)
            {
                return;
            }

            var first = redoHistory[0];
            for (int i = 0; i < redoHistory.Length - 1; i++)
            {
                redoHistory[i] = redoHistory[i + 1];
            }

            UndoPushInternal(context);

            var index = first.Index;
            var texture = source.Textures[index];
            var staging = stagingTextures[index];

            var mapped = context.Map(staging, index, MapMode.Write, MapFlags.None);

            var size = mapped.RowPitch * staging.Description.Height;

            Memcpy(first.Data, mapped.PData, size);

            context.Unmap(staging, index);
            context.CopyResource(texture, staging);
            redoHistory[^1] = first;
            redoHistoryCount--;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < undoHistory.Length; i++)
                {
                    undoHistory[i].Release();
                }
                undoHistoryCount = 0;
                for (int i = 0; i < redoHistory.Length; i++)
                {
                    redoHistory[i].Release();
                }
                redoHistoryCount = 0;

                for (int i = 0; i < stagingTextures.Length; i++)
                {
                    stagingTextures[i].Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}