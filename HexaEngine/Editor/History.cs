namespace HexaEngine.Editor
{
    using System;
    using System.Collections.Generic;

    public class History
    {
        private readonly Stack<HistoryItem> undostack = new();
        private readonly Stack<HistoryItem> redostack = new();

        public bool CanUndo => undostack.Count != 0;

        public bool CanRedo => redostack.Count != 0;

        public int UndoCount => undostack.Count;

        public int RedoCount => redostack.Count;

        private struct HistoryItem
        {
            public Action DoAction;
            public Action UndoAction;

            public HistoryItem(Action doAction, Action undoAction)
            {
                DoAction = doAction;
                UndoAction = undoAction;
            }
        }

        public void Push(Action doAction, Action undoAction)
        {
            undostack.Push(new(doAction, undoAction));
            redostack.Clear();
        }

        public void Do(Action doAction, Action undoAction)
        {
            doAction();
            undostack.Push(new(doAction, undoAction));
            redostack.Clear();
        }

        public (Action, Action) Pop()
        {
            var e = undostack.Pop();
            return (e.DoAction, e.UndoAction);
        }

        public void Clear()
        {
            undostack.Clear();
            redostack.Clear();
        }

        public void Undo()
        {
            var item = undostack.Pop();
            item.UndoAction();
            redostack.Push(item);
        }

        public bool TryUndo()
        {
            if (undostack.TryPop(out HistoryItem item))
            {
                item.UndoAction();
                redostack.Push(item);
                return true;
            }
            return false;
        }

        public void Redo()
        {
            var item = redostack.Pop();
            item.DoAction();
            undostack.Push(item);
        }

        public bool TryRedo()
        {
            if (redostack.TryPop(out HistoryItem item))
            {
                item.DoAction();
                undostack.Push(item);
                return true;
            }
            return false;
        }
    }
}