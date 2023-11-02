namespace HexaEngine.Editor.TextEditor
{
    using HexaEngine.Core.Unsafes;

    public unsafe class TextHistory
    {
        private readonly TextHistoryEntry[] undoHistory;
        private readonly TextHistoryEntry[] redoHistory;
        private readonly TextSource source;
        private readonly int maxCount;
        private int undoHistoryCount;
        private int redoHistoryCount;
        private bool disposedValue;

        public TextHistory(TextSource source, int maxCount)
        {
            this.source = source;
            this.maxCount = maxCount;

            undoHistory = new TextHistoryEntry[maxCount];
            redoHistory = new TextHistoryEntry[maxCount];
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

        public void UndoPush()
        {
            UndoPushInternal();
            for (int i = 0; i < redoHistoryCount; i++)
            {
                var index = maxCount - redoHistoryCount;
                redoHistory[index].Release();
            }
            redoHistoryCount = 0;
        }

        private void UndoPushInternal()
        {
            if (source == null)
            {
                return;
            }

            var last = undoHistory[^1];
            for (int i = undoHistory.Length - 1; i != 0; i--)
            {
                undoHistory[i] = undoHistory[i - 1];
            }

            last.Data = AllocT<StdString>();
            *last.Data = source.Text->Clone();

            undoHistory[0] = last;
            undoHistoryCount++;
            if (undoHistoryCount > maxCount)
            {
                undoHistoryCount = maxCount;
            }
        }

        private void RedoPushInternal(TextHistoryEntry entry)
        {
            if (source == null)
            {
                return;
            }

            var last = redoHistory[^1];
            for (int i = redoHistory.Length - 1; i != 0; i--)
            {
                redoHistory[i] = redoHistory[i - 1];
            }

            last.Release();

            last.Data = AllocT<StdString>();
            *last.Data = entry.Data->Clone();

            redoHistory[0] = last;
            redoHistoryCount++;
            if (redoHistoryCount > maxCount)
            {
                redoHistoryCount = maxCount;
            }
        }

        public void Undo()
        {
            if (undoHistoryCount == 0)
            {
                return;
            }

            if (source == null)
            {
                return;
            }

            var first = undoHistory[0];
            for (int i = 0; i < undoHistory.Length - 1; i++)
            {
                undoHistory[i] = undoHistory[i + 1];
            }
            RedoPushInternal(first);

            source.Text = first.Data;

            first.Release();

            undoHistory[^1] = first;
            undoHistoryCount--;
        }

        public void Redo()
        {
            if (redoHistoryCount == 0)
            {
                return;
            }

            if (source == null)
            {
                return;
            }

            var first = redoHistory[0];
            for (int i = 0; i < redoHistory.Length - 1; i++)
            {
                redoHistory[i] = redoHistory[i + 1];
            }

            UndoPushInternal();

            source.Text = first.Data;

            redoHistory[^1] = first;
            redoHistoryCount--;
        }

        public void Dispose()
        {
            if (disposedValue)
            {
                return;
            }

            disposedValue = true;
        }
    }
}