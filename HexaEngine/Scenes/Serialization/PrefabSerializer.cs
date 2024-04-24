namespace HexaEngine.Scenes.Serialization
{
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using System.Diagnostics.CodeAnalysis;

    public unsafe class PrefabSerializer
    {
        private static readonly JsonSerializerSettings settings = new()
        {
            ConstructorHandling = ConstructorHandling.Default,
            DefaultValueHandling = DefaultValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            SerializationBinder = new CustomSerializationBinder(),
            ContractResolver = new CustomContractResolver(),
            Converters = [new PropertyListConverter()]
        };

        private static readonly JsonSerializer serializer = JsonSerializer.Create(settings);

        public static byte[] Serialize(Prefab prefab)
        {
            MemoryStream ms = new();

            BsonDataWriter writer = new(ms);

            serializer.Serialize(writer, prefab);

            var bytes = ms.ToArray();

            writer.Close();
            ms.Close();

            return bytes;
        }

        public static void Serialize(Prefab prefab, string path)
        {
            BsonDataWriter writer = new(File.Create(path));
            try
            {
                serializer.Serialize(writer, prefab);
            }
            finally
            {
                writer.Close();
            }
        }

        public static bool TrySerialize(Prefab prefab, string path, [NotNullWhen(false)] out Exception? exception)
        {
            try
            {
                Serialize(prefab, path);
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static Prefab? Deserialize(string path)
        {
            BsonDataReader reader = new(File.OpenRead(path));

            Prefab? prefab;
            try
            {
                prefab = (Prefab?)serializer.Deserialize(reader, typeof(Prefab)) ?? throw new InvalidDataException("scene was null, failed to deserialize");
            }
            finally
            {
                reader.Close();
            }

            prefab.Path = path;
            prefab.BuildReferences();

            return prefab;
        }

        public static bool TryDeserialize(string path, [NotNullWhen(true)] out Prefab? prefab, [NotNullWhen(false)] out Exception? exception)
        {
            try
            {
                prefab = Deserialize(path) ?? throw new InvalidDataException("scene was null, failed to deserialize");
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                prefab = null;
                exception = ex;
                return false;
            }
        }
    }
}