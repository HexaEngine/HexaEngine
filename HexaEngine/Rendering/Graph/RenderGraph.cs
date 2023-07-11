namespace HexaEngine.Rendering.Graph
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public class RenderGraph
    {
        private readonly WriteDependencyRegistry mGlobalWriteDependencyRegistry = new();
        private readonly List<Node> passNodes = new();
        private readonly string name;

        public RenderGraph(string name)
        {
            this.name = name;
        }

        public string Name => name;

        public IReadOnlyList<Node> NodesInGlobalExecutionOrder => mNodesInGlobalExecutionOrder;

        public IReadOnlyList<Node> Nodes => passNodes;

        public IReadOnlyList<DependencyLevel> DependencyLevels => mDependencyLevels;

        public ulong DetectedQueueCount => mDetectedQueueCount;

        public List<Node> NodesForQueue(ulong queueIndex)
        {
            return mNodesPerQueue[(int)queueIndex];
        }

        public Node FirstNodeThatUsesRayTracingOnQueue(ulong queueIndex)
        {
            return mFirstNodesThatUseRayTracing[(int)queueIndex];
        }

        public ulong NodeCountForQueue(ulong queueIndex)
        {
            if (mQueueNodeCounters.TryGetValue(queueIndex, out var counter))
            {
                return counter;
            }

            return 0;
        }

        public ResourceUsageTimeline GetResourceUsageTimeline(Name resourceName)
        {
            if (mResourceUsageTimelines.TryGetValue(resourceName, out var resourceUsageTimeline))
            {
                return resourceUsageTimeline;
            }

            Trace.Assert(false, $"Resource timeline ({resourceName}) doesn't exist");
            return default;
        }

        public Node GetNodeThatWritesToSubresource(SubresourceName subresourceName)
        {
            if (mWrittenSubresourceToPassMap.TryGetValue(subresourceName, out Node? node))
            {
                return node;
            }

            Trace.Assert(false, $"Subresource {SubresourceName.DecodeSubresourceName(subresourceName).Item1} is not registered for writing in the graph.");
            return default;
        }

        public ulong AddPass(RenderPassMetadata metadata)
        {
            EnsureRenderPassUniqueness(metadata.Name);
            Node node = new(metadata, mGlobalWriteDependencyRegistry);
            passNodes.Add(node);
            node.IndexInUnorderedList = (ulong)(passNodes.Count - 1);
            return (ulong)(passNodes.Count - 1);
        }

        public void Build()
        {
            BuildAdjacencyLists();
            TopologicalSort();
            BuildDependencyLevels();
            FinalizeDependencyLevels();
            CullRedundantSynchronizations();
        }

        public void Clear()
        {
            mGlobalWriteDependencyRegistry.Clear();
            mDependencyLevels.Clear();
            mResourceUsageTimelines.Clear();
            mQueueNodeCounters.Clear();
            mTopologicallySortedNodes.Clear();
            mNodesInGlobalExecutionOrder.Clear();
            mAdjacencyLists.Clear();
            mDetectedQueueCount = 1;
            mNodesPerQueue.Clear();
            mFirstNodesThatUseRayTracing.Clear();

            for (int i = 0; i < passNodes.Count; i++)
            {
                passNodes[i].Clear();
            }
        }

        private void EnsureRenderPassUniqueness(Name passName)
        {
            Trace.Assert(!passNodes.Any(x => x.Metadata.Name == passName), $"Render pass {passName} is already added to the graph.");
        }

        private readonly List<List<int>> mAdjacencyLists = new();

        private void BuildAdjacencyLists()
        {
            mAdjacencyLists.Capacity = passNodes.Count;
            mAdjacencyLists.Clear();
            for (int i = 0; i < passNodes.Count; i++)
            {
                mAdjacencyLists.Add(new List<int>());
            }

            for (int nodeIdx = 0; nodeIdx < passNodes.Count; ++nodeIdx)
            {
                Node node = passNodes[nodeIdx];

                if (!node.HasAnyDependencies())
                {
                    continue;
                }

                List<int> adjacentNodeIndices = mAdjacencyLists[nodeIdx];

                for (int otherNodeIdx = 0; otherNodeIdx < passNodes.Count; ++otherNodeIdx)
                {
                    // Do not check dependencies on itself
                    if (nodeIdx == otherNodeIdx) continue;

                    Node otherNode = passNodes[otherNodeIdx];

                    bool EstablishAdjacency(SubresourceName otherNodeReadResource)
                    {
                        // If other node reads a subresource written by the current node, then it depends on current node and is an adjacent dependency
                        bool otherNodeDependsOnCurrentNode = node.WrittenSubresources.Contains(otherNodeReadResource);

                        if (otherNodeDependsOnCurrentNode)
                        {
                            adjacentNodeIndices.Add(otherNodeIdx);

                            if (node.ExecutionQueueIndex != otherNode.ExecutionQueueIndex)
                            {
                                node.SyncSignalRequired = true;
                                otherNode.NodesToSyncWith.Add(node);
                            }

                            return true;
                        }

                        return false;
                    };

                    for (int i = 0; i < otherNode.ReadSubresources.Count; i++)
                    {
                        SubresourceName otherNodeReadResource = otherNode.ReadSubresources[i];
                        if (EstablishAdjacency(otherNodeReadResource)) break;
                    }

                    for (int i = 0; i < otherNode.AliasedSubresources.Count; i++)
                    {
                        SubresourceName otherNodeReadResource = otherNode.AliasedSubresources[i];
                        if (EstablishAdjacency(otherNodeReadResource)) break;
                    }
                }
            }
        }

        private readonly List<Node> mTopologicallySortedNodes = new();

        private void DepthFirstSearch(int nodeIndex, List<bool> visited, List<bool> onStack, ref bool isCyclic)
        {
            if (isCyclic)
                return;

            visited[nodeIndex] = true;
            onStack[nodeIndex] = true;

            ulong adjacencyListIndex = passNodes[nodeIndex].IndexInUnorderedList;

            var list = mAdjacencyLists[(int)adjacencyListIndex];
            for (int i = 0; i < list.Count; i++)
            {
                int neighbour = list[i];
                if (visited[neighbour] && onStack[neighbour])
                {
                    isCyclic = true;
                    return;
                }

                if (!visited[neighbour])
                {
                    DepthFirstSearch(neighbour, visited, onStack, ref isCyclic);
                }
            }

            onStack[nodeIndex] = false;
            mTopologicallySortedNodes.Add(passNodes[nodeIndex]);
        }

        private void TopologicalSort()
        {
            List<bool> visitedNodes = new(new bool[passNodes.Count]);
            List<bool> onStackNodes = new(new bool[passNodes.Count]);

            bool isCyclic = false;

            for (int nodeIndex = 0; nodeIndex < passNodes.Count; ++nodeIndex)
            {
                Node node = passNodes[nodeIndex];

                // Visited nodes and nodes without outputs are not processed
                if (!visitedNodes[nodeIndex] && node.HasAnyDependencies())
                {
                    DepthFirstSearch(nodeIndex, visitedNodes, onStackNodes, ref isCyclic);
                    Trace.Assert(!isCyclic, $"Detected cyclic dependency in pass: {node.Metadata.Name}");
                }
            }

            mTopologicallySortedNodes.Reverse();
        }

        private readonly List<DependencyLevel> mDependencyLevels = new();
        private ulong mDetectedQueueCount;

        private void BuildDependencyLevels()
        {
            List<long> longestDistances = new(passNodes.Count);

            ulong dependencyLevelCount = 1;

            // Perform longest node distance search
            for (int nodeIndex = 0; nodeIndex < mTopologicallySortedNodes.Count; ++nodeIndex)
            {
                ulong originalIndex = mTopologicallySortedNodes[nodeIndex].IndexInUnorderedList;
                ulong adjacencyListIndex = originalIndex;
                var list = mAdjacencyLists[(int)adjacencyListIndex];
                for (int i = 0; i < list.Count; i++)
                {
                    int adjacentNodeIndex = list[i];
                    if (longestDistances[adjacentNodeIndex] < longestDistances[(int)originalIndex] + 1)
                    {
                        long newLongestDistance = longestDistances[(int)originalIndex] + 1;
                        longestDistances[adjacentNodeIndex] = newLongestDistance;
                        dependencyLevelCount = Math.Max((ulong)(newLongestDistance + 1), dependencyLevelCount);
                    }
                }
            }

            mDependencyLevels.Clear();
            mDependencyLevels.Capacity = (int)dependencyLevelCount;
            for (ulong i = 0; i < dependencyLevelCount; i++)
            {
                mDependencyLevels.Add(new());
            }
            mDetectedQueueCount = 1;

            // Dispatch nodes to corresponding dependency levels.
            for (int nodeIndex = 0; nodeIndex < mTopologicallySortedNodes.Count; ++nodeIndex)
            {
                Node node = mTopologicallySortedNodes[nodeIndex];
                ulong levelIndex = (ulong)longestDistances[(int)node.IndexInUnorderedList];
                DependencyLevel dependencyLevel = mDependencyLevels[(int)levelIndex];
                dependencyLevel.LevelIndex = levelIndex;
                dependencyLevel.AddNode(node);
                node.DependencyLevelIndex = levelIndex;
                mDetectedQueueCount = Math.Max(mDetectedQueueCount, node.ExecutionQueueIndex + 1);
            }
        }

        private List<Node> mNodesInGlobalExecutionOrder = new();
        private List<List<Node>> mNodesPerQueue = new();
        private List<Node> mFirstNodesThatUseRayTracing = new();
        private Dictionary<ulong, ulong> mQueueNodeCounters = new();
        private Dictionary<SubresourceName, Node> mWrittenSubresourceToPassMap = new();

        private Dictionary<Name, ResourceUsageTimeline> mResourceUsageTimelines = new();

        private void FinalizeDependencyLevels()
        {
            ulong globalExecutionIndex = 0;

            mNodesInGlobalExecutionOrder = new(new Node[mTopologicallySortedNodes.Count]);
            mNodesPerQueue = new((int)mDetectedQueueCount);
            for (ulong i = 0; i < mDetectedQueueCount; i++)
            {
                mNodesPerQueue.Add(new());
            }
            mFirstNodesThatUseRayTracing = new(new Node[mDetectedQueueCount]);
            List<Node> perQueuePreviousNodes = new(new Node[mDetectedQueueCount]);

            for (int i1 = 0; i1 < mDependencyLevels.Count; i1++)
            {
                DependencyLevel dependencyLevel = mDependencyLevels[i1];
                ulong localExecutionIndex = 0;

                Dictionary<SubresourceName, HashSet<ulong>> resourceReadingQueueTracker = new();
                dependencyLevel.NodesPerQueue.Capacity = (int)mDetectedQueueCount;
                for (ulong i = 0; i < mDetectedQueueCount; i++)
                {
                    dependencyLevel.NodesPerQueue.Add(new());
                }

                for (int i = 0; i < dependencyLevel.Nodes.Count; i++)
                {
                    Node node = dependencyLevel.Nodes[i];
                    // Track which resource is read by which queue in this dependency level
                    for (int i2 = 0; i2 < node.ReadSubresources.Count; i2++)
                    {
                        SubresourceName subresourceName = node.ReadSubresources[i2];
                        resourceReadingQueueTracker[subresourceName].Add(node.ExecutionQueueIndex);
                    }

                    // Associate written subresource with render pass that writes to it for quick access when needed
                    for (int i2 = 0; i2 < node.WrittenSubresources.Count; i2++)
                    {
                        SubresourceName subresourceName = node.WrittenSubresources[i2];
                        mWrittenSubresourceToPassMap[subresourceName] = node;
                    }

                    node.GlobalExecutionIndex = globalExecutionIndex;
                    node.LocalToDependencyLevelExecutionIndex = localExecutionIndex;
                    node.LocalToQueueExecutionIndex = mQueueNodeCounters[node.ExecutionQueueIndex]++;

                    mNodesInGlobalExecutionOrder[(int)globalExecutionIndex] = node;

                    dependencyLevel.NodesPerQueue[(int)node.ExecutionQueueIndex].Add(node);
                    mNodesPerQueue[(int)node.ExecutionQueueIndex].Add(node);

                    // Add previous node on that queue as a dependency for sync optimization later
                    if (perQueuePreviousNodes[(int)node.ExecutionQueueIndex] != null)
                    {
                        node.NodesToSyncWith.Add(perQueuePreviousNodes[(int)node.ExecutionQueueIndex]);
                    }

                    perQueuePreviousNodes[(int)node.ExecutionQueueIndex] = node;

                    for (int i2 = 0; i2 < node.AllResources.Count; i2++)
                    {
                        Name resourceName = node.AllResources[i2];
                        bool timelineExists = mResourceUsageTimelines.ContainsKey(resourceName);

                        if (timelineExists)
                        {
                            // Update "end"
                            var value = mResourceUsageTimelines[resourceName];
                            value.Second = node.GlobalExecutionIndex;
                            mResourceUsageTimelines[resourceName] = value;
                        }
                        else
                        {
                            // Create "start"
                            var timeline = mResourceUsageTimelines[resourceName];
                            timeline.First = node.GlobalExecutionIndex;
                            timeline.Second = node.GlobalExecutionIndex;
                            mResourceUsageTimelines[resourceName] = timeline;
                        }
                    }

                    // Track first RT-using node to sync BVH builds with
                    if (node.UsesRayTracing && mFirstNodesThatUseRayTracing[(int)node.ExecutionQueueIndex] == null)
                    {
                        mFirstNodesThatUseRayTracing[(int)node.ExecutionQueueIndex] = node;
                    }

                    localExecutionIndex++;
                    globalExecutionIndex++;
                }

                // Record queue indices that are detected to read common resources
                foreach (var (subresourceName, queueIndices) in resourceReadingQueueTracker)
                {
                    // If resource is read by more than one queue
                    if (queueIndices.Count > 1)
                    {
                        foreach (ulong queueIndex in queueIndices)
                        {
                            dependencyLevel.QueuesInvoledInCrossQueueResourceReads.Add(queueIndex);
                            dependencyLevel.SubresourcesReadByMultipleQueues.Add(subresourceName);
                        }
                    }
                }
            }
        }

        private void CullRedundantSynchronizations()
        {
            ulong[] defaults = new ulong[mDetectedQueueCount];
            Array.Fill(defaults, Node.InvalidSynchronizationIndex);
            // Initialize synchronization index sets
            foreach (Node node in passNodes)
            {
                node.SynchronizationIndexSet.AddRange(defaults);
            }

            foreach (DependencyLevel dependencyLevel in mDependencyLevels)
            {
                // First pass: find closest nodes to sync with, compute initial SSIS (sufficient synchronization index set)
                foreach (Node node in dependencyLevel.Nodes)
                {
                    // Closest node to sync with on each queue
                    List<Node> closestNodesToSyncWith = new(new Node[mDetectedQueueCount]);

                    // Find closest dependencies from other queues for the current node
                    foreach (Node dependencyNode in node.NodesToSyncWith)
                    {
                        Node closestNode = closestNodesToSyncWith[(int)dependencyNode.ExecutionQueueIndex];

                        if (closestNode == null || dependencyNode.LocalToQueueExecutionIndex > closestNode.LocalToQueueExecutionIndex)
                        {
                            closestNodesToSyncWith[(int)dependencyNode.ExecutionQueueIndex] = dependencyNode;
                        }
                    }

                    // Get rid of nodes to sync that may have had redundancies
                    node.NodesToSyncWith.Clear();

                    // Compute initial SSIS
                    for (ulong queueIdx = 0; queueIdx < mDetectedQueueCount; ++queueIdx)
                    {
                        Node closestNode = closestNodesToSyncWith[(int)queueIdx];

                        if (closestNode == null)
                        {
                            // If we do not have a closest node to sync with on another queue (queueIdx),
                            // we need to use SSIS value for that queue from the previous node on this node's queue (closestNodesToSyncWith[node.ExecutionQueueIndex])
                            // to correctly propagate SSIS values for all queues through the graph and do not lose them
                            Node previousNodeOnNodesQueue = closestNodesToSyncWith[(int)node.ExecutionQueueIndex];

                            // Previous node can be null if we're dealing with first node in the queue
                            if (previousNodeOnNodesQueue != null)
                            {
                                ulong syncIndexForOtherQueueFromPreviousNode = previousNodeOnNodesQueue.SynchronizationIndexSet[(int)queueIdx];
                                node.SynchronizationIndexSet[(int)queueIdx] = syncIndexForOtherQueueFromPreviousNode;
                            }
                        }
                        else
                        {
                            // Update SSIS using closest nodes' indices
                            if (closestNode.ExecutionQueueIndex != node.ExecutionQueueIndex)
                                node.SynchronizationIndexSet[(int)closestNode.ExecutionQueueIndex] = closestNode.LocalToQueueExecutionIndex;

                            // Store only closest nodes to sync with
                            node.NodesToSyncWith.Add(closestNode);
                        }
                    }

                    // Use node's execution index as synchronization index on its own queue
                    node.SynchronizationIndexSet[(int)node.ExecutionQueueIndex] = node.LocalToQueueExecutionIndex;
                }

                // Second pass: cull redundant dependencies by searching for indirect synchronizations
                foreach (Node node in dependencyLevel.Nodes)
                {
                    // Keep track of queues we still need to sync with
                    HashSet<ulong> queueToSyncWithIndices = new();

                    // Store nodes and queue syncs they cover
                    List<SyncCoverage> syncCoverageArray = new();

                    // Final optimized list of nodes without redundant dependencies
                    List<Node> optimalNodesToSyncWith = new();

                    foreach (Node nodeToSyncWith in node.NodesToSyncWith)
                    {
                        queueToSyncWithIndices.Add(nodeToSyncWith.ExecutionQueueIndex);
                    }

                    while (queueToSyncWithIndices.Count > 0)
                    {
                        ulong maxNumberOfSyncsCoveredBySingleNode = 0;

                        for (var dependencyNodeIdx = 0u; dependencyNodeIdx < node.NodesToSyncWith.Count; ++dependencyNodeIdx)
                        {
                            Node dependencyNode = node.NodesToSyncWith[(int)dependencyNodeIdx];

                            // Take a dependency node and check how many queues we would sync with
                            // if we would only sync with this one node. We very well may encounter a case
                            // where by synchronizing with just one node we will sync with more then one queue
                            // or even all of them through indirect synchronizations,
                            // which will make other synchronizations previously detected for this node redundant.

                            List<ulong> syncedQueueIndices = new();

                            foreach (ulong queueIndex in queueToSyncWithIndices)
                            {
                                ulong currentNodeDesiredSyncIndex = node.SynchronizationIndexSet[(int)queueIndex];
                                ulong dependencyNodeSyncIndex = dependencyNode.SynchronizationIndexSet[(int)queueIndex];

                                Trace.Assert(currentNodeDesiredSyncIndex != Node.InvalidSynchronizationIndex, "Bug! Node that wants to sync with some queue must have a valid sync index for that queue.");

                                if (queueIndex == node.ExecutionQueueIndex)
                                {
                                    currentNodeDesiredSyncIndex -= 1;
                                }

                                if (dependencyNodeSyncIndex != Node.InvalidSynchronizationIndex &&
                                    dependencyNodeSyncIndex >= currentNodeDesiredSyncIndex)
                                {
                                    syncedQueueIndices.Add(queueIndex);
                                }
                            }

                            syncCoverageArray.Add(new(dependencyNode, dependencyNodeIdx, syncedQueueIndices));
                            maxNumberOfSyncsCoveredBySingleNode = Math.Max(maxNumberOfSyncsCoveredBySingleNode, (ulong)syncedQueueIndices.Count);
                        }

                        foreach (SyncCoverage syncCoverage in syncCoverageArray)
                        {
                            var coveredSyncCount = syncCoverage.SyncedQueueIndices.Count;

                            if ((ulong)coveredSyncCount >= maxNumberOfSyncsCoveredBySingleNode)
                            {
                                // Optimal list of synchronizations should not contain nodes from the same queue,
                                // because work on the same queue is synchronized automatically and implicitly
                                if (syncCoverage.NodeToSyncWith.ExecutionQueueIndex != node.ExecutionQueueIndex)
                                {
                                    optimalNodesToSyncWith.Add(syncCoverage.NodeToSyncWith);

                                    // Update SSIS
                                    var index = node.SynchronizationIndexSet[(int)syncCoverage.NodeToSyncWith.ExecutionQueueIndex];
                                    index = Math.Max(index, node.SynchronizationIndexSet[(int)syncCoverage.NodeToSyncWith.ExecutionQueueIndex]);
                                    node.SynchronizationIndexSet[(int)syncCoverage.NodeToSyncWith.ExecutionQueueIndex] = index;
                                }

                                // Remove covered queues from the list of queues we need to sync with
                                foreach (ulong syncedQueueIndex in syncCoverage.SyncedQueueIndices)
                                {
                                    queueToSyncWithIndices.Remove(syncedQueueIndex);
                                }
                            }
                        }

                        // Remove nodes that we synced with from the original list. Reverse iterating to avoid index invalidation.
                        for (var syncCoverageIt = syncCoverageArray.Count; syncCoverageIt != 0; --syncCoverageIt)
                        {
                            node.NodesToSyncWith.RemoveAt((int)syncCoverageArray[syncCoverageIt].NodeToSyncWithIndex);
                        }
                    }

                    // Finally, assign an optimal list of nodes to sync with to the current node
                    node.NodesToSyncWith = optimalNodesToSyncWith;
                }
            }
        }
    }
}