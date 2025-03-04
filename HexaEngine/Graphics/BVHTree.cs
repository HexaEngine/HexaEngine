namespace HexaEngine.Graphics
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities.Threading;
    using MagicPhysX;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public enum BVHFilterResult
    {
        Skip,
        Keep,
        SkipReturnKeep,
    }

    public class BVHTree<T> where T : notnull
    {
        private BVHNode<T>[] nodes;
        private int capacity;
        private int nodeCount;
        private int deadlist = int.MaxValue;
        private int deadlistCount;
        private readonly ReadWriteLock readWriteLock = new(maxReader: 64, maxWriter: 1);

        public BVHTree() : this(4)
        {
        }

        public BVHTree(int capacity)
        {
            nodes = new BVHNode<T>[capacity];
            this.capacity = capacity;
            AllocateInternalNode(); // root.
        }

        public int Capacity
        {
            get => capacity;
            set
            {
                if (capacity == value)
                {
                    return;
                }
                readWriteLock.BeginWrite();
                Resize(value);
                readWriteLock.EndWrite();
            }
        }

        private void Resize(int value)
        {
            Array.Resize(ref nodes, value);
            capacity = value;

            // zero out new nodes.
            for (int i = nodeCount; i < value; i++)
            {
                nodes[i] = default;
            }
        }

        public int Count => nodeCount;

        public BVHNode<T> this[int index]
        {
            get
            {
                readWriteLock.BeginRead();
                var node = nodes[index];
                readWriteLock.EndRead();
                return node;
            }
            set
            {
                readWriteLock.BeginWrite();
                nodes[index] = value;
                readWriteLock.EndWrite();
            }
        }

        private void Grow(int capacity)
        {
            if (this.capacity < capacity)
            {
                Resize(capacity);
            }
        }

        private void EnsureCapacity(int capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity * 2);
            }
        }

        public void Compact()
        {
            if (nodeCount < capacity)
            {
                Capacity = nodeCount;
            }
        }

        public void Clear()
        {
            readWriteLock.BeginWrite();
            deadlist = int.MaxValue;
            deadlistCount = 0;
            nodeCount = 0;
            Array.Clear(nodes);
            readWriteLock.EndWrite();
        }

        private int GetNextIndex()
        {
            int index;
            if (deadlistCount != 0)
            {
                index = deadlist;
                deadlistCount--;
                if (deadlistCount > 0)
                {
                    for (int i = deadlist + 1; i < capacity; i++)
                    {
                        if (nodes[i].ParentIndex == -1)
                        {
                            deadlist = i;
                            break;
                        }
                    }
                }
                else
                {
                    deadlist = int.MaxValue;
                }
            }
            else
            {
                index = nodeCount;
            }
            nodeCount++;
            return index;
        }

        private int AllocateLeafNode(T value, BoundingBox box)
        {
            int index = GetNextIndex();
            EnsureCapacity(nodeCount);
            nodes[index] = new() { Box = box, Value = value, IsLeaf = true, Child1 = -1, Child2 = -1 };
            return index;
        }

        private int AllocateInternalNode()
        {
            int index = GetNextIndex();
            EnsureCapacity(nodeCount);
            nodes[index] = new() { IsLeaf = false, Child1 = -1, Child2 = -1 };
            return index;
        }

        private void InsertNode(BoundingBox box, int leafIndex, int sibling)
        {
            ref var siblingNode = ref nodes[sibling];
            if (siblingNode.IsLeaf)
            {
                SplitNode(box, leafIndex, sibling);
            }
            else
            {
                if (siblingNode.Child1 == -1)
                {
                    siblingNode.Child1 = leafIndex;
                }
                else
                {
                    siblingNode.Child2 = leafIndex;
                }

                nodes[leafIndex].ParentIndex = sibling;
                nodes[sibling].Box = box;
            }
        }

        private void SplitNode(BoundingBox box, int leafIndex, int sibling)
        {
            int oldParent = nodes[sibling].ParentIndex;
            int newParent = AllocateInternalNode();

            nodes[newParent].ParentIndex = oldParent;
            nodes[newParent].Box = BoundingBox.CreateMerged(box, nodes[sibling].Box);

            if (nodes[oldParent].Child1 == sibling)
            {
                nodes[oldParent].Child1 = newParent;
            }
            else
            {
                nodes[oldParent].Child2 = newParent;
            }

            nodes[newParent].Child1 = sibling;
            nodes[newParent].Child2 = leafIndex;

            nodes[sibling].ParentIndex = newParent;
            nodes[leafIndex].ParentIndex = newParent;
        }

        private void RefitHierarchy(int startIndex)
        {
            while (startIndex != 0 && startIndex != -1)
            {
                int child1 = nodes[startIndex].Child1;
                int child2 = nodes[startIndex].Child2;
                var a = nodes[child1].Box;
                var b = nodes[child2].Box;
                nodes[startIndex].Box.Min = Vector3.Min(a.Min, b.Min);
                nodes[startIndex].Box.Max = Vector3.Max(a.Max, b.Max);
                startIndex = nodes[startIndex].ParentIndex;
            }

            // Update the root node box as well
            int rootChild1 = nodes[0].Child1;
            int rootChild2 = nodes[0].Child2;
            if (rootChild1 == -1 || rootChild2 == -1)
            {
                if (rootChild1 != -1)
                {
                    nodes[0].Box = nodes[rootChild1].Box;
                }
                else if (rootChild2 != -1)
                {
                    nodes[0].Box = nodes[rootChild2].Box;
                }
                return;
            }
            nodes[0].Box = BoundingBox.CreateMerged(nodes[rootChild1].Box, nodes[rootChild2].Box);
        }

        private void FreeNode(int index)
        {
            if (index == nodeCount - 1)
            {
                nodes[index] = default;
                nodeCount--;
                return;
            }

            nodes[index] = new() { ParentIndex = -1 };
            deadlistCount++;
            deadlist = Math.Min(index, deadlist);
            nodeCount--;
        }

        private int SearchBestForInsert(BoundingBox box)
        {
            int index = 0;  // Start at root node (which is always index 0)

            while (true)
            {
                var node = nodes[index];
                if (node.IsLeaf || node.Child1 == -1 || node.Child2 == -1)
                {
                    break;
                }

                var child1 = node.Child1;
                var child2 = node.Child2;
                var box1 = nodes[child1].Box;
                var box2 = nodes[child2].Box;
                var cost1 = BoundingBox.CreateMerged(box1, box).Area();
                var cost2 = BoundingBox.CreateMerged(box2, box).Area();

                index = cost1 > cost2 ? child2 : child1;
            }

            return index;
        }

        public int UpdateLeaf(int index, BoundingBox box)
        {
            var node = nodes[index];
            var parent = nodes[node.ParentIndex];
            BVHNode<T> sibling;
            if (parent.Child1 == index)
            {
                sibling = nodes[parent.Child2];
            }
            else
            {
                sibling = nodes[parent.Child1];
            }
            var oldCost = parent.Box.Area();
            var newCost = BoundingBox.CreateMerged(sibling.Box, box).Area();
            const float areaExpansionThreshold = 1.1f; // 10% threshold
            if (newCost > oldCost * areaExpansionThreshold)
            {
                RemoveLeaf(index);
                return InsertLeaf(node.Value, box);
            }
            else
            {
                nodes[index].Box = box;
                RefitHierarchy(node.ParentIndex);
                return index;
            }
        }

        public int InsertLeaf(T value, BoundingBox box)
        {
            readWriteLock.BeginWrite();

            int leafIndex = AllocateLeafNode(value, box);

            int sibling = SearchBestForInsert(box);
            InsertNode(box, leafIndex, sibling);
            RefitHierarchy(nodes[leafIndex].ParentIndex);

            readWriteLock.EndWrite();
            return leafIndex;
        }

        public bool RemoveLeaf(int leafIndex)
        {
            readWriteLock.BeginWrite();

            var node = nodes[leafIndex];

            if (!node.IsLeaf)
            {
                readWriteLock.EndWrite();
                return false;
            }

            int parent = node.ParentIndex;

            if (parent != 0) // if not root collapse node.
            {
                int siblingIndex;
                if (nodes[parent].Child1 == leafIndex)
                {
                    siblingIndex = nodes[parent].Child2;
                }
                else
                {
                    siblingIndex = nodes[parent].Child1;
                }

                int grandparent = nodes[parent].ParentIndex;
                if (nodes[grandparent].Child1 == parent)
                {
                    nodes[grandparent].Child1 = siblingIndex;
                }
                else
                {
                    nodes[grandparent].Child2 = siblingIndex;
                }
                nodes[siblingIndex].ParentIndex = grandparent;

                FreeNode(parent);
                parent = grandparent;
            }
            else
            {
                if (nodes[parent].Child1 == leafIndex)
                {
                    nodes[parent].Child1 = -1;
                }
                else
                {
                    nodes[parent].Child2 = -1;
                }
            }

            FreeNode(leafIndex);
            RefitHierarchy(parent);

            readWriteLock.EndWrite();

            return true;
        }

        public int FindLeafNode(T value)
        {
            readWriteLock.BeginRead();
            for (int i = 0; i < nodeCount; i++)
            {
                if (nodes[i].Value.Equals(value))
                {
                    readWriteLock.EndRead();
                    return i;
                }
            }

            readWriteLock.EndRead();
            return -1;
        }

        public float ComputeCost()
        {
            readWriteLock.BeginRead();
            float cost = 0.0f;
            for (int i = 0; i < nodeCount; ++i)
            {
                cost += nodes[i].Box.Area();
            }
            readWriteLock.EndRead();
            return cost;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref BVHNode<T> GetNode(int index)
        {
            return ref nodes[index];
        }

        public unsafe IEnumerable<BVHNode<T>> Enumerate(Func<BVHNode<T>, BVHFilterResult> selector, Stack<int> walkStack)
        {
            readWriteLock.BeginRead();

            if (nodeCount == 0)
            {
                readWriteLock.EndRead();
                yield break;
            }

            if (nodeCount == 1)
            {
                if (selector(nodes[0]) == BVHFilterResult.Keep)
                {
                    yield return nodes[0];
                }

                readWriteLock.EndRead();
                yield break;
            }

            walkStack.Push(0);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var n = nodes[index];
                if (n.Child1 != -1)
                {
                    var node1 = nodes[n.Child1];
                    var result1 = selector(node1);
                    if (result1 != BVHFilterResult.Skip)
                    {
                        if (node1.IsLeaf)
                        {
                            if (result1 != BVHFilterResult.SkipReturnKeep)
                            {
                                yield return node1;
                            }
                        }
                        else
                        {
                            walkStack.Push(n.Child1);
                        }
                    }
                }

                if (n.Child2 != -1)
                {
                    var node2 = nodes[n.Child2];
                    var result2 = selector(node2);
                    if (result2 != BVHFilterResult.Skip)
                    {
                        if (node2.IsLeaf)
                        {
                            if (result2 != BVHFilterResult.SkipReturnKeep)
                            {
                                yield return node2;
                            }
                        }
                        else
                        {
                            walkStack.Push(n.Child2);
                        }
                    }
                }
            }

            readWriteLock.EndRead();
        }

        public unsafe IEnumerable<BVHNode<T>> Enumerate<TUserdata>(Func<BVHNode<T>, TUserdata, BVHFilterResult> selector, Stack<int> walkStack, TUserdata userdata)
        {
            readWriteLock.BeginRead();

            if (nodeCount == 0)
            {
                readWriteLock.EndRead();
                yield break;
            }

            if (nodeCount == 1)
            {
                if (selector(nodes[0], userdata) == BVHFilterResult.Keep)
                {
                    if (nodes[0].IsLeaf)
                    {
                        yield return nodes[0];
                    }
                }

                readWriteLock.EndRead();
                yield break;
            }

            walkStack.Push(0);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var n = nodes[index];
                if (n.Child1 != -1)
                {
                    var node1 = nodes[n.Child1];
                    var result1 = selector(node1, userdata);
                    if (result1 != BVHFilterResult.Skip)
                    {
                        if (node1.IsLeaf)
                        {
                            if (result1 != BVHFilterResult.SkipReturnKeep)
                            {
                                yield return node1;
                            }
                        }
                        else
                        {
                            walkStack.Push(n.Child1);
                        }
                    }
                }

                if (n.Child2 != -1)
                {
                    var node2 = nodes[n.Child2];
                    var result2 = selector(node2, userdata);
                    if (result2 != BVHFilterResult.Skip)
                    {
                        if (node2.IsLeaf)
                        {
                            if (result2 != BVHFilterResult.SkipReturnKeep)
                            {
                                yield return node2;
                            }
                        }
                        else
                        {
                            walkStack.Push(n.Child2);
                        }
                    }
                }
            }

            readWriteLock.EndRead();
        }

        public string Debug()
        {
            if (nodeCount == 0)
            {
                return "Tree is empty.";
            }

            return DebugNode(0, 0) + $"Count: {Count}, Capacity: {Capacity}, Dead Count: {deadlistCount} \n";
        }

        // Recursive helper method to visualize the tree
        private string DebugNode(int nodeIndex, int depth)
        {
            if (nodeIndex == -1 && depth > 0)
            {
                return "";
            }

            var node = nodes[nodeIndex];

            // Indentation for the current depth
            string indent = new(' ', depth * 4);

            // Create a representation for the current node
            string nodeRepresentation = indent + $"(Node {nodeIndex}, IsLeaf: {node.IsLeaf}, Parent: {node.ParentIndex}), {node.Box}\n";

            // If the node is not a leaf, recursively add the children
            if (!node.IsLeaf)
            {
                nodeRepresentation += DebugNode(node.Child1, depth + 1);
                nodeRepresentation += DebugNode(node.Child2, depth + 1);
            }

            return nodeRepresentation;
        }
    }
}