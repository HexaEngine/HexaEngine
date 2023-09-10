﻿namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics.Shaders;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Thread-safe shader cache
    /// </summary>
    public static class ShaderCache
    {
        private const string file = "cache/shadercache.bin";
        private static readonly List<ShaderCacheEntry> entries = new();
        private static readonly SemaphoreSlim s = new(1);
        private const int Version = 2;

        static ShaderCache()
        {
            _ = Directory.CreateDirectory("cache");
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Load();
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            Save();
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Free();
            }
        }

        public static bool DisableCache { get; set; } = false;

        public static IReadOnlyList<ShaderCacheEntry> Entries => entries;

        public static unsafe void CacheShader(string path, uint crc32Hash, SourceLanguage language, ShaderMacro[] macros, InputElementDescription[] inputElements, Shader* shader)
        {
            lock (entries)
            {
                if (DisableCache)
                {
                    return;
                }

                var entry = new ShaderCacheEntry(path, crc32Hash, language, macros, inputElements, shader->Clone());
                entries.RemoveAll(x => x.Equals(entry));
                entries.Add(entry);
                SaveAsync();
            }
        }

        /// <summary>
        /// Returns true if succesfully found a matching shader
        /// </summary>
        /// <param name="path"></param>
        /// <param name="language"></param>
        /// <param name="macros"></param>
        /// <param name="shader"></param>
        /// <param name="inputElements"></param>
        /// <returns></returns>
        public static unsafe bool GetShader(string path, uint crc32Hash, SourceLanguage language, ShaderMacro[] macros, Shader** shader, [MaybeNullWhen(false)] out InputElementDescription[]? inputElements)
        {
            lock (entries)
            {
                *shader = default;
                inputElements = null;
                if (DisableCache)
                {
                    return false;
                }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                var ventry = new ShaderCacheEntry(path, crc32Hash, language, macros, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                var entry = entries.FirstOrDefault(x => x.Equals(ventry));
                if (entry != default && entry.Crc32Hash == crc32Hash)
                {
                    inputElements = entry.InputElements;
                    *shader = entry.Shader->Clone();
                    return true;
                }
                return false;
            }
        }

        public static void Clear()
        {
            lock (entries)
            {
                Logger.Info("Clearing shader cache ...");
                for (int i = 0; i < entries.Count; i++)
                {
                    entries[i].Free();
                }
                entries.Clear();
                Logger.Info("Clearing shader cache ... done");
            }
        }

        private static unsafe bool GetShaderInternal(string path, uint crc32Hash, SourceLanguage language, ShaderMacro[] macros, Shader** shader, [MaybeNullWhen(false)] out InputElementDescription[]? inputElements)
        {
            lock (entries)
            {
                *shader = null;
                inputElements = null;
                if (DisableCache)
                {
                    return false;
                }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                var ventry = new ShaderCacheEntry(path, crc32Hash, language, macros, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                var entry = entries.FirstOrDefault(x => x.Equals(ventry));
                if (entry != default)
                {
                    inputElements = entry.InputElements;
                    *shader = entry.Shader;
                    return true;
                }
                return false;
            }
        }

        private static void Load()
        {
            if (!File.Exists(file))
            {
                return;
            }

            lock (entries)
            {
                var span = (Span<byte>)File.ReadAllBytes(file);
                var decoder = Encoding.UTF8.GetDecoder();
                var version = BinaryPrimitives.ReadInt32LittleEndian(span);
                if (version != Version)
                    return;
                var count = BinaryPrimitives.ReadInt32LittleEndian(span[4..]);
                entries.EnsureCapacity(count);

                int idx = 8;
                for (int i = 0; i < count; i++)
                {
                    var entry = new ShaderCacheEntry();
                    idx += entry.Read(span[idx..], decoder);
                    entries.Add(entry);
                }
            }
        }

        private static void Save()
        {
            s.Wait();
            lock (entries)
            {
                var encoder = Encoding.UTF8.GetEncoder();
                var size = 8 + entries.Sum(x => x.SizeOf(encoder));
                var span = size < 2048 ? stackalloc byte[size] : new byte[size];

                BinaryPrimitives.WriteInt32LittleEndian(span[0..], Version);
                BinaryPrimitives.WriteInt32LittleEndian(span[4..], entries.Count);

                int idx = 8;
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    idx += entry.Write(span[idx..], encoder);
                }

                File.WriteAllBytes(file, span.ToArray());
            }
            s.Release();
        }

        private static Task SaveAsync()
        {
            return Task.Run(() =>
            {
                s.Wait();
                lock (entries)
                {
                    var encoder = Encoding.UTF8.GetEncoder();
                    var size = 8 + entries.Sum(x => x.SizeOf(encoder));
                    var span = size < 2048 ? stackalloc byte[size] : new byte[size];

                    BinaryPrimitives.WriteInt32LittleEndian(span[0..], Version);
                    BinaryPrimitives.WriteInt32LittleEndian(span[4..], entries.Count);

                    int idx = 8;
                    for (var i = 0; i < entries.Count; i++)
                    {
                        var entry = entries[i];
                        idx += entry.Write(span[idx..], encoder);
                    }

                    File.WriteAllBytes(file, span.ToArray());
                }
                s.Release();
            });
        }
    }

    public unsafe struct ShaderCacheEntry : IEquatable<ShaderCacheEntry>
    {
        public string Name;
        public uint Crc32Hash;
        public SourceLanguage Language;
        public ShaderMacro[] Macros;
        public InputElementDescription[] InputElements;
        public Shader* Shader;

        public ShaderCacheEntry(string name, uint crc32Hash, SourceLanguage language, ShaderMacro[] macros, InputElementDescription[] inputElements, Shader* bytecode)
        {
            Name = name;
            Crc32Hash = crc32Hash;
            Language = language;
            Macros = macros;
            InputElements = inputElements;
            Shader = bytecode;
        }

        public int Write(Span<byte> dest, Encoder encoder)
        {
            int idx = 0;
            idx += WriteString(dest[idx..], Name, encoder);
            idx += WriteUInt32(dest[idx..], Crc32Hash);
            idx += WriteInt32(dest[idx..], (int)Language);
            idx += WriteInt32(dest[idx..], Macros.Length);

            for (int i = 0; i < Macros.Length; i++)
            {
                var macro = Macros[i];
                idx += WriteString(dest[idx..], macro.Name, encoder);
                idx += WriteString(dest[idx..], macro.Definition, encoder);
            }
            idx += WriteInt32(dest[idx..], InputElements.Length);
            for (int i = 0; i < InputElements.Length; i++)
            {
                var element = InputElements[i];
                idx += WriteString(dest[idx..], element.SemanticName, encoder);
                idx += WriteInt32(dest[idx..], element.SemanticIndex);
                idx += WriteInt32(dest[idx..], (int)element.Format);
                idx += WriteInt32(dest[idx..], element.Slot);
                idx += WriteInt32(dest[idx..], element.AlignedByteOffset);
                idx += WriteInt32(dest[idx..], (int)element.Classification);
                idx += WriteInt32(dest[idx..], element.InstanceDataStepRate);
            }
            if (Shader != null)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], (int)Shader->Length);
            }
            else
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], 0);
            }

            idx += 4;

            if (Shader != null)
            {
                Shader->CopyTo(dest[idx..]);
                idx += (int)Shader->Length;
            }
            else
            {
                idx += 0;
            }

            return idx;
        }

        public int Read(ReadOnlySpan<byte> src, Decoder decoder)
        {
            int idx = 0;
            idx += ReadString(src, out Name, decoder);
            idx += ReadUInt32(src[idx..], out var crc32Hash);
            Crc32Hash = crc32Hash;
            idx += ReadInt32(src[idx..], out var lang);
            Language = (SourceLanguage)lang;

            // read macros
            int count = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            Macros = new ShaderMacro[count];
            for (var i = 0; i < count; i++)
            {
                idx += ReadString(src[idx..], out string name, decoder);
                idx += ReadString(src[idx..], out string definition, decoder);
                Macros[i] = new ShaderMacro(name, definition);
            }

            int countElements = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            InputElements = new InputElementDescription[countElements];
            for (var i = 0; i < countElements; i++)
            {
                idx += ReadString(src[idx..], out string semanticName, decoder);
                idx += ReadInt32(src[idx..], out int semanticIndex);
                idx += ReadInt32(src[idx..], out int format);
                idx += ReadInt32(src[idx..], out int slot);
                idx += ReadInt32(src[idx..], out int alignedByteOffset);
                idx += ReadInt32(src[idx..], out int classification);
                idx += ReadInt32(src[idx..], out int instanceDataStepRate);
                InputElements[i] = new(semanticName, semanticIndex, (Format)format, alignedByteOffset, slot, (InputClassification)classification, instanceDataStepRate);
            }

            int len = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;
            Shader = AllocT<Shader>();
            Shader->Bytecode = AllocT<byte>(len);
            Shader->Length = (nuint)len;
            fixed (void* ptr = src.Slice(idx, len))
            {
                Buffer.MemoryCopy(ptr, Shader->Bytecode, len, len);
            }
            idx += len;
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

        private static int WriteInt32(Span<byte> dest, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, value);
            return 4;
        }

        private static int ReadInt32(ReadOnlySpan<byte> src, out int value)
        {
            value = BinaryPrimitives.ReadInt32LittleEndian(src);
            return 4;
        }

        private static int WriteUInt32(Span<byte> dest, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(dest, value);
            return 4;
        }

        private static int ReadUInt32(ReadOnlySpan<byte> src, out uint value)
        {
            value = BinaryPrimitives.ReadUInt32LittleEndian(src);
            return 4;
        }

        private static int SizeOf(string str, Encoder encoder)
        {
            return 4 + encoder.GetByteCount(str, true);
        }

        public int SizeOf(Encoder encoder)
        {
            if (Shader != null)
            {
                return 28 +
                    SizeOf(Name, encoder) +
                    Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder)) +
                    InputElements.Sum(x => SizeOf(x.SemanticName, encoder) + 24) +
                    (int)Shader->Length;
            }
            else
            {
                return 28 +
                    SizeOf(Name, encoder) +
                    Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder)) +
                    InputElements.Sum(x => SizeOf(x.SemanticName, encoder) + 24);
            }
        }

        public void Free()
        {
            Utils.Free(Shader);
        }

        public bool Equals(ShaderCacheEntry other)
        {
            if (Name != other.Name)
            {
                return false;
            }

            if (Language != other.Language)
            {
                return false;
            }

            if (Macros == other.Macros && Macros == null && other.Macros == null)
            {
                return true;
            }

            if (Macros != other.Macros && (Macros == null || other.Macros == null))
            {
                return false;
            }

            if (Macros.Length != (other.Macros?.Length ?? 0))
            {
                return false;
            }

            for (int i = 0; i < Macros.Length; i++)
            {
#nullable disable
                if (Macros[i].Name != other.Macros[i].Name ||
                    Macros[i].Definition != other.Macros[i].Definition)
                {
                    return false;
                }
#nullable enable
            }
            return true;
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
            return HashCode.Combine(Name, Macros);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}