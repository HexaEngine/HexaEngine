namespace HexaEngine.Core.Graphics
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a mechanism for tracking memory usage associated with graphics resources.
    /// </summary>
    public static class MemoryManager
    {
        private static readonly List<MemoryEntry> entries = new();

        /// <summary>
        /// Gets a read-only list of memory entries tracking resources and their associated memory sizes.
        /// </summary>
        public static IReadOnlyList<MemoryEntry> Entries => entries;

        /// <summary>
        /// Represents an entry tracking a graphics resource and its memory usage.
        /// </summary>
        public struct MemoryEntry : IEquatable<MemoryEntry>
        {
            /// <summary>
            /// Gets the associated graphics resource.
            /// </summary>
            public IResource Resource;

            /// <summary>
            /// Gets an optional name for the entry.
            /// </summary>
            public string? Name;

            /// <summary>
            /// Gets the size of memory consumed by the resource in bytes.
            /// </summary>
            public ulong Size;

            /// <summary>
            /// Initializes a new instance of the <see cref="MemoryEntry"/> struct.
            /// </summary>
            /// <param name="resource">The associated graphics resource.</param>
            /// <param name="name">An optional name for the entry.</param>
            /// <param name="size">The size of memory consumed by the resource in bytes.</param>
            public MemoryEntry(IResource resource, string? name, ulong size)
            {
                Resource = resource;
                Name = name;
                Size = size;
            }

            public override readonly bool Equals(object? obj)
            {
                return obj is MemoryEntry entry && Equals(entry);
            }

            public readonly bool Equals(MemoryEntry other)
            {
                return Resource.NativePointer == other.Resource.NativePointer &&
                       Name == other.Name &&
                       Size == other.Size;
            }

            public override readonly int GetHashCode()
            {
                return HashCode.Combine(Resource, Name, Size);
            }

            public static bool operator ==(MemoryEntry left, MemoryEntry right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(MemoryEntry left, MemoryEntry right)
            {
                return !(left == right);
            }
        }

        /// <summary>
        /// Unregisters a graphics resource from memory tracking.
        /// </summary>
        /// <param name="resource">The graphics resource to unregister.</param>
        internal static void Unregister(IResource? resource)
        {
            if (resource == null)
            {
                return;
            }

            lock (entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].Resource == resource)
                    {
                        entries.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Registers a graphics resource in memory tracking.
        /// </summary>
        /// <param name="resource">The graphics resource to register.</param>
        internal static void Register(IResource resource)
        {
            if (string.IsNullOrEmpty(resource.DebugName))
            {
                throw new Exception("Resource's DebugName cannot be empty or null.");
            }

            if (resource is IBuffer buffer)
            {
                Register(buffer);
            }

            if (resource is ITexture1D texture1d)
            {
                Register(texture1d);
            }

            if (resource is ITexture2D texture2d)
            {
                Register(texture2d);
            }

            if (resource is ITexture3D texture3d)
            {
                Register(texture3d);
            }
        }

        /// <summary>
        /// Registers an IBuffer resource in memory tracking.
        /// </summary>
        /// <param name="buffer">The IBuffer resource to register.</param>
        internal static void Register(IBuffer buffer)
        {
            if (string.IsNullOrEmpty(buffer.DebugName))
            {
                throw new Exception("Buffer's DebugName cannot be empty or null.");
            }

            BufferDescription description = buffer.Description;
            ulong size = (ulong)description.ByteWidth;

            lock (entries)
            {
                entries.Add(new(buffer, buffer.DebugName, size));
            }
        }

        /// <summary>
        /// Registers an ITexture1D resource in memory tracking.
        /// </summary>
        /// <param name="texture">The ITexture1D resource to register.</param>
        internal static void Register(ITexture1D texture)
        {
            if (string.IsNullOrEmpty(texture.DebugName))
            {
                throw new Exception("Texture1D's DebugName cannot be empty or null.");
            }

            Texture1DDescription description = texture.Description;
            ulong stride = (ulong)(FormatHelper.BitsPerPixel(description.Format) / 8);
            ulong size = 0;
            ulong width = (ulong)description.Width;
            ulong arraySize = (ulong)description.ArraySize;
            for (int i = 0; i < description.MipLevels; i++)
            {
                size += stride * width * arraySize;
                width /= 2;
            }

            lock (entries)
            {
                entries.Add(new(texture, texture.DebugName, size));
            }
        }

        /// <summary>
        /// Registers an ITexture2D resource in memory tracking.
        /// </summary>
        /// <param name="texture">The ITexture2D resource to register.</param>
        internal static void Register(ITexture2D texture)
        {
            if (string.IsNullOrEmpty(texture.DebugName))
            {
                throw new Exception("Texture2D's DebugName cannot be empty or null.");
            }

            Texture2DDescription description = texture.Description;
            ulong stride = (ulong)(FormatHelper.BitsPerPixel(description.Format) / 8);
            ulong size = 0;
            ulong width = (ulong)description.Width;
            ulong height = (ulong)description.Height;
            ulong arraySize = (ulong)description.ArraySize;
            for (int i = 0; i < description.MipLevels; i++)
            {
                size += stride * width * height * arraySize;
                width /= 2;
                height /= 2;
            }

            lock (entries)
            {
                entries.Add(new(texture, texture.DebugName, size));
            }
        }

        /// <summary>
        /// Registers an ITexture3D resource in memory tracking.
        /// </summary>
        /// <param name="texture">The ITexture3D resource to register.</param>
        internal static void Register(ITexture3D texture)
        {
            if (string.IsNullOrEmpty(texture.DebugName))
            {
                throw new Exception("Texture3D's DebugName cannot be empty or null.");
            }

            Texture3DDescription description = texture.Description;
            ulong stride = (ulong)(FormatHelper.BitsPerPixel(description.Format) / 8);
            ulong size = 0;
            ulong width = (ulong)description.Width;
            ulong height = (ulong)description.Height;
            ulong depth = (ulong)description.Depth;
            for (int i = 0; i < description.MipLevels; i++)
            {
                size += stride * width * height * depth;
                width /= 2;
                height /= 2;
                depth /= 2;
            }

            lock (entries)
            {
                entries.Add(new(texture, texture.DebugName, size));
            }
        }
    }
}