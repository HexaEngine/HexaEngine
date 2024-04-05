namespace HexaEngine.Graphics
{
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public unsafe class Octree
    {
        private readonly OctreeNode root;
        private readonly int maxObjectsPerNode;
        private OctreeNode* nodes;
        private int capacity;
        private int nodeCount;
        private readonly int rootIndex;

        public Octree() : this(128, 4)
        {
        }

        public Octree(int maxObjectsPerNode) : this(maxObjectsPerNode, 4)
        {
        }

        public Octree(int maxObjectsPerNode, int capacity)
        {
            nodes = AllocT<OctreeNode>(capacity);
            ZeroMemoryT(nodes, capacity);
            this.maxObjectsPerNode = maxObjectsPerNode;
            this.capacity = capacity;

            rootIndex = AllocateLeafNode(new BoundingBox(new Vector3(float.MinValue), new Vector3(float.MaxValue)));
            SubdivideNode(GetNode(rootIndex));
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

        public OctreeNode this[int index]
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

        private int AllocateLeafNode(BoundingBox box)
        {
            int index = nodeCount;
            nodeCount++;
            EnsureCapacity(nodeCount);
            OctreeNode* node = &nodes[index];
            return index;
        }

        private int AllocateInternalNode()
        {
            int index = nodeCount;
            nodeCount++;
            EnsureCapacity(nodeCount);
            nodes[index] = new();
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OctreeNode* GetNode(int index)
        {
            return &nodes[index];
        }

        private void FreeNode(int index)
        {
            nodes[index].Objects.Release();

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
                var node = &nodes[i];
                if (node->ParentIndex == indexOld)
                {
                    node->ParentIndex = indexNew;
                }

                if (!node->IsLeafNode())
                {
                    continue;
                }

                for (int j = 0; j < 8; j++)
                {
                    if (node->Children[j] == indexOld)
                    {
                        node->Children[j] = indexNew;
                    }
                }
            }
        }

        private void SubdivideNode(OctreeNode* node)
        {
            var bounds = node->Bounds;
            Vector3 sizeHalf = bounds.Size * 0.5f;
            Vector3 center = bounds.Center;

            node->Children[0] = AllocateLeafNode(new BoundingBox(center + new Vector3(-sizeHalf.X, sizeHalf.Y, -sizeHalf.Z), new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z)));
            node->Children[1] = AllocateLeafNode(new BoundingBox(center + new Vector3(sizeHalf.X, sizeHalf.Y, -sizeHalf.Z), new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z)));
            node->Children[2] = AllocateLeafNode(new BoundingBox(center + new Vector3(-sizeHalf.X, sizeHalf.Y, sizeHalf.Z), new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z)));
            node->Children[3] = AllocateLeafNode(new BoundingBox(center + new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z), new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z)));

            node->Children[4] = AllocateLeafNode(new BoundingBox(center + new Vector3(-sizeHalf.X, -sizeHalf.Y, -sizeHalf.Z), new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z)));
            node->Children[5] = AllocateLeafNode(new BoundingBox(center + new Vector3(sizeHalf.X, -sizeHalf.Y, -sizeHalf.Z), new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z)));
            node->Children[6] = AllocateLeafNode(new BoundingBox(center + new Vector3(-sizeHalf.X, -sizeHalf.Y, sizeHalf.Z), new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z)));
            node->Children[7] = AllocateLeafNode(new BoundingBox(center + new Vector3(sizeHalf.X, -sizeHalf.Y, sizeHalf.Z), new Vector3(sizeHalf.X, sizeHalf.Y, sizeHalf.Z)));
        }

        public void Insert(int objectIndex, BoundingBox objBounds)
        {
            Insert(GetNode(rootIndex), new(objectIndex, objBounds), objBounds);
        }

        private void Insert(OctreeNode* node, OctreeObject objectIndex, BoundingBox objBounds)
        {
            if (!node->Bounds.Intersects(objBounds))
                return;

            if (node->IsLeafNode() && node->Objects.Count < maxObjectsPerNode)
            {
                node->Objects.PushBack(objectIndex);
                return;
            }

            if (node->IsLeafNode())
            {
                SubdivideNode(node);
                for (int i = 0; i < node->Objects.Count; i++)
                {
                    OctreeObject existingObjectIndex = node->Objects[i];
                    for (int j = 0; j < 8; j++)
                    {
                        int childNodeIndex = node->Children[j];
                        Insert(GetNode(childNodeIndex), existingObjectIndex, node->Bounds);
                    }
                }

                node->Objects.Release();
            }

            for (int i = 0; i < 8; i++)
            {
                int childNodeIndex = node->Children[i];
                Insert(GetNode(childNodeIndex), objectIndex, objBounds);
            }
        }

        public void Remove(int objectIndex, BoundingBox objBounds)
        {
            Remove(GetNode(rootIndex), new(objectIndex, objBounds), objBounds);
        }

        private bool Remove(OctreeNode* node, OctreeObject objectIndex, BoundingBox objBounds)
        {
            if (!node->Bounds.Intersects(objBounds))
                return false;

            if (node->IsLeafNode() && node->Objects.Contains(objectIndex))
            {
                node->Objects.Remove(objectIndex);
                return true;
            }

            bool removedFromChild = false;
            for (int i = 0; i < 8; i++)
            {
                int childNodeIndex = node->Children[i];
                removedFromChild |= Remove(GetNode(childNodeIndex), objectIndex, objBounds);
            }

            if (removedFromChild)
            {
                // If object removed from any child, check if this node can be merged
                if (node->IsLeafNode() && node->Objects.Count == 0)
                {
                    // Check if all children are leaf nodes and have no objects
                    bool allChildrenEmpty = true;
                    for (int i = 0; i < 8; i++)
                    {
                        int childNodeIndex = node->Children[i];
                        OctreeNode* childNode = GetNode(childNodeIndex);
                        if (!childNode->IsLeafNode() || childNode->Objects.Count > 0)
                        {
                            allChildrenEmpty = false;
                            break;
                        }
                    }

                    if (allChildrenEmpty)
                    {
                        // Merge children into this node
                        for (int i = 0; i < 8; i++)
                        {
                            int childNodeIndex = node->Children[i];
                            OctreeNode* childNode = GetNode(childNodeIndex);
                            childNode->Objects.Release();
                            FreeNode(childNodeIndex);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public bool Intersects(BoundingBox box, Stack<int> walkStack, List<int> intersections)
        {
            walkStack.Push(rootIndex);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var node = nodes[index];

                if (!node.Bounds.Intersects(box))
                {
                    continue;
                }

                if (node.IsLeafNode())
                {
                    for (int i = 0; i < node.Objects.Count; i++)
                    {
                        var obj = node.Objects[i];
                        if (obj.Bounds.Intersects(box))
                        {
                            intersections.Add(obj.Index);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        walkStack.Push(node.Children[i]);
                    }
                }
            }

            return intersections.Count > 0;
        }

        public bool Intersects(BoundingSphere sphere, Stack<int> walkStack, List<int> intersections)
        {
            walkStack.Push(rootIndex);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var node = nodes[index];

                if (!node.Bounds.Intersects(sphere))
                {
                    continue;
                }

                if (node.IsLeafNode())
                {
                    for (int i = 0; i < node.Objects.Count; i++)
                    {
                        var obj = node.Objects[i];
                        if (obj.Bounds.Intersects(sphere))
                        {
                            intersections.Add(obj.Index);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        walkStack.Push(node.Children[i]);
                    }
                }
            }

            return intersections.Count > 0;
        }

        public bool Intersects(BoundingFrustum frustum, Stack<int> walkStack, List<int> intersections)
        {
            walkStack.Push(rootIndex);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var node = nodes[index];

                if (!node.Bounds.Intersects(frustum))
                {
                    continue;
                }

                if (node.IsLeafNode())
                {
                    for (int i = 0; i < node.Objects.Count; i++)
                    {
                        var obj = node.Objects[i];
                        if (obj.Bounds.Intersects(frustum))
                        {
                            intersections.Add(obj.Index);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        walkStack.Push(node.Children[i]);
                    }
                }
            }

            return intersections.Count > 0;
        }

        public bool Intersects(Ray ray, Stack<int> walkStack, List<OctreeRaycastResult> intersections)
        {
            walkStack.Push(rootIndex);
            while (walkStack.Count > 0)
            {
                int index = walkStack.Pop();
                var node = nodes[index];

                if (node.Bounds.Intersects(ray) != 0)
                {
                    continue;
                }

                if (node.IsLeafNode())
                {
                    for (int i = 0; i < node.Objects.Count; i++)
                    {
                        var obj = node.Objects[i];
                        var depth = obj.Bounds.Intersects(ray);
                        if (depth != null)
                        {
                            intersections.Add(new(obj.Index, depth.Value));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        walkStack.Push(node.Children[i]);
                    }
                }
            }

            return intersections.Count > 0;
        }
    }
}