namespace HexaEngine.Scenes
{
    using System.Collections;
    using System.Collections.Generic;

    public class SceneNodeList : IList<GameObject>
    {
        private readonly List<GameObject> nodes = new();

        public GameObject this[int index] { get => ((IList<GameObject>)nodes)[index]; set => ((IList<GameObject>)nodes)[index] = value; }

        public int Count => ((ICollection<GameObject>)nodes).Count;

        public bool IsReadOnly => ((ICollection<GameObject>)nodes).IsReadOnly;

        public void Add(GameObject item)
        {
            ((ICollection<GameObject>)nodes).Add(item);
        }

        public void Clear()
        {
            ((ICollection<GameObject>)nodes).Clear();
        }

        public bool Contains(GameObject item)
        {
            return ((ICollection<GameObject>)nodes).Contains(item);
        }

        public void CopyTo(GameObject[] array, int arrayIndex)
        {
            ((ICollection<GameObject>)nodes).CopyTo(array, arrayIndex);
        }

        public IEnumerator<GameObject> GetEnumerator()
        {
            return ((IEnumerable<GameObject>)nodes).GetEnumerator();
        }

        public int IndexOf(GameObject item)
        {
            return ((IList<GameObject>)nodes).IndexOf(item);
        }

        public void Insert(int index, GameObject item)
        {
            ((IList<GameObject>)nodes).Insert(index, item);
        }

        public bool Remove(GameObject item)
        {
            return ((ICollection<GameObject>)nodes).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<GameObject>)nodes).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)nodes).GetEnumerator();
        }
    }
}