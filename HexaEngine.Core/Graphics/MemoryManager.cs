namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class MemoryManager
    {
        private static IGraphicsDevice device;
        private static readonly List<TextureEntry> entries = new();
        private static ulong usedMemory;
        private static ulong freeMemory;
        private static ulong maxMemory;

        public static IReadOnlyList<TextureEntry> Entries => entries;

        public struct TextureEntry
        {
            public IResource Resource;
            public string? Name;
            public ulong Size;

            public TextureEntry(IResource resource, string? name, ulong size)
            {
                Resource = resource;
                Name = name;
                Size = size;
            }
        }

        internal static void Unregister(IResource? resource)
        {
            if (resource == null) return;
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

        internal static void Register(IResource resource)
        {
            if (resource is IBuffer buffer)
                Register(buffer);
            if (resource is ITexture1D texture1d)
                Register(texture1d);
            if (resource is ITexture2D texture2d)
                Register(texture2d);
            if (resource is ITexture3D texture3d)
                Register(texture3d);
        }

        internal static void Register(IBuffer buffer)
        {
            BufferDescription description = buffer.Description;
            ulong size = (ulong)description.ByteWidth;
            lock (entries)
                entries.Add(new(buffer, buffer.DebugName, size));
        }

        internal static void Register(ITexture1D texture)
        {
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
                entries.Add(new(texture, texture.DebugName, size));
        }

        internal static void Register(ITexture2D texture)
        {
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
                entries.Add(new(texture, texture.DebugName, size));
        }

        internal static void Register(ITexture3D texture)
        {
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
                entries.Add(new(texture, texture.DebugName, size));
        }
    }
}