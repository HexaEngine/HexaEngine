namespace HexaEngine.Rendering.Graph
{
    using System;
    using System.Diagnostics;

    public class Node
    {
        private readonly WriteDependencyRegistry writeDependencyRegistry;
        private readonly RenderPassMetadata metadata;

        public const ulong InvalidSynchronizationIndex = ulong.MaxValue;

        public Node(RenderPassMetadata metadata, WriteDependencyRegistry writeDependencyRegistry)
        {
            this.metadata = metadata;
            this.writeDependencyRegistry = writeDependencyRegistry;
        }

        public RenderPassMetadata Metadata => metadata;

        public ulong ExecutionQueueIndex { get; internal set; } = 0;

        public bool UsesRayTracing { get; internal set; } = false;

        public ulong GlobalExecutionIndex { get; internal set; } = 0;

        public ulong DependencyLevelIndex { get; internal set; } = 0;

        public ulong LocalToDependencyLevelExecutionIndex { get; internal set; } = 0;

        public ulong LocalToQueueExecutionIndex { get; internal set; } = 0;

        public ulong IndexInUnorderedList { get; internal set; } = 0;

        public List<SubresourceName> ReadSubresources { get; } = new();

        public List<SubresourceName> WrittenSubresources { get; } = new();

        public List<SubresourceName> ReadAndWrittenSubresources { get; } = new();

        public List<SubresourceName> AliasedSubresources { get; } = new();

        public List<Name> AllResources { get; } = new();

        public List<ulong> SynchronizationIndexSet { get; } = new();

        public List<Node> NodesToSyncWith { get; internal set; } = new();

        public bool SyncSignalRequired { get; internal set; } = false;

        public void AddReadDependency(Name resourceName, uint firstSubresourceIndex, uint lastSubresourceIndex)
        {
            for (uint i = firstSubresourceIndex; i <= lastSubresourceIndex; ++i)
            {
                SubresourceName name = SubresourceName.ConstructSubresourceName(resourceName, i);
                ReadSubresources.Add(name);
                ReadAndWrittenSubresources.Add(name);
                AllResources.Add(resourceName);
            }
        }

        public void AddReadDependency(Name resourceName, List<uint> subresources)
        {
            if (subresources.Count == 0)
            {
                AddReadDependency(resourceName, 1);
            }
            else
            {
                for (int i = 0; i < subresources.Count; i++)
                {
                    uint subresourceIndex = subresources[i];
                    SubresourceName name = SubresourceName.ConstructSubresourceName(resourceName, subresourceIndex);
                    ReadSubresources.Add(name);
                    ReadAndWrittenSubresources.Add(name);
                    AllResources.Add(resourceName);
                }
            }
        }

        public void AddReadDependency(Name resourceName, uint subresourceCount)
        {
            Trace.Assert(subresourceCount > 0, "0 subresource count");
            AddReadDependency(resourceName, 0, subresourceCount - 1);
        }

        public void AddWriteDependency(Name resourceName, Name? originalResourceName, uint firstSubresourceIndex, uint lastSubresourceIndex)
        {
            for (uint i = firstSubresourceIndex; i <= lastSubresourceIndex; ++i)
            {
                SubresourceName name = SubresourceName.ConstructSubresourceName(resourceName, i);
                EnsureSingleWriteDependency(name);
                WrittenSubresources.Add(name);
                ReadAndWrittenSubresources.Add(name);
                AllResources.Add(resourceName);

                if (originalResourceName.HasValue)
                {
                    SubresourceName originalSubresoruce = SubresourceName.ConstructSubresourceName(originalResourceName.Value, i);
                    AliasedSubresources.Add(originalSubresoruce);
                    AllResources.Add(originalResourceName.Value);
                }
            }
        }

        public void AddWriteDependency(Name resourceName, Name? originalResourceName, List<uint> subresources)
        {
            if (subresources.Count == 0)
            {
                AddWriteDependency(resourceName, originalResourceName, 1);
            }
            else
            {
                for (int i = 0; i < subresources.Count; i++)
                {
                    uint subresourceIndex = subresources[i];
                    SubresourceName name = SubresourceName.ConstructSubresourceName(resourceName, subresourceIndex);
                    EnsureSingleWriteDependency(name);
                    WrittenSubresources.Add(name);
                    ReadAndWrittenSubresources.Add(name);
                    AllResources.Add(resourceName);
                }
            }
        }

        public void AddWriteDependency(Name resourceName, Name? originalResourceName, uint subresourceCount)
        {
            Trace.Assert(subresourceCount > 0, "0 subresource count");
            AddWriteDependency(resourceName, originalResourceName, 0, subresourceCount - 1);
        }

        public bool HasDependency(Name resourceName, uint subresourceIndex)
        {
            return HasDependency(SubresourceName.ConstructSubresourceName(resourceName, subresourceIndex));
        }

        public bool HasDependency(SubresourceName subresourceName)
        {
            return ReadAndWrittenSubresources.Contains(subresourceName);
        }

        public bool HasAnyDependencies()
        {
            return ReadAndWrittenSubresources.Count != 0;
        }

        private void EnsureSingleWriteDependency(SubresourceName name)
        {
            (var resourceName, var subresourceIndex) = SubresourceName.DecodeSubresourceName(name);

            var it = writeDependencyRegistry.Contains(name);

            Trace.Assert(!it, $"Resource {resourceName}, subresource {subresourceIndex} already has a write dependency in {writeDependencyRegistry[name]}. Use Aliases to perform multiple writes into the same resource in {metadata.Name} pass.");

            writeDependencyRegistry.Add(name, metadata.Name);
        }

        internal void Clear()
        {
            ReadSubresources.Clear();
            WrittenSubresources.Clear();
            ReadAndWrittenSubresources.Clear();
            AllResources.Clear();
            AliasedSubresources.Clear();
            NodesToSyncWith.Clear();
            SynchronizationIndexSet.Clear();
            DependencyLevelIndex = 0;
            SyncSignalRequired = false;
            ExecutionQueueIndex = 0;
            UsesRayTracing = false;
            GlobalExecutionIndex = 0;
            LocalToDependencyLevelExecutionIndex = 0;
        }

        public static bool operator ==(Node a, Node b)
        {
            return a.metadata.Name == b.metadata.Name;
        }

        public static bool operator !=(Node a, Node b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Node other)
            {
                return false;
            }
            return this == other;
        }

        public override int GetHashCode()
        {
            return metadata.Name.GetHashCode();
        }
    }
}