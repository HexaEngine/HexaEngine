//#define MoveAndReplace

namespace HexaEngine.PerformanceTests
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public class Octree<T> where T : notnull
    {
        private OctreeNode<T>[] nodes;
        private int capacity;
        private int count;

#if !MoveAndReplace
        private int deadlist = int.MaxValue;
        private int deadlistCount;
#endif

        private BoundingBox worldBounds;

        private readonly Dictionary<T, int> objectToNode = [];

        public Octree(BoundingBox boundingBox)
        {
            worldBounds = boundingBox;
            capacity = 128;
            nodes = new OctreeNode<T>[capacity];
            AllocateNode(-1, boundingBox); // root is always 0.
        }

        public Octree(int capacity, BoundingBox boundingBox)
        {
            worldBounds = boundingBox;
            this.capacity = capacity;
            nodes = new OctreeNode<T>[capacity];
            AllocateNode(-1, boundingBox); // root is always 0.
        }

        public int Capacity
        {
            get => capacity;
            set
            {
                if (value < count)
                {
                    return;
                }

                Array.Resize(ref nodes, value);
                capacity = value;
            }
        }

        private int AllocateNode(int parentIndex, BoundingBox box)
        {
            int index = count;
#if !MoveAndReplace
            if (deadlistCount != 0)
            {
                index = deadlist;
                deadlistCount--;
                if (deadlistCount != 0)
                {
                    for (int i = deadlist + 1; i < capacity; i++)
                    {
                        var node = nodes[i];
                        if (node.ParentIndex == -1)
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
#endif

            if (count + 1 > capacity)
            {
                Capacity = capacity * 2;
            }

            nodes[index] = new() { Bounds = box, ParentIndex = parentIndex };
            count++;
            return index;
        }

        private void FreeNode(int index)
        {
            int lastIndex = count - 1;
            if (lastIndex == index)
            {
                count--;
                return;
            }

#if MoveAndReplace

            var lastNode = nodes[lastIndex];
            if (lastNode.Children[0] != -1)
            {
                for (int i = 0; i < 8; i++)
                {
                    var childIndex = lastNode.Children[i];
                    nodes[childIndex].ParentIndex = index;
                }
            }

            var parent = nodes[lastNode.ParentIndex];
            for (int i = 0; i < 8; i++)
            {
                var sibling = parent.Children[i];
                if (sibling == lastIndex)
                {
                    nodes[lastNode.ParentIndex].Children[i] = index;
                    break;
                }
            }

            if (lastNode.Objects != null)
            {
                foreach (var obj in lastNode.Objects)
                {
                    objectToNode[obj.Value] = index;
                }
            }

            nodes[index] = lastNode;
            nodes[lastIndex] = new() { ParentIndex = -1 };
            count--;
#else

            // dead list approach
            deadlistCount++;
            deadlist = Math.Min(index, deadlist);
            nodes[index] = new() { ParentIndex = -1 };
            count--;
#endif
        }

        public bool AddObject(T obj, BoundingSphere sphere)
        {
            if (!worldBounds.Intersects(sphere))
            {
                return false;
            }

            InsertObject(0, obj, sphere);

            return true;
        }

        private int FindBestNodeForInsert(int nodeIndex, BoundingSphere sphere, out int depth)
        {
            depth = 0;
            if (nodes[nodeIndex].Children[0] == -1)
            {
                return nodeIndex;
            }

            int currentNode = nodeIndex;

            while (true)
            {
                bool found = false;

                for (int i = 0; i < 8; i++)
                {
                    var childIndex = nodes[currentNode].Children[i];

                    if (childIndex == -1)
                    {
                        return currentNode;
                    }

                    var child = nodes[childIndex];

                    if (child.Bounds.Contains(sphere) == ContainmentType.Contains)
                    {
                        depth++;
                        currentNode = childIndex;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    break;
                }
            }

            return currentNode;
        }

        private bool InsertObject(int nodeIndex, T obj, BoundingSphere sphere)
        {
            var node = nodes[nodeIndex];

            if (node.Bounds.Contains(sphere) != ContainmentType.Contains)
            {
                return false;
            }

            int nodeToInsert = FindBestNodeForInsert(nodeIndex, sphere, out int depth);

            var parent = nodes[nodeToInsert];

            parent.AddObject(obj, sphere);

            nodes[nodeToInsert] = parent;

            objectToNode[obj] = nodeToInsert;

            const int someThreshold = 32;

            if (parent.Objects!.Count > someThreshold && depth < 8)
            {
                SplitNode(nodeToInsert);
            }

            return true;
        }

        private void SplitNode(int nodeIndex)
        {
            var node = nodes[nodeIndex];

            if (node.Children[0] != -1)
            {
                return;
            }

            var bounds = node.Bounds;
            var center = bounds.Center;
            var min = bounds.Min;
            var max = bounds.Max;

            // Child 0: (-X, -Y, -Z)
            node.Children[0] = AllocateNode(nodeIndex, new BoundingBox(min, center));

            // Child 1: (-X, -Y, +Z)
            node.Children[1] = AllocateNode(nodeIndex, new BoundingBox(new Vector3(min.X, min.Y, center.Z), new Vector3(center.X, center.Y, max.Z)));

            // Child 2: (+X, -Y, -Z)
            node.Children[2] = AllocateNode(nodeIndex, new BoundingBox(new Vector3(center.X, min.Y, min.Z), new Vector3(max.X, center.Y, center.Z)));

            // Child 3: (+X, -Y, +Z)
            node.Children[3] = AllocateNode(nodeIndex, new BoundingBox(new Vector3(center.X, min.Y, center.Z), new Vector3(max.X, center.Y, max.Z)));

            // Child 4: (-X, +Y, -Z)
            node.Children[4] = AllocateNode(nodeIndex, new BoundingBox(new Vector3(min.X, center.Y, min.Z), new Vector3(center.X, max.Y, center.Z)));

            // Child 5: (-X, +Y, +Z)
            node.Children[5] = AllocateNode(nodeIndex, new BoundingBox(new Vector3(min.X, center.Y, center.Z), new Vector3(center.X, max.Y, max.Z)));

            // Child 6: (+X, +Y, -Z)
            node.Children[6] = AllocateNode(nodeIndex, new BoundingBox(new Vector3(center.X, center.Y, min.Z), new Vector3(max.X, max.Y, center.Z)));

            // Child 7: (+X, +Y, +Z)
            node.Children[7] = AllocateNode(nodeIndex, new BoundingBox(center, max));

            nodes[nodeIndex] = node;

            if (node.Objects != null)
            {
                for (int i = node.Objects.Count - 1; i >= 0; i--)
                {
                    var octreeObject = node.Objects[i];

                    for (int j = 0; j < 8; j++)
                    {
                        var childIndex = node.Children[j];
                        if (InsertObject(childIndex, octreeObject.Value, octreeObject.Sphere))
                        {
                            node.Objects.RemoveAt(i);
                            break;
                        }
                    }
                }

                if (node.Objects.Count == 0)
                {
                    node.Clear();
                }
            }

            nodes[nodeIndex] = node;
        }

        private void CompactNode(int nodeIndex)
        {
            var node = nodes[nodeIndex];
            if (node.Children[0] == -1)
            {
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                var childIndex = node.Children[i];

                var childNode = nodes[childIndex];

                if (childNode.Objects != null)
                {
                    for (int j = 0; j < childNode.Objects.Count; j++)
                    {
                        var obj = childNode.Objects[j];
                        node.AddObject(obj.Value, obj.Sphere);
                        objectToNode[obj.Value] = nodeIndex;
                    }

                    childNode.Clear();
                    nodes[childIndex] = childNode;
                }

                FreeNode(childIndex);
                node.Children[i] = -1;
            }

            nodes[nodeIndex] = node;
        }

        public bool RemoveObject(T obj)
        {
            // O(1) delete time.

            if (!objectToNode.TryGetValue(obj, out var nodeIndex))
            {
                return false;
            }
            objectToNode.Remove(obj);

            var node = nodes[nodeIndex];
            node.RemoveObject(obj);
            nodes[nodeIndex] = node;

            if (node.Objects == null)
            {
                TryCompact(node.ParentIndex);
            }

            return true;
        }

        private bool TryCompact(int nodeIndex)
        {
            if (nodeIndex == -1) return false;
            var parent = nodes[nodeIndex];

            if (parent.Children[0] == -1) return false;

            const int collapseThreshold = 8;
            int totalCount = 0;
            for (int i = 0; i < 8; i++)
            {
                var siblingIndex = parent.Children[i];
                var sibling = nodes[siblingIndex];
                if (sibling.Children[0] != -1)
                {
                    totalCount = int.MaxValue;
                    break;
                }
                totalCount += sibling.ObjectsCount;
            }

            nodes[nodeIndex] = parent;

            if (totalCount <= collapseThreshold)
            {
                CompactNode(nodeIndex);
                TryCompact(parent.ParentIndex);
                return true;
            }

            return false;
        }

        public void UpdateObject(T obj, BoundingSphere sphere)
        {
            if (!objectToNode.TryGetValue(obj, out var nodeIndex))
            {
                return;
            }

            // early exit for out of world bounds.
            if (worldBounds.Contains(sphere) != ContainmentType.Contains)
            {
                RemoveObject(obj);
                return;
            }

            int startNode = nodeIndex;
            // traverse up the tree to find best matching.
            while (true)
            {
                var node = nodes[startNode];
                if (node.Bounds.Contains(sphere) == ContainmentType.Contains)
                {
                    break;
                }
                startNode = node.ParentIndex;
            }

            // if node didn't change update object and return. Best case O(1).
            if (startNode == nodeIndex)
            {
                var node = nodes[nodeIndex];
                var list = node.Objects!;
                var newState = new OctreeObject<T>(obj, sphere);
                var idx = list.IndexOf(newState);
                list[idx] = newState;
                return;
            }

            {
                var node = nodes[nodeIndex];
                node.RemoveObject(obj);
                nodes[nodeIndex] = node;

                if (node.Objects == null)
                {
                    TryCompact(node.ParentIndex);
                }
            }

            {
                // traverse down but with the optimized starting node. Worst case O(log n).
                int nodeToInsert = FindBestNodeForInsert(startNode, sphere, out int depth);

                var parent = nodes[nodeToInsert];

                parent.AddObject(obj, sphere);

                nodes[nodeToInsert] = parent;

                objectToNode[obj] = nodeToInsert;

                const int someThreshold = 32;

                if (parent.Objects!.Count > someThreshold && depth < 8)
                {
                    SplitNode(nodeToInsert);
                }
            }
        }

        public IEnumerable<T> EnumerateObjects<TUserdata>(TUserdata userdata, Stack<int> walkStack, Func<OctreeNode<T>, TUserdata, OctreeFilterResult> filter, Func<OctreeObject<T>, TUserdata, OctreeFilterResult> objectFilter)
        {
            if (count == 0)
            {
                yield break;
            }

            walkStack.Push(0);
            while (walkStack.TryPop(out var nodeIndex))
            {
                var node = nodes[nodeIndex];
                if (node.Children[0] != -1)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        var childIndex = node.Children[i];
                        var child = nodes[childIndex];
                        if (filter.Invoke(child, userdata) == OctreeFilterResult.Keep)
                        {
                            walkStack.Push(childIndex);
                        }
                    }
                }

                if (node.Objects != null)
                {
                    foreach (var obj in node.Objects)
                    {
                        if (objectFilter(obj, userdata) == OctreeFilterResult.Keep)
                        {
                            yield return obj.Value;
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < count; i++)
            {
                nodes[i].Clear();
            }
            count = 0;
            objectToNode.Clear();
            AllocateNode(-1, worldBounds); // root is always 0.
        }
    }
}