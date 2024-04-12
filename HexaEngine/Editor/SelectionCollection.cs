namespace HexaEngine.Editor
{
    using HexaEngine.Core.Editor;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class SelectionCollection : ICollection<object>, IEnumerable, IEnumerable<object>
    {
        private readonly List<object> _objects = [];
        private Type? type;

        public object this[int index] { get => _objects[index]; set => _objects[index] = value; }

        public int Count => _objects.Count;

        public bool SelectedMultiple => _objects.Count > 1;

        public Type? Type => type;

        public object SyncRoot => this;

        public bool IsReadOnly => ((ICollection<object>)_objects).IsReadOnly;

        public static readonly SelectionCollection Global = new();

        public void PurgeSelection()
        {
            foreach (object obj in _objects)
            {
                if (obj is IHierarchyObject hierarchyObject)
                {
                    hierarchyObject.Parent?.RemoveChild(hierarchyObject);
                }
            }
            ClearSelection();
        }

        public void MoveSelection(IHierarchyObject parent)
        {
            foreach (object obj in _objects)
            {
                if (obj is IHierarchyObject hierarchyObject)
                {
                    hierarchyObject.Uninitialize();
                }
            }
            foreach (object obj in _objects)
            {
                if (obj is IHierarchyObject hierarchyObject)
                {
                    parent.AddChild(hierarchyObject);
                }
            }
        }

        public void AddSelection(object obj)
        {
            if (obj is IEditorSelectable selectable)
            {
                selectable.IsEditorSelected = true;
            }

            if (_objects.Count == 0)
            {
                type = obj.GetType();
            }
            else if (!(type?.IsInstanceOfType(obj) ?? true))
            {
                type = null;
            }

            if (_objects.Contains(obj))
            {
                return;
            }

            _objects.Add(obj);
        }

        public void AddOverwriteSelection(object obj)
        {
            ClearSelection();
            AddSelection(obj);
        }

        public void AddMultipleSelection(IEnumerable<object> objects)
        {
            foreach (object obj in objects)
            {
                AddSelection(obj);
            }
        }

        public bool RemoveSelection(object item)
        {
            if (item is IEditorSelectable selectable)
            {
                selectable.IsEditorSelected = false;
            }

            var result = _objects.Remove(item);

            if (_objects.Count == 0)
            {
                type = null;
            }

            return result;
        }

        public void RemoveMultipleSelection(IEnumerable<object> objects)
        {
            foreach (object obj in objects)
            {
                RemoveSelection(obj);
            }
        }

        public void ClearSelection()
        {
            foreach (object obj in _objects)
            {
                if (obj is IEditorSelectable selectable)
                {
                    selectable.IsEditorSelected = false;
                }
            }

            _objects.Clear();
            type = null;
        }

        public object? First() => _objects.Count == 0 ? null : _objects[0];

        public object? Last() => _objects[^1];

        public T? First<T>()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
                if (obj is T t)
                {
                    return t;
                }
            }
            return default;
        }

        public T? Last<T>()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                var obj = _objects[i];
                if (obj is T t)
                {
                    return t;
                }
            }
            return default;
        }

        public bool Contains(object item)
        {
            return _objects.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            _objects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

        public void Add(object item)
        {
            AddSelection(item);
        }

        public void Clear()
        {
            ClearSelection();
        }

        public bool Remove(object item)
        {
            return RemoveSelection(item);
        }
    }
}