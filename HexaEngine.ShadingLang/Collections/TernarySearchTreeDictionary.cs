namespace HexaEngine.ShadingLang.Collections
{
    using Hexa.NET.Utilities;
    using System;
    using System.Numerics;

    public unsafe class TernarySearchTreeDictionary<T> where T : unmanaged, IEquatable<T>
    {
        private Node* nodes;
        private int count;
        private int capacity;

        private struct Node
        {
            public int ParentIndex;
            public char SplitChar;
            public int Left;
            public int Middle;
            public int Right;

            public T Value;

            public Node(int parentIndex, char splitChar)
            {
                ParentIndex = parentIndex;
                SplitChar = splitChar;
                Left = -1;
                Middle = -1;
                Right = -1;
                Value = default;
            }
        }

        public TernarySearchTreeDictionary()
        {
            AddNode(-1, '\0');
        }

        public int Capacity
        {
            get => capacity;
            set
            {
                if (nodes == null)
                {
                    nodes = AllocT<Node>(value);
                    capacity = value;
                }
                else
                {
                    nodes = ReAllocT(nodes, value);
                    capacity = value;
                    count = capacity < count ? capacity : count;
                }
            }
        }

        public int Count => count;

        public T this[ReadOnlySpan<char> key]
        {
            get
            {
                if (!LookupNode(key, out int index, out int len) || key.Length != len) throw new KeyNotFoundException();
                return nodes[index].Value;
            }
        }

        public void ShrinkToFit()
        {
            Capacity = count;
        }

        private void Grow(int capacity)
        {
            int newCapacity = count == 0 ? 4 : 2 * count;
            if (newCapacity < capacity)
            {
                newCapacity = capacity;
            }

            Capacity = newCapacity;
        }

        public void Clear()
        {
            count = 0;
            AddNode(-1, '\0');
        }

        private int AddNode(int parentIndex, char splitChar)
        {
            Node node = new(parentIndex, splitChar);
            int index = count;
            if (index < capacity)
            {
                count++;
                nodes[index] = node;
            }
            else
            {
                AddWithResize(node);
            }

            return index;
        }

        private void AddWithResize(Node node)
        {
            int index = count;
            Grow(index + 1);
            count = index + 1;
            nodes[index] = node;
        }

        private void RemoveNode(int index)
        {
            // Could potentially degrade performance because of the loss of cache locality when looking up, but it's as bad as a linked list.
            int last = count - 1;
            if (last != index)
            {
                var lastNode = nodes[last];
                nodes[index] = lastNode;
                int parentIndex = lastNode.ParentIndex;
                if (parentIndex != -1)
                {
                    var parent = &nodes[parentIndex];
                    if (parent->Right == last) parent->Right = index;
                    if (parent->Middle == last) parent->Middle = index;
                    if (parent->Left == last) parent->Left = index;
                }
            }

            count--;
        }

        public bool RemoveKey(ReadOnlySpan<char> key)
        {
            if (nodes == null || count == 0)
            {
                return false;
            }

            if (!LookupNode(key, out var index, out int matchedLength) || matchedLength != key.Length)
            {
                return false;
            }

            while (index != -1)
            {
                Node node = nodes[index];

                if (node.Value.Equals(default) && node.Left == -1 && node.Middle == -1 && node.Right == -1)
                {
                    int parentIndex = node.ParentIndex;

                    if (parentIndex != -1)
                    {
                        Node* parent = &nodes[parentIndex];
                        if (parent->Left == index) parent->Left = -1;
                        if (parent->Middle == index) parent->Middle = -1;
                        if (parent->Right == index) parent->Right = -1;
                    }

                    RemoveNode(index);

                    index = parentIndex;
                }
                else
                {
                    break;
                }
            }

            return true;
        }

        public void InsertRange(IEnumerable<KeyValuePair<string, T>> dict)
        {
            foreach (var (key, value) in dict)
            {
                Insert(key, value);
            }
        }

        public void Insert(ReadOnlySpan<char> key, T value)
        {
            int index = 0;

            foreach (char c in key)
            {
                index = InsertChar(ref index, c);
            }

            (&nodes[index])->Value = value;
        }

        private int InsertChar(ref int index, char c)
        {
            while (true)
            {
                Node node = nodes[index];

                if (node.SplitChar == '\0')
                {
                    node.SplitChar = c;

                    int newIndex = AddNode(index, '\0');
                    node.Middle = newIndex;

                    nodes[index] = node;

                    return newIndex;
                }

                if (c < node.SplitChar)
                {
                    if (node.Left == -1)
                    {
                        node.Left = AddNode(index, '\0');
                        nodes[index] = node;
                    }
                    index = node.Left;
                }
                else if (c > node.SplitChar)
                {
                    if (node.Right == -1)
                    {
                        node.Right = AddNode(index, '\0');
                        nodes[index] = node;
                    }
                    index = node.Right;
                }
                else
                {
                    if (node.Middle == -1)
                    {
                        node.Middle = AddNode(index, '\0');
                        nodes[index] = node;
                    }
                    return node.Middle;
                }
            }
        }

        public bool MatchLongestPrefix(ReadOnlySpan<char> text, out T value, out int matchedLength)
        {
            if (count == 1)
            {
                value = default;
                matchedLength = 0;
                return false;
            }

            if (LookupNode(text, out var nodeIndex, out matchedLength))
            {
                var node = nodes[nodeIndex];
                value = node.Value;
                return true;
            }
            value = default;
            return false;
        }

        private bool LookupNode(ReadOnlySpan<char> text, out int nodeIndex, out int matchedLength)
        {
            int index = 0;
            int lastMatchedIndex = -1;
            matchedLength = 0;

            int i = 0;
            while (i < text.Length)
            {
                Node node = nodes[index];
                char c = text[i];

                if (c < node.SplitChar)
                {
                    if (node.Left == -1)
                    {
                        break;
                    }

                    index = node.Left;
                }
                else if (c > node.SplitChar)
                {
                    if (node.Right == -1)
                    {
                        break;
                    }

                    index = node.Right;
                }
                else
                {
                    i++;

                    if (node.Middle == -1)
                    {
                        break;
                    }

                    lastMatchedIndex = node.Middle;
                    matchedLength = i;
                    index = node.Middle;
                }
            }

            if (lastMatchedIndex != -1)
            {
                nodeIndex = lastMatchedIndex;
                return true;
            }

            nodeIndex = default;
            return false;
        }

        public void Release()
        {
            if (nodes != null)
            {
                Free(nodes);
                nodes = null;
            }

            count = 0;
            capacity = 0;
        }
    }
}