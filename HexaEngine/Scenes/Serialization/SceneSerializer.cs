namespace HexaEngine.Scenes.Serialization
{
    using HexaEngine.IO;
    using HexaEngine.Scenes.Managers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;

    public unsafe class SceneSerializer
    {
        public static void Serialize(Scene scene, string path)
        {
            JsonSerializerSettings settings = new()
            {
                ConstructorHandling = ConstructorHandling.Default,
                DefaultValueHandling = DefaultValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Include,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            };
            var serializer = JsonSerializer.Create(settings);

            BsonDataWriter writer = new(File.Create(path));

            serializer.Serialize(writer, scene);

            writer.Close();
        }

        public static Scene? Deserialize(string path)
        {
            JsonSerializerSettings settings = new()
            {
                ConstructorHandling = ConstructorHandling.Default,
                DefaultValueHandling = DefaultValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Include,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            };
            var serializer = JsonSerializer.Create(settings);

            BsonDataReader reader = new(File.OpenRead(path));

            Scene? result = (Scene?)serializer.Deserialize(reader, typeof(Scene)) ?? throw new InvalidDataException();
            result.Path = path;
            result.BuildTree();
            reader.Close();
            return result;
        }
    }
}