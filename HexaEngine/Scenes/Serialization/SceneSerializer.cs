namespace HexaEngine.Scenes.Serialization
{
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using System.Diagnostics.CodeAnalysis;

    public unsafe class SceneSerializer
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

        public static byte[] Serialize(Scene scene)
        {
            MemoryStream ms = new();

            BsonDataWriter writer = new(ms);

            serializer.Serialize(writer, scene);

            var bytes = ms.ToArray();

            writer.Close();
            ms.Close();

            return bytes;
        }

        public static void Serialize(Scene scene, string path)
        {
            BsonDataWriter writer = new(File.Create(path));
            try
            {
                serializer.Serialize(writer, scene);
            }
            finally
            {
                writer.Close();
            }
        }

        public static bool TrySerialize(Scene scene, string path, [NotNullWhen(false)] out Exception? exception)
        {
            try
            {
                Serialize(scene, path);
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static Scene? Deserialize(string path)
        {
            BsonDataReader reader = new(File.OpenRead(path));

            Scene? scene;
            try
            {
                scene = (Scene?)serializer.Deserialize(reader, typeof(Scene)) ?? throw new InvalidDataException("scene was null, failed to deserialize");
            }
            finally
            {
                reader.Close();
            }

            IScene scene1 = scene;

            scene.Path = path;
            scene1.BuildReferences();

            return scene;
        }

        public static bool TryDeserialize(string path, [NotNullWhen(true)] out Scene? scene, [NotNullWhen(false)] out Exception? exception)
        {
            try
            {
                scene = Deserialize(path) ?? throw new InvalidDataException("scene was null, failed to deserialize");
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                scene = null;
                exception = ex;
                return false;
            }
        }
    }
}