namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Debugging;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    /// <summary>
    /// Thread-safe shader cache
    /// </summary>
    public static class ShaderCache
    {
        private const string file = "cache/shadercache.bin";
        private static readonly List<ShaderCacheEntry> entries = new();

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

        public static unsafe void CacheShader(string path, ShaderMacro[] macros, Shader* shader)
        {
            lock (entries)
            {
                if (DisableCache) return;
                var entry = new ShaderCacheEntry(path, macros, shader);
                entries.RemoveAll(x => x.Equals(entry));
                entries.Add(entry);
            }
        }

        public static unsafe void GetShaderOrCompile(IGraphicsDevice device, string code, string entry, string path, string profile, ShaderMacro[] macros, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !GetShader(path, macros, &pShader))
            {
                device.Compile(code, entry, path, profile, &pShader);
                CacheShader(entry, macros, pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompileFile(IGraphicsDevice device, string entry, string path, string profile, ShaderMacro[] macros, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !GetShader(path, macros, &pShader))
            {
                device.CompileFromFile(path, entry, profile, &pShader);
                CacheShader(entry, macros, pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompile(IGraphicsDevice device, string code, string entry, string path, string profile, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !GetShader(path, Array.Empty<ShaderMacro>(), &pShader))
            {
                device.Compile(code, entry, path, profile, &pShader);
                CacheShader(entry, Array.Empty<ShaderMacro>(), pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompileFile(IGraphicsDevice device, string entry, string path, string profile, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !GetShader(path, Array.Empty<ShaderMacro>(), &pShader))
            {
                device.CompileFromFile(path, entry, profile, &pShader);
                CacheShader(entry, Array.Empty<ShaderMacro>(), pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompile(IGraphicsDevice device, string code, string path, string profile, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !GetShader(path, Array.Empty<ShaderMacro>(), &pShader))
            {
                device.Compile(code, "main", path, profile, &pShader);
                CacheShader("main", Array.Empty<ShaderMacro>(), pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompileFile(IGraphicsDevice device, string path, string profile, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !GetShader(path, Array.Empty<ShaderMacro>(), &pShader))
            {
                device.CompileFromFile(path, "main", profile, &pShader);
                CacheShader("main", Array.Empty<ShaderMacro>(), pShader);
            }
            *shader = pShader;
        }

        public static unsafe bool GetShader(string path, ShaderMacro[] macros, Shader** shader)
        {
            lock (entries)
            {
                *shader = default;
                if (DisableCache) return false;

                var ventry = new ShaderCacheEntry(path, macros, null);
                var entry = entries.FirstOrDefault(x => x.Equals(ventry));
                if (entry != default)
                {
                    *shader = entry.Shader;
                    return true;
                }
                return false;
            }
        }

        public static void Clear()
        {
            lock (entries)
            {
                ImGuiConsole.Log(LogSeverity.Info, "Clearing shader cache ...");
                for (int i = 0; i < entries.Count; i++)
                {
                    entries[i].Free();
                }
                entries.Clear();
                ImGuiConsole.Log(LogSeverity.Info, "Clearing shader cache ... done");
            }
        }

        private static unsafe bool GetShaderInternal(string path, ShaderMacro[] macros, Shader** shader)
        {
            lock (entries)
            {
                *shader = null;
                if (DisableCache) return false;

                var ventry = new ShaderCacheEntry(path, macros, null);
                var entry = entries.FirstOrDefault(x => x.Equals(ventry));
                if (entry != default)
                {
                    *shader = entry.Shader;
                    return true;
                }
                return false;
            }
        }

        private static void Load()
        {
            if (!File.Exists(file)) return;
            lock (entries)
            {
                var span = (Span<byte>)File.ReadAllBytes(file);
                var decoder = Encoding.UTF8.GetDecoder();
                var count = BinaryPrimitives.ReadInt32LittleEndian(span);
                entries.EnsureCapacity(count);

                int idx = 4;
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
            lock (entries)
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
    }

    public unsafe struct ShaderCacheEntry : IEquatable<ShaderCacheEntry>
    {
        public string Name;
        public ShaderMacro[] Macros;
        public Shader* Shader;

        public ShaderCacheEntry(string name, ShaderMacro[] macros, Shader* bytecode)
        {
            Name = name;
            Macros = macros;
            Shader = bytecode;
        }

        public int Write(Span<byte> dest, Encoder encoder)
        {
            int idx = 0;
            idx += WriteString(dest[idx..], Name, encoder);
            BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], Macros.Length);
            idx += 4;
            for (int i = 0; i < Macros.Length; i++)
            {
                var macro = Macros[i];
                idx += WriteString(dest[idx..], macro.Name, encoder);
                idx += WriteString(dest[idx..], macro.Definition, encoder);
            }
            if (Shader != null)
                BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], (int)Shader->Length);
            else
                BinaryPrimitives.WriteInt32LittleEndian(dest[idx..], 0);
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
            Shader = Utilities.Alloc<Shader>();
            Shader->Bytecode = Utilities.Alloc<byte>(len);
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

        private static int SizeOf(string str, Encoder encoder)
        {
            return 4 + encoder.GetByteCount(str, true);
        }

        public int SizeOf(Encoder encoder)
        {
            if (Shader != null)
                return 16 + SizeOf(Name, encoder) + Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder)) + (int)Shader->Length;
            else
                return 16 + SizeOf(Name, encoder) + Macros.Sum(x => SizeOf(x.Name, encoder) + SizeOf(x.Definition, encoder));
        }

        public void Free()
        {
            Shader->Free();
            Utilities.Free(Shader);
        }

        public bool Equals(ShaderCacheEntry other)
        {
            if (Name != other.Name) return false;
            if (Macros == other.Macros && Macros == null && other.Macros == null) return true;
            if (Macros != other.Macros && (Macros == null || other.Macros == null)) return false;
            if (Macros.Length != (other.Macros?.Length ?? 0)) return false;
            for (int i = 0; i < Macros.Length; i++)
            {
                if (Macros[i].Name != other.Macros[i].Name ||
                    Macros[i].Definition != other.Macros[i].Definition)
                {
                    return false;
                }
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