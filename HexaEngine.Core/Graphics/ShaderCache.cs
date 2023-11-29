namespace HexaEngine.Core.Graphics
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

        /// <summary>
        /// Gets or sets a value indicating whether shader caching is disabled.
        /// </summary>
        public static bool DisableCache { get; set; } = false;

        /// <summary>
        /// Gets the list of shader cache entries.
        /// </summary>
        public static IReadOnlyList<ShaderCacheEntry> Entries => entries;

        /// <summary>
        /// Caches a shader in the shader cache.
        /// </summary>
        /// <param name="path">The path to the shader file.</param>
        /// <param name="crc32Hash">The CRC32 hash of the shader file content.</param>
        /// <param name="language">The source language of the shader.</param>
        /// <param name="macros">The array of shader macros.</param>
        /// <param name="inputElements">The array of input elements for the shader.</param>
        /// <param name="shader">The pointer to the shader.</param>
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
        /// <param filename="path"></param>
        /// <param filename="language"></param>
        /// <param filename="macros"></param>
        /// <param filename="shader"></param>
        /// <param filename="inputElements"></param>
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

        /// <summary>
        /// Clears the shader cache.
        /// </summary>
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

        private const int HeaderSize = 8;

        private static void Load()
        {
            if (!File.Exists(file))
            {
                return;
            }

            lock (entries)
            {
                var span = (Span<byte>)File.ReadAllBytes(file);
                if (span.Length < HeaderSize)
                {
                    return;
                }
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
                var size = HeaderSize + entries.Sum(x => x.SizeOf(encoder));
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
}