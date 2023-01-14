namespace HexaEngine.Scenes.Serialization
{
    using HexaEngine.Scenes.Managers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;

    public unsafe class SceneSerializer
    {
        public static void Serialize(Scene scene)
        {
            JsonSerializerSettings settings = new()
            {
                ConstructorHandling = ConstructorHandling.Default,
                DefaultValueHandling = DefaultValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Include,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
            };
            var serializer = JsonSerializer.Create(settings);

            BsonDataWriter writer = new(File.Create("scene.json"));

            serializer.Serialize(writer, scene);

            writer.Close();
        }

        public static Scene? Deserialize()
        {
            JsonSerializerSettings settings = new()
            {
                ConstructorHandling = ConstructorHandling.Default,
                DefaultValueHandling = DefaultValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Include,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            };
            var serializer = JsonSerializer.Create(settings);

            BsonDataReader reader = new(File.OpenRead("scene.json"));

            Scene? result = (Scene?)serializer.Deserialize(reader, typeof(Scene));
            result?.BuildTree();
            reader.Close();
            return result;
        }
    }
}