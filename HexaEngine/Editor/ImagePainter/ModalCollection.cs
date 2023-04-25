namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Editor.Dialogs;
    using System.Collections;

    public class ModalCollection<T> : ICollection<T> where T : Modal
    {
        private readonly List<T> list = new();

        public T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public int Count => list.Count;

        public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

        public void Add(T item)
        {
            list.Add(item);
        }

        public T1 Create<T1>() where T1 : T, new()
        {
            T1 value = new();
            list.Add(value);
            return value;
        }

        public T1 GetOrCreate<T1>() where T1 : T, new()
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is T1 item)
                    return item;
            }

            T1 value = new();
            list.Add(value);
            return value;
        }

        public T1 GetOrCreate<T1>(Func<T1> constructor) where T1 : T
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is T1 item)
                    return item;
            }

            T1 value = constructor();
            list.Add(value);
            return value;
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public void Draw()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Draw();
            }
        }

        public void Clear()
        {
            ((ICollection<T>)list).Clear();
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)list).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)list).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }
    }
}