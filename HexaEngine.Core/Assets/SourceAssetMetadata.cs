namespace HexaEngine.Core.Assets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using System.Diagnostics.CodeAnalysis;

    public class SourceAssetMetadata
    {
        [JsonConstructor]
        public SourceAssetMetadata(string filePath, Guid guid, DateTime lastModified, uint crc32, Dictionary<string, object> additional)
        {
            FilePath = filePath;
            Guid = guid;
            LastModified = lastModified;
            CRC32 = crc32;
            Additional = additional;
        }

        public SourceAssetMetadata(string filePath, DateTime lastModified, uint crc32)
        {
            FilePath = filePath;
            Guid = Guid.NewGuid();
            LastModified = lastModified;
            CRC32 = crc32;
            Additional = [];
        }

        [JsonIgnore]
        internal string MetadataFilePath { get; set; }

        public string FilePath { get; internal set; }

        public Guid Guid { get; }

        public DateTime LastModified { get; internal set; }

        public uint CRC32 { get; internal set; }

        public Dictionary<string, object> Additional { get; internal set; }

        private static readonly JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });

        internal static SourceAssetMetadata? GetMetadata(string path)
        {
            string? metafile = GetMetadataFilePath(path);
            if (metafile == null)
            {
                Logger.Error($"Failed to load metadata file, {path}");
                //MessageBox.Show($"Failed to load metadata file, {metafile}", "Couldn't determine correct path");
                return null;
            }

            SourceAssetMetadata? metadata = null;

            try
            {
                var reader = File.OpenText(metafile);

                try
                {
                    metadata = (SourceAssetMetadata?)serializer.Deserialize(reader, typeof(SourceAssetMetadata));
                    if (metadata != null)
                    {
                        metadata.MetadataFilePath = metafile;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to load metadata file, {metafile} ({path})");
                //MessageBox.Show($"Failed to load metadata file ({path})", ex.Message);
            }

            return metadata;
        }

        internal static SourceAssetMetadata? LoadMetadata(string metafile)
        {
            SourceAssetMetadata? metadata = null;

            try
            {
                var reader = File.OpenText(metafile);

                try
                {
                    metadata = (SourceAssetMetadata?)serializer.Deserialize(reader, typeof(SourceAssetMetadata));
                    metadata.MetadataFilePath = metafile;
                }
                finally
                {
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to load metadata file ({metafile})");
                MessageBox.Show($"Failed to load metadata file ({metafile})", ex.Message);
            }

            return metadata;
        }

        public void Save()
        {
            string? metafile = MetadataFilePath;
            if (metafile == null)
            {
                Logger.Error($"Failed to save metadata file, {metafile}");
                //MessageBox.Show($"Failed to save metadata file, {metafile}", "Couldn't determine correct path");
                return;
            }
            Save(metafile);
        }

        public string GetFullPath()
        {
            return Path.Combine(SourceAssetsDatabase.RootFolder, FilePath);
        }

        internal void Save(string path)
        {
            try
            {
                FileAttributes attributes;
                if (File.Exists(path))
                {
                    attributes = File.GetAttributes(path);

                    attributes &= ~FileAttributes.Hidden;

                    File.SetAttributes(path, attributes);
                }

                var writer = File.CreateText(path);

                try
                {
                    serializer.Serialize(writer, this);
                }
                finally
                {
                    writer.Close();
                }

                attributes = File.GetAttributes(path);

                attributes |= FileAttributes.Hidden;

                File.SetAttributes(path, attributes);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to save metadata file ({path})");
                //MessageBox.Show($"Failed to save metadata file ({path})", ex.Message);
            }
        }

        internal static SourceAssetMetadata Create(string filename, DateTime lastModified, uint crc32, string outputPath)
        {
            SourceAssetMetadata metadata = new(filename, lastModified, crc32);
            metadata.Save(outputPath);

            metadata.MetadataFilePath = outputPath;
            return metadata;
        }

        private static bool IgnoreDir(ReadOnlySpan<char> span)
        {
            if (span.IsWhiteSpace() || span.Contains('.') || span.Contains("bin", StringComparison.InvariantCulture) || span.Contains("obj", StringComparison.InvariantCulture) || span.Contains("build", StringComparison.InvariantCulture))
            {
                return true;
            }

            return false;
        }

        private static bool IgnoreFile(ReadOnlySpan<char> span)
        {
            if (span.IsWhiteSpace() || span.StartsWith(".") || span.EndsWith(".meta"))
            {
                return true;
            }

            return false;
        }

        public static string? GetMetadataFilePath(string? path)
        {
            if (path == null)
            {
                return null;
            }

            ReadOnlySpan<char> span = path;
            ReadOnlySpan<char> dir = Path.GetDirectoryName(span);

            if (IgnoreDir(dir))
            {
                return null;
            }

            ReadOnlySpan<char> name = Path.GetFileName(span);

            if (IgnoreFile(name))
            {
                return null;
            }

            string metafile = $"{dir}\\{name}.meta";

            return metafile;
        }

        public bool TryGetKey<T>(string key, [MaybeNullWhen(false)] out T? value)
        {
            if (Additional.TryGetValue(key, out var metadataKey) && metadataKey is T t)
            {
                value = t;
                return true;
            }

            value = default;
            return false;
        }

        public T? GetOrCreateKey<T>(string key, T? defaultValue)
        {
            if (Additional.TryGetValue(key, out var value))
            {
                if (value is T t)
                {
                    return t;
                }

                Additional[key] = defaultValue;
                return defaultValue;
            }

            Additional.Add(key, defaultValue);
            return defaultValue;
        }

        public T? GetOrAddValue<T>(string key, T defaultValue)
        {
            return GetOrCreateKey(key, defaultValue);
        }

        public void SetValue<T>(string key, T value)
        {
            GetOrCreateKey(key, value);
        }

        public void Update()
        {
            SourceAssetsDatabase.Update(this);
        }

        public Task UpdateAsync()
        {
            return SourceAssetsDatabase.UpdateAsync(this);
        }

        public void Delete()
        {
            SourceAssetsDatabase.Delete(FilePath);
        }

        public void Rename(string newName)
        {
            var extension = Path.GetExtension(FilePath);
            var dir = Path.GetDirectoryName(FilePath);

            if (dir == null)
            {
                return;
            }

            var newPath = Path.Combine(dir, $"{newName}{extension}");
            SourceAssetsDatabase.Move(FilePath, newPath);
        }
    }
}