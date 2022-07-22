namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ShaderCache
    {
        private const string file = "cache/shadercache.bin";
        private static readonly List<ShaderCacheEntry> entries = new();
        private static readonly Dictionary<string, KeyValuePair<DateTime, byte[]>> cache = new();

        static ShaderCache()
        {
            _ = Directory.CreateDirectory("cache");
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Load();
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            Save();
        }

        public static bool DisableCache { get; set; } = false;

        public static void CacheShader(string path, ShaderMacro[] macros, Blob blob)
        {
            if (DisableCache) return;
            var datetime = File.GetLastWriteTime(path);
            var entry = new ShaderCacheEntry(path, datetime, macros, blob.AsBytes());
            entries.RemoveAll(x => x.Name == path);
            entries.Add(entry);
        }

        public static bool GetShader(string path, ShaderMacro[] macros, out Span<byte> data)
        {
            data = default;
            if (DisableCache) return false;
            var datetime = File.GetLastWriteTime(path);
            var ventry = new ShaderCacheEntry(path, datetime, macros, null);
            var entry = entries.FirstOrDefault(x => x.Equals(ventry));
            if (entry != default)
            {
                data = entry.Bytecode;
                return true;
            }
            return false;
        }

        private static void Load()
        {
            if (!File.Exists(file)) return;
            var span = (Span<byte>)File.ReadAllBytes(file);
            var decoder = Encoding.UTF8.GetDecoder();
            var count = BinaryPrimitives.ReadInt32LittleEndian(span);
            entries.EnsureCapacity(count);

            int idx = 4;
            for (int i = 0; i < count; i++)
            {
                var entry = new ShaderCacheEntry();
                entry.Read(span[idx..], decoder);
                entries.Add(entry);
            }
        }

        private static void Save()
        {
            var encoder = Encoding.UTF8.GetEncoder();
            var size = 4 + entries.Sum(x => x.SizeOf(encoder));
            var span = size < 2048 ? stackalloc byte[size] : new byte[size];

            BinaryPrimitives.WriteInt32LittleEndian(span, entries.Count);

            int idx = 4;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                idx += entry.Write(span[idx..], encoder);
            }

            File.WriteAllBytes(file, span.ToArray());
        }
    }

    public struct ShaderCacheEntry : IEquatable<ShaderCacheEntry>
    {
        public string Name;
        public DateTime Timestamp;
        public ShaderMacro[] Macros;
        public byte[] Bytecode;

        public ShaderCacheEntry(string name, DateTime timestamp, ShaderMacro[] macros, byte[] bytecode)
        {
            Name = name;
            Timestamp = timestamp;
            Macros = macros;
            Bytecode = bytecode;
        }

        public int Write(Span<byte> dest, Encoder encoder)
        {
            int idx = 0;
            idx += WriteString(dest[idx..], Name, encoder);
            BinaryPrimitives.WriteInt64LittleEndian(dest[idx..], Timestamp.ToFileTime());
            idx += 8;
            BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], Macros.Length);
            idx += 4;
            for (int i = 0; i < Macros.Length; i++)
            {
                var macro = Macros[i];
                idx += WriteString(dest[idx..], macro.Name, encoder);
                idx += WriteString(dest[idx..], macro.Definition, encoder);
            }
            BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], Bytecode.Length);
            idx += 4;
            Bytecode.CopyTo(dest[idx..]);
            idx += Bytecode.Length;
            return idx;
        }

        public int Read(ReadOnlySpan<byte> src, Decoder decoder)
        {
            int idx = 0;
            idx += ReadString(src, out Name, decoder);
            Timestamp = DateTime.FromFileTime(BinaryPrimitives.ReadInt64LittleEndian(src[idx..]));
            idx += 8;
            int count = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            Macros = new ShaderMacro[count];
            for (var i = 0; i < count; i++)
            {
                idx += ReadString(src[idx..], out string name, decoder);
                idx += ReadString(src[idx..], out string definition, decoder);
                Macros[i] = new ShaderMacro(name, definition);
            }
            int len = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            Bytecode = src.Slice(idx, len).ToArray();
            idx += Bytecode.Length;
            return idx;
        }

        private static int WriteString(Span<byte> dest, string str, Encoder encoder)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, encoder.GetByteCount(str, true));
            return encoder.GetBytes(str, dest[4..], true) + 4;
        }

        private static int ReadString(ReadOnlySpan<byte> src, out string str, Decoder decoder)
        {
            int len = BinaryPrimitives.ReadInt32LittleEndian(src);
            ReadOnlySpan<byte> bytes = src.Slice(4, len);
            int charCount = decoder.GetCharCount(bytes, true);
            Span<char> chars = charCount < 2048 ? stackalloc char[charCount] : new char[charCount];
            decoder.GetChars(bytes, chars, true);
            str = new(chars);
            return len + 4;
        }

        private static int SizeOf(string str, Encoder encoder)
        {
            return 4 + encoder.GetByteCount(str, true);
        }

        public int SizeOf(Encoder encoder)
        {
            return 16 + SizeOf(Name, encoder) + Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder)) + Bytecode.Length;
        }

        public bool Equals(ShaderCacheEntry other)
        {
            if (Name != other.Name) return false;
            if (!Timestamp.Equals(other.Timestamp)) return false;
            if (Macros == other.Macros) return true;
            return Macros.SequenceEqual(other.Macros);
        }

        public override bool Equals(object? obj)
        {
            return obj is ShaderCacheEntry entry && Equals(entry);
        }

        public static bool operator ==(ShaderCacheEntry left, ShaderCacheEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShaderCacheEntry left, ShaderCacheEntry right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Timestamp, Macros);
        }
    }
}