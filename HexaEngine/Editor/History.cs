namespace HexaEngine.Editor
{
    using System;
    using System.Collections.Generic;

    public interface IHistoryContext<T1, T2>
    {
        public T1 Target { get; set; }

        public T2 OldValue { get; set; }

        public T2 NewValue { get; set; }
    }

    public struct HistoryContext<T1, T2>
    {
        public T1 Target;
        public T2 OldValue;
        public T2 NewValue;

        public HistoryContext(T1 target, T2 oldValue, T2 newValue)
        {
            Target = target;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public class History
    {
        private readonly Stack<(object, HistoryItem)> undostack = new();
        private readonly Stack<(object, HistoryItem)> redostack = new();

        public bool CanUndo => undostack.Count != 0;

        public bool CanRedo => redostack.Count != 0;

        public int UndoCount => undostack.Count;

        public int RedoCount => redostack.Count;

        public static History Default { get; } = new();

        private struct HistoryItem
        {
            public Action<object> DoAction;
            public Action<object> UndoAction;

            public HistoryItem(Action<object> doAction, Action<object> undoAction)
            {
                DoAction = doAction;
                UndoAction = undoAction;
            }
        }

        public void Push(object context, Action<object> doAction, Action<object> undoAction)
        {
            undostack.Push((context, new(doAction, undoAction)));
            redostack.Clear();
        }

        public void Push<T1, T2>(T1 target, T2 oldValue, T2 newValue, Action<object> doAction, Action<object> undoAction)
        {
            var context = new HistoryContext<T1, T2>(target, oldValue, newValue);
            undostack.Push((context, new(doAction, undoAction)));
            redostack.Clear();
        }

        public void Do(object context, Action<object> doAction, Action<object> undoAction)
        {
            doAction(context);
            undostack.Push((context, new(doAction, undoAction)));
            redostack.Clear();
        }

        public void Do<T1, T2>(T1 target, T2 oldValue, T2 newValue, Action<object> doAction, Action<object> undoAction)
        {
            var context = new HistoryContext<T1, T2>(target, oldValue, newValue);
            doAction(context);
            undostack.Push((context, new(doAction, undoAction)));
            redostack.Clear();
        }

        public (object, Action<object>, Action<object>) Pop()
        {
            var e = undostack.Pop();
            return (e.Item1, e.Item2.DoAction, e.Item2.UndoAction);
        }

        public void Clear()
        {
            undostack.Clear();
            redostack.Clear();
        }

        public void Undo()
        {
            var item = undostack.Pop();
            item.Item2.UndoAction(item.Item1);
            redostack.Push(item);
        }

        public bool TryUndo()
        {
            if (undostack.TryPop(out var item))
            {
                item.Item2.UndoAction(item.Item1);
                redostack.Push(item);
                return true;
            }
            return false;
        }

        public void Redo()
        {
            var item = redostack.Pop();
            item.Item2.DoAction(item.Item1);
            undostack.Push(item);
        }

        public bool TryRedo()
        {
            if (redostack.TryPop(out var item))
            {
                item.Item2.DoAction(item.Item1);
                undostack.Push(item);
                return true;
            }
            return false;
        }
    }
}