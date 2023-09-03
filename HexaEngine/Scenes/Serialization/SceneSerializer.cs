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
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
        };

        private static readonly JsonSerializer serializer = JsonSerializer.Create(settings);

        public static void Serialize(Scene scene, string path)
        {
            BsonDataWriter writer = new(File.Create(path));

            serializer.Serialize(writer, scene);

            writer.Close();
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

            Scene? result = (Scene?)serializer.Deserialize(reader, typeof(Scene)) ?? throw new InvalidDataException("scene was null, failed to deserialize");
            result.Path = path;
            result.BuildReferences();
            reader.Close();
            return result;
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