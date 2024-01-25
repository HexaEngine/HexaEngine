namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Reflection;
    using System.Collections.Generic;
    using System.Resources;

    public struct ResourceEntry : IEquatable<ResourceEntry>
    {
        public string Name;
        public IResource Resource;
        public long Size;

        public ResourceEntry(string name, IResource resource, long size)
        {
            Name = name;
            Resource = resource;
            Size = size;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ResourceEntry entry && Equals(entry);
        }

        public readonly bool Equals(ResourceEntry other)
        {
            return Name == other.Name;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(ResourceEntry left, ResourceEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ResourceEntry left, ResourceEntry right)
        {
            return !(left == right);
        }
    }

    public class GraphResourceContainer
    {
        private readonly List<ResourceRef> resources = new();
        private readonly Dictionary<string, ResourceRef> nameToResource = new();
        private readonly List<ResourceEntry> entries = new();
        private readonly object _lock = new();

        private long size;

        public IReadOnlyList<ResourceRef> Resources => resources;

        public IReadOnlyList<ResourceEntry> Entries => entries;

        public long Size => size;

        public object SyncObject => _lock;

        public void AddResource(ResourceRef resourceRef)
        {
            lock (_lock)
            {
                TryAddOrOverwriteEntry(resourceRef);
                resources.Add(resourceRef);
                nameToResource.Add(resourceRef.Name, resourceRef);
                resourceRef.ValueChanged += ResourceChanged;
            }
        }

        public void RemoveResource(ResourceRef resourceRef)
        {
            lock (_lock)
            {
                resourceRef.ValueChanged -= ResourceChanged;
                RemoveEntry(resourceRef);
                resources.Remove(resourceRef);
                nameToResource.Remove(resourceRef.Name);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                resources.Clear();
                nameToResource.Clear();
                entries.Clear();
                size = 0;
            }
        }

        private void ResourceChanged(object? sender, IDisposable? e)
        {
            if (sender is ResourceRef resourceRef)
            {
                TryAddOrOverwriteEntry(resourceRef);
            }
        }

        private bool TryAddOrOverwriteEntry(ResourceRef resourceRef)
        {
            lock (_lock)
            {
                if (FindEntry(resourceRef.Name, out var entry, out var index))
                {
                    size -= entry.Size;
                    entries.RemoveAt(index);
                }

                if (resourceRef.Value is not IResource resource)
                {
                    return false;
                }

                long resourceSize = ResourceHelper.ComputeSize(resource);

                if (resourceSize == -1)
                {
                    return false;
                }

                entry = new(resourceRef.Name, resource, resourceSize);
                entries.Add(entry);
                size += resourceSize;
            }

            return true;
        }

        private bool RemoveEntry(ResourceRef resourceRef)
        {
            lock (_lock)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry.Name == resourceRef.Name)
                    {
                        entries.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool FindEntry(string name, out ResourceEntry entry, out int index)
        {
            lock (_lock)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    entry = entries[i];
                    if (entry.Name == name)
                    {
                        index = i;
                        return true;
                    }
                }
                entry = default;
                index = -1;
                return false;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (ResourceRef resourceRef in resources)
                {
                    resourceRef.Dispose();
                }
                Clear();
            }
        }

        public void DisposeResources()
        {
            lock (_lock)
            {
                foreach (ResourceRef resourceRef in resources)
                {
                    resourceRef.Dispose();
                }
            }
        }
    }
}