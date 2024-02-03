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
    using System.Threading;

    /// <summary>
    /// Thread-safe shader cache, allows up to 64 read threads at the same time to access the cache and one write thread.
    /// </summary>
    public static class ShaderCache
    {
        private const string file = "cache/shadercache.bin";
        private static readonly List<ShaderCacheEntry> entries = [];
        private static readonly int MaxReadThreads = Environment.ProcessorCount - 1 == 1 ? 2 : Environment.ProcessorCount - 1;
        private static readonly SemaphoreSlim readSemaphore = new(MaxReadThreads);
        private static readonly SemaphoreSlim writeSemaphore = new(1);
        private static readonly ManualResetEvent writeHandle = new(true);
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
            if (DisableCache)
            {
                return;
            }

            var entry = new ShaderCacheEntry(path, crc32Hash, language, macros, inputElements, shader->Clone());

            BeginRead();

            int index = entries.IndexOf(entry);

            EndRead();

            BeginWrite();

            if (index == -1)
            {
                entries.Add(entry);
            }
            else
            {
                entries[index].Free();
                entries[index] = entry;
            }

            EndWrite();

            SaveAsync();
        }

        /// <summary>
        /// Returns true if successfully found a matching shader
        /// </summary>
        /// <param filename="path"></param>
        /// <param filename="language"></param>
        /// <param filename="macros"></param>
        /// <param filename="shader"></param>
        /// <param filename="inputElements"></param>
        /// <returns></returns>
        public static unsafe bool GetShader(string path, uint crc32Hash, SourceLanguage language, ShaderMacro[] macros, Shader** shader, [MaybeNullWhen(false)] out InputElementDescription[]? inputElements)
        {
            *shader = default;
            inputElements = null;

            if (DisableCache)
            {
                return false;
            }

            BeginRead();

            ShaderCacheEntry vEntry = default;
            vEntry.Name = path;
            vEntry.Crc32Hash = crc32Hash;
            vEntry.Language = language;
            vEntry.Macros = macros;

            var entry = entries.FirstOrDefault(x => x.Equals(vEntry));
            if (entry != default && entry.Crc32Hash == crc32Hash)
            {
                inputElements = entry.InputElements;
                *shader = entry.Shader->Clone();
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

            File.WriteAllBytes(file, span.ToArray());
        }

        /// <summary>
        /// Saves the shader cache to disk asynchronously.
        /// </summary>
        public static Task SaveAsync()
        {
            return Task.Run(() =>
            {
                BeginRead();

                var encoder = Encoding.UTF8.GetEncoder();
                var size = 8 + entries.Sum(x => x.SizeOf(encoder));
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

                File.WriteAllBytes(file, span.ToArray());
            });
        }

        private static void BeginWrite()
        {
            writeSemaphore.Wait();

            // block all read threads and wait for completion.
            writeHandle.Reset();
            SpinWait.SpinUntil(() => readSemaphore.CurrentCount == MaxReadThreads);
        }

        private static void EndWrite()
        {
            writeHandle.Set();
            writeSemaphore.Release();
        }

        private static void BeginRead()
        {
            writeHandle.WaitOne();
            readSemaphore.Wait();
        }

        private static void EndRead()
        {
            readSemaphore.Release();
        }
    }
}