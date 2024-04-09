namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.Security.Cryptography;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Thread-safe shader cache, allows up to 64 read threads at the same time to access the cache and one write thread.
    /// </summary>
    public static class ShaderCache
    {
        private const string file = "cache/shadercache.bin";
        private static readonly List<ShaderCacheEntry> entries = [];
        private static readonly Dictionary<SHA256Signature, ShaderCacheEntry> keyToEntry = [];
        private static readonly int MaxConcurrentReaders = 64;
        private static readonly SemaphoreSlim readSemaphore = new(MaxConcurrentReaders);
        private static readonly SemaphoreSlim writeSemaphore = new(1);
        private static readonly SemaphoreSlim fileSemaphore = new(1);
        private static readonly ManualResetEvent writeHandle = new(true);
        private static readonly ManualResetEvent readHandle = new(true);
        private const int Version = 3;

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

        public static SHA256Signature GetKey(string name, SourceLanguage language, ShaderMacro[] macros)
        {
            Span<SHA256Signature> signature = stackalloc SHA256Signature[2];
            Span<byte> hash0 = MemoryMarshal.AsBytes(signature);
            Span<byte> hash1 = MemoryMarshal.AsBytes(signature[1..]);

            SHA256.HashData(MemoryMarshal.AsBytes(name.AsSpan()), hash0);

            SHA256.HashData(MemoryMarshal.AsBytes([language]), hash1);

            signature[0] ^= signature[1];

            for (int i = 0; i < macros.Length; i++)
            {
                var macro = macros[i];

                SHA256.HashData(MemoryMarshal.AsBytes(macro.Name.AsSpan()), hash1);
                signature[0] ^= signature[1];
                SHA256.HashData(MemoryMarshal.AsBytes(macro.Definition.AsSpan()), hash1);
                signature[0] ^= signature[1];
            }

            return signature[0];
        }

        private static bool TryGetEntry(SHA256Signature key, [MaybeNullWhen(false)] out ShaderCacheEntry? entry)
        {
            return keyToEntry.TryGetValue(key, out entry);
        }

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
            if (DisableCache)
            {
                return;
            }

            var key = GetKey(path, language, macros);

            CacheShader(key, crc32Hash, inputElements, shader);
        }

        public static unsafe void CacheShader(SHA256Signature key, uint crc32Hash, InputElementDescription[] inputElements, Shader* shader)
        {
            if (DisableCache)
            {
                return;
            }

            BeginRead();

            if (TryGetEntry(key, out ShaderCacheEntry? entry))
            {
                entry.Lock();

                entry.Free();
                entry.Crc32Hash = crc32Hash;
                entry.Shader = shader->Clone();

                entry.ReleaseLock();

                EndRead();
            }
            else
            {
                EndRead();

                BeginWrite();

                entry = new(key, crc32Hash, inputElements, shader->Clone());
                entries.Add(entry);
                keyToEntry.Add(key, entry);

                EndWrite();
            }

            SaveAsync();
        }

        public static unsafe bool GetShader(string path, uint crc32Hash, SourceLanguage language, ShaderMacro[] macros, Shader** shader, [MaybeNullWhen(false)] out InputElementDescription[]? inputElements)
        {
            *shader = default;
            inputElements = null;

            if (DisableCache)
            {
                return false;
            }

            var key = GetKey(path, language, macros);

            return GetShader(key, crc32Hash, shader, out inputElements);
        }

        public static unsafe bool GetShader(SHA256Signature key, uint crc32Hash, Shader** shader, [MaybeNullWhen(false)] out InputElementDescription[]? inputElements)
        {
            *shader = default;
            inputElements = null;

            if (DisableCache)
            {
                return false;
            }

            BeginRead();

            if (TryGetEntry(key, out ShaderCacheEntry? entry))
            {
                entry.Lock();

                if (entry.Crc32Hash != crc32Hash)
                {
                    entry.ReleaseLock();
                    EndRead();
                    return false;
                }

                inputElements = entry.InputElements;
                *shader = entry.Shader->Clone();

                entry.ReleaseLock();
                EndRead();

                return true;
            }

            EndRead();
            return false;
        }

        /// <summary>
        /// Clears the shader cache.
        /// </summary>
        public static void Clear()
        {
            BeginWrite();
            Logger.Info("Clearing shader cache ...");
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Free();
            }
            entries.Clear();
            keyToEntry.Clear();
            Logger.Info("Clearing shader cache ... done");
            EndWrite();
        }

        private const int HeaderSize = 8;

        /// <summary>
        /// Loads the shader cache from disk.
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(file))
            {
                return;
            }

            BeginWrite();

            var span = (Span<byte>)File.ReadAllBytes(file);
            if (span.Length < HeaderSize)
            {
                EndWrite();
                return;
            }
            var decoder = Encoding.UTF8.GetDecoder();
            var version = BinaryPrimitives.ReadInt32LittleEndian(span);
            if (version != Version)
            {
                EndWrite();
                return;
            }

            var count = BinaryPrimitives.ReadInt32LittleEndian(span[4..]);
            entries.EnsureCapacity(count);

            int idx = 8;
            for (int i = 0; i < count; i++)
            {
                var entry = new ShaderCacheEntry();
                idx += entry.Read(span[idx..], decoder);
                entries.Add(entry);
                keyToEntry.Add(entry.Key, entry);
            }

            EndWrite();
        }

        /// <summary>
        /// Saves the shader cache to disk.
        /// </summary>
        public static void Save()
        {
            BeginRead();

            var encoder = Encoding.UTF8.GetEncoder();
            var size = HeaderSize + entries.Sum(x => x.SizeOf(encoder));
            var span = size < 4096 ? stackalloc byte[size] : new byte[size];

            BinaryPrimitives.WriteInt32LittleEndian(span[0..], Version);
            BinaryPrimitives.WriteInt32LittleEndian(span[4..], entries.Count);

            int idx = 8;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                idx += entry.Write(span[idx..], encoder);
            }

            EndRead();

            fileSemaphore.Wait();
            File.WriteAllBytes(file, span.ToArray());
            fileSemaphore.Release();
        }

        /// <summary>
        /// Saves the shader cache to disk asynchronously.
        /// </summary>
        public static Task SaveAsync()
        {
            return Task.Run(Save);
        }

        private static void BeginWrite()
        {
            writeSemaphore.Wait();

            // block all read threads and wait for completion.
            writeHandle.Reset();

            readHandle.WaitOne();
        }

        private static void EndWrite()
        {
            writeHandle.Set();

            writeSemaphore.Release();
        }

        private static void BeginRead()
        {
            writeHandle.WaitOne();

            readHandle.Reset();

            readSemaphore.Wait();
        }

        private static void EndRead()
        {
            readSemaphore.Release();

            if (readSemaphore.CurrentCount == MaxConcurrentReaders)
            {
                readHandle.Set();
            }
        }
    }
}