namespace HexaEngine.UI
{
    using System;
    using System.Collections;

    public class UIElementCollection : IList<UIElement>, ICollection<UIElement>, IEnumerable<UIElement>, IList, ICollection, IEnumerable
    {
        private readonly List<UIElement> objects = new();
        private readonly UIElement parent;

        public UIElementCollection(UIElement parent)
        {
            this.parent = parent;
        }

        public UIElement this[int index] { get => ((IList<UIElement>)objects)[index]; set => ((IList<UIElement>)objects)[index] = value; }

        public int Count => ((ICollection<UIElement>)objects).Count;

        public bool IsReadOnly => ((ICollection<UIElement>)objects).IsReadOnly;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        object? IList.this[int index] { get => this[index]; set => this[index] = (UIElement)value!; }

        public event EventHandler<UIElement>? ElementAdded;

        public event EventHandler<UIElement>? ElementRemoved;

        public void Add(UIElement item)
        {
            if (item.Parent != null)
            {
                throw new InvalidOperationException("The UIElement element already has a parent and cannot be added as a child to another UIElement.");
            }
            item.Parent = parent;
            objects.Add(item);
            ElementAdded?.Invoke(this, item);
            parent.AddVisualChild(item);
            if (parent.IsInitialized && !item.IsInitialized)
            {
                item.Initialize();
            }
            parent.InvalidateMeasure();
        }

        public void Clear()
        {
            foreach (var element in objects)
            {
                parent.RemoveVisualChild(element);
                element.Uninitialize();
                element.Parent = null;
                ElementRemoved?.Invoke(this, element);
            }
            objects.Clear();
            parent.InvalidateMeasure();
        }

        public void AddRange(UIElement[] items)
        {
            foreach (var item in items)
            {
                if (item.Parent != null)
                {
                    throw new InvalidOperationException("The UIElement element already has a parent and cannot be added as a child to another UIElement.");
                }

                ElementAdded?.Invoke(this, item);
                parent.AddVisualChild(item);
                if (parent.IsInitialized && !item.IsInitialized)
                {
                    item.Initialize();
                }
            }
            objects.AddRange(items);
            parent.InvalidateMeasure();
        }

        public bool Remove(UIElement item)
        {
            if (objects.Remove(item))
            {
                parent.RemoveVisualChild(item);
                item.Uninitialize();
                item.Parent = null;

                ElementRemoved?.Invoke(this, item);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            var item = objects[index];
            parent.RemoveVisualChild(item);
            item.Uninitialize();
            item.Parent = null;

            objects.RemoveAt(index);
            ElementRemoved?.Invoke(this, item);
        }

        public bool Contains(UIElement item)
        {
            return ((ICollection<UIElement>)objects).Contains(item);
        }

        public void CopyTo(UIElement[] array, int arrayIndex)
        {
            ((ICollection<UIElement>)objects).CopyTo(array, arrayIndex);
            foreach (var element in array)
            {
                element.Parent = parent;
                ElementAdded?.Invoke(this, element);
            }
        }

        public IEnumerator<UIElement> GetEnumerator()
        {
            return ((IEnumerable<UIElement>)objects).GetEnumerator();
        }

        public int IndexOf(UIElement item)
        {
            return ((IList<UIElement>)objects).IndexOf(item);
        }

        public void Insert(int index, UIElement item)
        {
            item.Parent = parent;
            ((IList<UIElement>)objects).Insert(index, item);
        }

        public void ForEach(Action<UIElement> p)
        {
            foreach (var obj in objects)
            {
                p.Invoke(obj);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)objects).GetEnumerator();
        }

        public void InitializeComponent()
        {
            ForEach(obj => obj.InitializeComponent());
        }

        int IList.Add(object? value)
        {
            if (value is UIElement element)
            {
                int index = Count;
                Add(element);
                return index;
            }
            return -1;
        }

        bool IList.Contains(object? value)
        {
            if (value is UIElement element)
            {
                return Contains(element);
            }

            return false;
        }

        int IList.IndexOf(object? value)
        {
            if (value is UIElement element)
            {
                return IndexOf(element);
            }

            return -1;
        }

        void IList.Insert(int index, object? value)
        {
            if (value is UIElement element)
            {
                Insert(index, element);
            }
        }

        void IList.Remove(object? value)
        {
            if (value is UIElement element)
            {
                Remove(element);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)objects).CopyTo(array, index);
        }
    }
}