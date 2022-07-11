namespace HexaEngine.Scenes
{
    using System.Collections;
    using System.Collections.Generic;

    public class SceneNodeList : IList<SceneNode>
    {
        private readonly List<SceneNode> nodes = new();

        public SceneNode this[int index] { get => ((IList<SceneNode>)nodes)[index]; set => ((IList<SceneNode>)nodes)[index] = value; }

        public int Count => ((ICollection<SceneNode>)nodes).Count;

        public bool IsReadOnly => ((ICollection<SceneNode>)nodes).IsReadOnly;

        public void Add(SceneNode item)
        {
            ((ICollection<SceneNode>)nodes).Add(item);
        }

        public void Clear()
        {
            ((ICollection<SceneNode>)nodes).Clear();
        }

        public bool Contains(SceneNode item)
        {
            return ((ICollection<SceneNode>)nodes).Contains(item);
        }

        public void CopyTo(SceneNode[] array, int arrayIndex)
        {
            ((ICollection<SceneNode>)nodes).CopyTo(array, arrayIndex);
        }

        public IEnumerator<SceneNode> GetEnumerator()
        {
            return ((IEnumerable<SceneNode>)nodes).GetEnumerator();
        }

        public int IndexOf(SceneNode item)
        {
            return ((IList<SceneNode>)nodes).IndexOf(item);
        }

        public void Insert(int index, SceneNode item)
        {
            ((IList<SceneNode>)nodes).Insert(index, item);
        }

        public bool Remove(SceneNode item)
        {
            return ((ICollection<SceneNode>)nodes).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<SceneNode>)nodes).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)nodes).GetEnumerator();
        }
    }
}