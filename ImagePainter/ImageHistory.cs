namespace ImagePainter
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using System.Diagnostics;

    public unsafe class ImageHistory : IDisposable
    {
        private readonly byte[][] undoHistory;
        private readonly byte[][] redoHistory;
        private readonly IGraphicsDevice device;
        private readonly ITexture2D source;
        private readonly ITexture2D staging;
        private int maxCount;
        private int undoHistoryCount;
        private int redoHistoryCount;
        private bool disposedValue;

        public ImageHistory(IGraphicsDevice device, ITexture2D source, int maxCount)
        {
            this.device = device;
            this.source = source;
            this.maxCount = maxCount;
            var desc = source.Description;
            desc.CPUAccessFlags = CpuAccessFlags.RW;
            desc.Usage = Usage.Staging;
            desc.BindFlags = BindFlags.None;
            staging = device.CreateTexture2D(desc);

            var mapped = device.Context.Map(staging, 0, MapMode.Read, MapFlags.None);
            var size = mapped.RowPitch * staging.Description.Height;
            device.Context.Unmap(staging, 0);

            undoHistory = new byte[maxCount][];
            for (int i = 0; i < maxCount; i++)
            {
                undoHistory[i] = new byte[size];
            }

            redoHistory = new byte[maxCount][];
            for (int i = 0; i < maxCount; i++)
            {
                redoHistory[i] = new byte[size];
            }
        }

        public int UndoCount => undoHistoryCount;

        public int RedoCount => redoHistoryCount;

        public void UndoPush(IGraphicsContext context)
        {
            UndoPushInternal(context);
            redoHistoryCount = 0;
        }

        private void UndoPushInternal(IGraphicsContext context)
        {
            if (source == null || staging == null) return;

            var last = undoHistory[^1];
            for (int i = undoHistory.Length - 1; i != 0; i--)
            {
                undoHistory[i] = undoHistory[i - 1];
            }
            undoHistory[0] = last;

            context.CopyResource(staging, source);
            var mapped = context.Map(staging, 0, MapMode.Read, MapFlags.None);

            var size = mapped.RowPitch * staging.Description.Height;

            fixed (byte* bp = last)
                MemoryCopy(mapped.PData, bp, size);

            context.Unmap(staging, 0);
            undoHistoryCount++;
            if (undoHistoryCount > maxCount)
            {
                undoHistoryCount = maxCount;
            }
        }

        private void RedoPushInternal(IGraphicsContext context)
        {
            if (source == null || staging == null) return;

            var last = redoHistory[^1];
            for (int i = redoHistory.Length - 1; i != 0; i--)
            {
                redoHistory[i] = redoHistory[i - 1];
            }
            redoHistory[0] = last;

            context.CopyResource(staging, source);
            var mapped = context.Map(staging, 0, MapMode.Read, MapFlags.None);

            var size = mapped.RowPitch * staging.Description.Height;

            fixed (byte* bp = last)
                MemoryCopy(mapped.PData, bp, size);

            context.Unmap(staging, 0);
            redoHistoryCount++;
            if (redoHistoryCount > maxCount)
            {
                redoHistoryCount = maxCount;
            }
        }

        public void Undo(IGraphicsContext context)
        {
            if (undoHistoryCount == 0) return;
            if (source == null || staging == null) return;

            var first = undoHistory[0];
            for (int i = 0; i < undoHistory.Length - 1; i++)
            {
                undoHistory[i] = undoHistory[i + 1];
            }
            undoHistory[^1] = first;

            RedoPushInternal(context);

            var index = IResource.CalculateSubresourceIndex(0, 0, staging.Description.MipLevels);
            var mapped = context.Map(staging, index, MapMode.Write, MapFlags.None);

            var size = mapped.RowPitch * staging.Description.Height;

            fixed (byte* bp = first)
                MemoryCopy(bp, mapped.PData, size);

            context.Unmap(staging, index);
            context.CopyResource(source, staging);
            undoHistoryCount--;
        }

        public void Redo(IGraphicsContext context)
        {
            if (redoHistoryCount == 0) return;
            if (source == null || staging == null) return;

            var first = redoHistory[0];
            for (int i = 0; i < redoHistory.Length - 1; i++)
            {
                redoHistory[i] = redoHistory[i + 1];
            }
            redoHistory[^1] = first;

            UndoPushInternal(context);

            var index = IResource.CalculateSubresourceIndex(0, 0, staging.Description.MipLevels);
            var mapped = context.Map(staging, index, MapMode.Write, MapFlags.None);

            var size = mapped.RowPitch * staging.Description.Height;

            fixed (byte* bp = first)
                MemoryCopy(bp, mapped.PData, size);

            context.Unmap(staging, index);
            context.CopyResource(source, staging);
            redoHistoryCount--;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                staging.Dispose();
                disposedValue = true;
            }
        }

        ~ImageHistory()
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