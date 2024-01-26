namespace HexaEngine.Editor
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a history tracking system that allows undo and redo operations with associated actions.
    /// </summary>
    public class History
    {
        private readonly HistoryStack<(object, HistoryItem)> undoStack = new();
        private readonly HistoryStack<(object, HistoryItem)> redoStack = new();
        private readonly int maxEntries;
        private readonly bool autoDispose;
        private readonly object _lock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="History"/> class with a specified maximum number of entries and auto-dispose setting.
        /// </summary>
        /// <param name="maxEntries">The maximum number of entries in the history.</param>
        /// <param name="autoDispose">Whether to automatically dispose objects associated with actions when removed from the history.</param>
        public History(int maxEntries = int.MaxValue, bool autoDispose = true)
        {
            this.maxEntries = maxEntries;
            this.autoDispose = autoDispose;
        }

        /// <summary>
        /// Gets a value indicating whether objects associated with actions should be automatically disposed when removed from the history.
        /// </summary>
        public bool AutoDispose => autoDispose;

        /// <summary>
        /// Gets the maximum number of entries allowed in the history.
        /// </summary>
        public int MaxEntries => maxEntries;

        /// <summary>
        /// Gets a value indicating whether there are actions that can be undone.
        /// </summary>
        public bool CanUndo => undoStack.Count != 0;

        /// <summary>
        /// Gets a value indicating whether there are actions that can be redone.
        /// </summary>
        public bool CanRedo => redoStack.Count != 0;

        /// <summary>
        /// Gets the number of undo-able actions in the history.
        /// </summary>
        public int UndoCount => undoStack.Count;

        /// <summary>
        /// Gets the number of redo-able actions in the history.
        /// </summary>
        public int RedoCount => redoStack.Count;

        /// <summary>
        /// Gets the undo stack. Use <see cref="SyncObject"/> to access it thread-safe.
        /// </summary>
        public IReadOnlyList<(object, HistoryItem)> UndoStack => undoStack;

        /// <summary>
        /// Gets the redo stack. Use <see cref="SyncObject"/> to access it thread-safe.
        /// </summary>
        public IReadOnlyList<(object, HistoryItem)> RedoStack => redoStack;

        /// <summary>
        /// Gets the sync object for thread synchronisation.
        /// </summary>
        public object SyncObject => _lock;

        /// <summary>
        /// Gets the default instance of the <see cref="History"/> class.
        /// </summary>
        public static History Default { get; } = new();

        /// <summary>
        /// Represents an action in the history.
        /// </summary>
        public struct HistoryItem
        {
            /// <summary>
            /// The name of the action.
            /// </summary>
            public string ActionName;

            /// <summary>
            /// The action to perform.
            /// </summary>
            public Action<object> DoAction;

            /// <summary>
            /// The action to undo the previous action.
            /// </summary>
            public Action<object> UndoAction;

            /// <summary>
            /// Initializes a new instance of the <see cref="HistoryItem"/> struct.
            /// </summary>
            /// <param name="actionName">The name of the action.</param>
            /// <param name="doAction">The action to perform.</param>
            /// <param name="undoAction">The action to undo the previous action.</param>
            public HistoryItem(string actionName, Action<object> doAction, Action<object> undoAction)
            {
                ActionName = actionName;
                DoAction = doAction;
                UndoAction = undoAction;
            }
        }

        /// <summary>
        /// Enters the lock.
        /// </summary>
        public void EnterLock()
        {
            Monitor.Enter(_lock);
        }

        /// <summary>
        /// Exits the lock.
        /// </summary>
        public void ExitLock()
        {
            Monitor.Exit(_lock);
        }

        /// <summary>
        /// Pushes a new action onto the undo stack.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="context">The context object associated with the action.</param>
        /// <param name="doAction">The action to perform.</param>
        /// <param name="undoAction">The action to undo the previous action.</param>
        public void Push(string actionName, object context, Action<object> doAction, Action<object> undoAction)
        {
            lock (_lock)
            {
                if (undoStack.Count == maxEntries)
                {
                    DestroyObject(undoStack.Pop());
                }
                undoStack.Push((context, new(actionName, doAction, undoAction)));
                ClearRedo();
            }
        }

        /// <summary>
        /// Pushes a new action onto the undo stack with specific typed parameters.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="target">The target object associated with the action.</param>
        /// <param name="oldValue">The old value associated with the action.</param>
        /// <param name="newValue">The new value associated with the action.</param>
        /// <param name="doAction">The action to perform.</param>
        /// <param name="undoAction">The action to undo the previous action.</param>
        public void Push<T1, T2>(string actionName, T1 target, T2 oldValue, T2 newValue, Action<object> doAction, Action<object> undoAction)
        {
            lock (_lock)
            {
                if (undoStack.Count == maxEntries)
                {
                    DestroyObject(undoStack.Pop());
                }
                var context = new HistoryContext<T1, T2>(target, oldValue, newValue);
                undoStack.Push((context, new(actionName, doAction, undoAction)));
                ClearRedo();
            }
        }

        /// <summary>
        /// Performs an action and pushes it onto the undo stack.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="context">The context object associated with the action.</param>
        /// <param name="doAction">The action to perform.</param>
        /// <param name="undoAction">The action to undo the previous action.</param>
        public void Do(string actionName, object context, Action<object> doAction, Action<object> undoAction)
        {
            lock (_lock)
            {
                if (undoStack.Count == maxEntries)
                {
                    DestroyObject(undoStack.Pop());
                }
                doAction(context);
                undoStack.Push((context, new(actionName, doAction, undoAction)));
                ClearRedo();
            }
        }

        /// <summary>
        /// Performs an action with specific typed parameters and pushes it onto the undo stack.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="target">The target object associated with the action.</param>
        /// <param name="oldValue">The old value associated with the action.</param>
        /// <param name="newValue">The new value associated with the action.</param>
        /// <param name="doAction">The action to perform.</param>
        /// <param name="undoAction">The action to undo the previous action.</param>
        public void Do<T1, T2>(string actionName, T1 target, T2 oldValue, T2 newValue, Action<object> doAction, Action<object> undoAction)
        {
            lock (_lock)
            {
                if (undoStack.Count == maxEntries)
                {
                    DestroyObject(undoStack.Pop());
                }
                var context = new HistoryContext<T1, T2>(target, oldValue, newValue);
                doAction(context);
                undoStack.Push((context, new(actionName, doAction, undoAction)));
                ClearRedo();
            }
        }

        /// <summary>
        /// Pops the last action from the undo stack.
        /// </summary>
        /// <returns>A tuple containing the context, do action, and undo action of the popped action.</returns>
        public (object, Action<object>, Action<object>) Pop()
        {
            lock (_lock)
            {
                var e = undoStack.Pop();
                return (e.Item1, e.Item2.DoAction, e.Item2.UndoAction);
            }
        }

        /// <summary>
        /// Clears both the undo and redo stacks.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                ClearUndo();
                ClearRedo();
            }
        }

        /// <summary>
        /// Clears the redo stack.
        /// </summary>
        public void ClearRedo()
        {
            lock (_lock)
            {
                while (redoStack.TryPop(out var result))
                {
                    DestroyObject(result);
                }
            }
        }

        /// <summary>
        /// Clears the undo stack.
        /// </summary>
        public void ClearUndo()
        {
            lock (_lock)
            {
                while (undoStack.TryPop(out var result))
                {
                    DestroyObject(result);
                }
            }
        }

        /// <summary>
        /// Undoes the last action in the undo stack.
        /// </summary>
        public void Undo()
        {
            lock (_lock)
            {
                var item = undoStack.Pop();
                item.Item2.UndoAction(item.Item1);
                if (redoStack.Count == maxEntries)
                {
                    DestroyObject(redoStack.Pop());
                }
                redoStack.Push(item);
            }
        }

        /// <summary>
        /// Attempts to undo the last action in the undo stack.
        /// </summary>
        /// <returns><c>true</c> if the undo operation was successful; otherwise, <c>false</c>.</returns>
        public bool TryUndo()
        {
            lock (_lock)
            {
                if (undoStack.TryPop(out var item))
                {
                    item.Item2.UndoAction(item.Item1);
                    if (redoStack.Count == maxEntries)
                    {
                        DestroyObject(redoStack.Pop());
                    }
                    redoStack.Push(item);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Redoes the last undone action in the redo stack.
        /// </summary>
        public void Redo()
        {
            lock (_lock)
            {
                var item = redoStack.Pop();
                item.Item2.DoAction(item.Item1);
                if (undoStack.Count == maxEntries)
                {
                    DestroyObject(undoStack.Pop());
                }
                undoStack.Push(item);
            }
        }

        /// <summary>
        /// Attempts to redo the last undone action in the redo stack.
        /// </summary>
        /// <returns><c>true</c> if the redo operation was successful; otherwise, <c>false</c>.</returns>
        public bool TryRedo()
        {
            lock (_lock)
            {
                if (redoStack.TryPop(out var item))
                {
                    item.Item2.DoAction(item.Item1);
                    if (undoStack.Count == maxEntries)
                    {
                        DestroyObject(undoStack.Pop());
                    }
                    undoStack.Push(item);
                    return true;
                }
                return false;
            }
        }

        private void DestroyObject((object, HistoryItem) obj)
        {
            if (autoDispose && obj.Item1 is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}