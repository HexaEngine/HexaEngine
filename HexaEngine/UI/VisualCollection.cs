namespace HexaEngine.UI
{
    using System.Collections;

    public class VisualCollection : IList<Visual>
    {
        private readonly List<Visual> visuals = [];

        public Visual this[int index] { get => visuals[index]; set => visuals[index] = value; }

        public int Count => visuals.Count;

        public bool IsReadOnly => false;

        public void Add(Visual item)
        {
            visuals.Add(item);
        }

        public void Clear()
        {
            visuals.Clear();
        }

        public bool Contains(Visual item)
        {
            return visuals.Contains(item);
        }

        public void CopyTo(Visual[] array, int arrayIndex)
        {
            visuals.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Visual> GetEnumerator()
        {
            return visuals.GetEnumerator();
        }

        public int IndexOf(Visual item)
        {
            return visuals.IndexOf(item);
        }

        public void Insert(int index, Visual item)
        {
            visuals.Insert(index, item);
        }

        public bool Remove(Visual item)
        {
            return visuals.Remove(item);
        }

        public void RemoveAt(int index)
        {
            visuals.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return visuals.GetEnumerator();
        }
    }
}