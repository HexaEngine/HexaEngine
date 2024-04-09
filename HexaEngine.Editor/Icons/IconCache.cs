namespace HexaEngine.Editor.Icons
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public unsafe class IconCache : IDisposable
    {
        private readonly string file;
        private readonly List<IconGlyphTileInfo> entries = [];
        private int atlasWidth;
        private int atlasHeight;
        private Format atlasFormat;
        private byte* atlasData;
        private int atlasDataSize;

        public IconCache(string file)
        {
            this.file = file;
            Load();
        }

        public bool IsValid(IList<Guid> infos)
        {
            for (int i = 0; i < infos.Count; i++)
            {
                bool found = false;
                var info = infos[i];
                for (int j = 0; j < entries.Count; j++)
                {
                    var entry = entries[j];
                    if (entry.Key == info)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        public void Set(IEnumerable<IconGlyphTileInfo> infos, int width, int height, Format format, byte* data, int size)
        {
            entries.Clear();
            entries.AddRange(infos);
            atlasWidth = width;
            atlasHeight = height;
            atlasFormat = format;
            atlasData = (byte*)ReAlloc(atlasData, size);
            atlasDataSize = size;
            new Span<byte>(data, size).CopyTo(new Span<byte>(atlasData, atlasDataSize));
            Save();
        }

        public void Get(out int width, out int height, out Format format, out byte* data, out int rowPitch)
        {
            width = atlasWidth;
            height = atlasHeight;
            format = atlasFormat;
            data = atlasData;
            rowPitch = atlasWidth * (int)MathF.Ceiling(FormatHelper.BitsPerPixel(atlasFormat) / 8);
        }

        public bool TryGet(IList<Guid> infos, out int width, out int height, out Format format, out byte* data, out int rowPitch, [MaybeNullWhen(false)] out IconGlyphTileInfo[] iconGlyphs)
        {
            if (IsValid(infos))
            {
                width = atlasWidth;
                height = atlasHeight;
                format = atlasFormat;
                data = atlasData;
                rowPitch = atlasWidth * (int)MathF.Ceiling(FormatHelper.BitsPerPixel(atlasFormat) / 8);
                iconGlyphs = [.. entries];
                return true;
            }

            width = 0;
            height = 0;
            format = 0;
            data = null;
            rowPitch = 0;
            iconGlyphs = null;
            return false;
        }

        private const int Version = 1;
        private const int HeaderSize = 8 + 12;

        private void Load()
        {
            if (!File.Exists(file))
            {
                return;
            }

            var span = (Span<byte>)File.ReadAllBytes(file);
            if (span.Length < HeaderSize)
            {
                return;
            }

            var version = BinaryPrimitives.ReadInt32LittleEndian(span);

            if (version != Version)
            {
                return;
            }

            var count = BinaryPrimitives.ReadInt32LittleEndian(span[4..]);
            entries.EnsureCapacity(count);

            atlasWidth = BinaryPrimitives.ReadInt32LittleEndian(span[8..]);
            atlasHeight = BinaryPrimitives.ReadInt32LittleEndian(span[12..]);
            atlasFormat = (Format)BinaryPrimitives.ReadInt32LittleEndian(span[16..]);

            atlasDataSize = atlasWidth * atlasHeight * (int)MathF.Ceiling(FormatHelper.BitsPerPixel(atlasFormat) / 8);

            int idx = HeaderSize;
            for (var i = 0; i < count; i++)
            {
                entries.Add(IconGlyphTileInfo.Read(span[idx..]));
                idx += IconGlyphTileInfo.SizeInBytes;
            }

            atlasData = (byte*)Alloc(atlasDataSize);
            span.Slice(idx, atlasDataSize).CopyTo(new Span<byte>(atlasData, atlasDataSize));
        }

        public void Save()
        {
            FileStream? fs = null;
            try
            {
                fs = File.Create(file);

                Span<byte> header = stackalloc byte[HeaderSize];

                BinaryPrimitives.WriteInt32LittleEndian(header, Version);
                BinaryPrimitives.WriteInt32LittleEndian(header[4..], entries.Count);
                BinaryPrimitives.WriteInt32LittleEndian(header[8..], atlasWidth);
                BinaryPrimitives.WriteInt32LittleEndian(header[12..], atlasHeight);
                BinaryPrimitives.WriteInt32LittleEndian(header[16..], (int)atlasFormat);

                fs.Write(header);

                Span<byte> entryBuffer = stackalloc byte[IconGlyphTileInfo.SizeInBytes];
                for (int i = 0; i < entries.Count; i++)
                {
                    entries[i].Write(entryBuffer);
                    fs.Write(entryBuffer);
                }

                fs.Write(new Span<byte>(atlasData, atlasDataSize));

                fs.Close();
            }
            finally
            {
                // ensures that file handle is released even if an exception was thrown.
                fs?.Close();
            }
        }

        public void Clear()
        {
            if (atlasData != null)
            {
                Free(atlasData);
                atlasData = null;
            }

            atlasDataSize = 0;
            atlasFormat = 0;
            atlasHeight = 0;
            atlasWidth = 0;
            entries.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            Clear();
        }

        ~IconCache()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}