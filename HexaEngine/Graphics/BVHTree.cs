namespace HexaEngine.Graphics
{
    using Hexa.NET.Mathematics;

    public unsafe class BVHTree
    {
        private BVHNode* nodes;
        private int capacity;
        private int nodeCount;
        private int rootIndex;

        public BVHTree() : this(4)
        {
        }

        public BVHTree(int capacity)
        {
            nodes = AllocT<BVHNode>(capacity);
            ZeroMemoryT(nodes, capacity);
            this.capacity = capacity;
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

                nodes = ReAllocT(nodes, value);
                capacity = value;

                // zero out new nodes.
                for (int i = nodeCount; i < value; i++)
                {
                    nodes[i] = default;
                }
            }
        }

        public int Count => nodeCount;

        public BVHNode this[int index]
        {
            get => nodes[index];
            set => nodes[index] = value;
        }

        private void Grow(int capacity)
        {
            if (this.capacity < capacity)
            {
                Capacity = capacity;
            }
        }

        public void EnsureCapacity(int capacity)
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

        private int AllocateLeafNode(int objectIndex, BoundingBox box)
        {
            int index = nodeCount;
            nodeCount++;
            EnsureCapacity(nodeCount);
            nodes[index] = new() { Box = box, ObjectIndex = objectIndex, IsLeaf = true };
            return index;
        }

        private int AllocateInternalNode()
        {
            int index = nodeCount;
            nodeCount++;
            EnsureCapacity(nodeCount);
            nodes[index] = new() { IsLeaf = false };
            return index;
        }

        private void SplitNode(BoundingBox box, int leafIndex, int sibling)
        {
            int oldParent = nodes[sibling].ParentIndex;
            int newParent = AllocateInternalNode();
            nodes[newParent].ParentIndex = oldParent;
            nodes[newParent].Box = BoundingBox.CreateMerged(box, nodes[sibling].Box);
            if (oldParent != 0)
            {
                // The sibling was not the root
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
            else
            {
                // The sibling was the root
                nodes[newParent].Child1 = sibling;
                nodes[newParent].Child2 = leafIndex;
                nodes[sibling].ParentIndex = newParent;
                nodes[leafIndex].ParentIndex = newParent;
                rootIndex = newParent;
            }
        }

        private void RefitHierarchy(int startIndex)
        {
            while (startIndex != 0)
            {
                int child1 = nodes[startIndex].Child1;
                int child2 = nodes[startIndex].Child2;
                nodes[startIndex].Box = BoundingBox.CreateMerged(nodes[child1].Box, nodes[child2].Box);
                startIndex = nodes[startIndex].ParentIndex;
            }
        }

        private void FreeNode(int index)
        {
            if (index == 0 && nodeCount == 1)
            {
                nodeCount--;
                return;
            }

            if (index == nodeCount - 1)
            {
                nodes[index] = default;
                nodeCount--;
                return;
            }

            nodes[index] = nodes[nodeCount - 1];
            nodes[nodeCount - 1] = default;

            MoveNode(nodeCount - 1, index);
            nodeCount--;
        }

        private void MoveNode(int indexOld, int indexNew)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                if (nodes[i].ParentIndex == indexOld)
                {
                    nodes[i].ParentIndex = indexNew;
                }

                if (nodes[i].Child1 == indexOld)
                {
                    nodes[i].Child1 = indexNew;
                }

                if (nodes[i].Child2 == indexOld)
                {
                    nodes[i].Child2 = indexNew;
                }
            }
        }

        private int SearchBestForInsert(BoundingBox box)
        {
            int index = 0;

            while (true)
            {
                var node = nodes[index];
                if (node.IsLeaf)
                {
                    break;
                }

                var child1 = node.Child1;
                var child2 = node.Child2;
                var box1 = nodes[child1].Box;
                var box2 = nodes[child2].Box;
                var cost1 = BoundingBox.CreateMerged(box1, box).Area();
                var cost2 = BoundingBox.CreateMerged(box2, box).Area();
                if (cost1 > cost2)
                {
                    index = child2;
                }
                else
                {
                    index = child1;
                }
            }

            return index;
        }

        public void InsertLeaf(int objectIndex, BoundingBox box)
        {
            int leafIndex = AllocateLeafNode(objectIndex, box);
            if (nodeCount == 1)
            {
                rootIndex = leafIndex;
            }

            int sibling = SearchBestForInsert(box);
            SplitNode(box, leafIndex, sibling);
            RefitHierarchy(nodes[leafIndex].ParentIndex);
        }

        public void RemoveLeaf(int leafIndex)
        {
            int parent = nodes[leafIndex].ParentIndex;

            if (parent != 0)
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
                parent = grandparent;
            }
            else
            {
                if (nodes[parent].Child1 == leafIndex)
                {
                    nodes[parent].Child1 = 0;
                }
                else
                {
                    nodes[parent].Child2 = 0;
                }
            }

            FreeNode(leafIndex);
            RefitHierarchy(parent);
        }

        public int FindLeafNode(int objectIndex)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                if (nodes[i].ObjectIndex == objectIndex)
                {
                    return i;
                }
            }

            return -1;
        }

        public float ComputeCost()
        {
            float cost = 0.0f;
            for (int i = 0; i < nodeCount; ++i)
            {
                cost += nodes[i].Box.Area();
            }
            return cost;
        }

        public bool Intersects(BoundingBox box, Stack<int> walkStack, List<int> intersections)
        {
            if (nodeCount == 0)
            {
                return false;
            }

            if (nodeCount == 1)
            {
                return nodes[0].Box.Intersects(box);
            }

            walkStack.Push(0);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var child1 = nodes[index].Child1;
                var child2 = nodes[index].Child2;
                var node1 = nodes[child1];
                var node2 = nodes[child2];
                if (node1.Box.Intersects(box))
                {
                    if (node1.IsLeaf)
                    {
                        intersections.Add(child1);
                    }
                    else
                    {
                        walkStack.Push(child1);
                    }
                }
                if (node2.Box.Intersects(box))
                {
                    if (node2.IsLeaf)
                    {
                        intersections.Add(child2);
                    }
                    else
                    {
                        walkStack.Push(child2);
                    }
                }
            }

            return intersections.Count > 0;
        }

        public bool Intersects(BoundingSphere sphere, Stack<int> walkStack, List<int> intersections)
        {
            if (nodeCount == 0)
            {
                return false;
            }

            if (nodeCount == 1)
            {
                return nodes[0].Box.Intersects(sphere);
            }

            walkStack.Push(0);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var child1 = nodes[index].Child1;
                var child2 = nodes[index].Child2;
                var node1 = nodes[child1];
                var node2 = nodes[child2];
                if (node1.Box.Intersects(sphere))
                {
                    if (node1.IsLeaf)
                    {
                        intersections.Add(child1);
                    }
                    else
                    {
                        walkStack.Push(child1);
                    }
                }
                if (node2.Box.Intersects(sphere))
                {
                    if (node2.IsLeaf)
                    {
                        intersections.Add(child2);
                    }
                    else
                    {
                        walkStack.Push(child2);
                    }
                }
            }

            return intersections.Count > 0;
        }

        public bool Intersects(BoundingFrustum frustum, Stack<int> walkStack, List<int> intersections)
        {
            if (nodeCount == 0)
            {
                return false;
            }

            if (nodeCount == 1)
            {
                return nodes[0].Box.Intersects(frustum);
            }

            walkStack.Push(0);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var child1 = nodes[index].Child1;
                var child2 = nodes[index].Child2;
                var node1 = nodes[child1];
                var node2 = nodes[child2];
                if (node1.Box.Intersects(frustum))
                {
                    if (node1.IsLeaf)
                    {
                        intersections.Add(child1);
                    }
                    else
                    {
                        walkStack.Push(child1);
                    }
                }
                if (node2.Box.Intersects(frustum))
                {
                    if (node2.IsLeaf)
                    {
                        intersections.Add(child2);
                    }
                    else
                    {
                        walkStack.Push(child2);
                    }
                }
            }

            return intersections.Count > 0;
        }

        public bool Intersects(Ray ray, Stack<int> walkStack, List<BVHRaycastResult> intersections)
        {
            if (nodeCount == 0)
            {
                return false;
            }

            if (nodeCount == 1)
            {
                var result = nodes[0].Box.Intersects(ray);
                if (result != null)
                {
                    intersections.Add(new(0, result.Value));
                }
                return result != null;
            }

            walkStack.Push(0);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var child1 = nodes[index].Child1;
                var child2 = nodes[index].Child2;
                var node1 = nodes[child1];
                var node2 = nodes[child2];
                var depth1 = node1.Box.Intersects(ray);
                var depth2 = node2.Box.Intersects(ray);
                if (depth1 != null)
                {
                    if (node1.IsLeaf)
                    {
                        intersections.Add(new BVHRaycastResult(child1, depth1.Value));
                    }
                    else
                    {
                        walkStack.Push(child1);
                    }
                }
                if (depth2 != null)
                {
                    if (node2.IsLeaf)
                    {
                        intersections.Add(new BVHRaycastResult(child2, depth2.Value));
                    }
                    else
                    {
                        walkStack.Push(child2);
                    }
                }
            }

            return intersections.Count > 0;
        }

        public void Release()
        {
            if (nodes != null)
            {
                Free(nodes);
                nodes = null;
            }

            nodeCount = 0;
            capacity = 0;
        }
    }
}