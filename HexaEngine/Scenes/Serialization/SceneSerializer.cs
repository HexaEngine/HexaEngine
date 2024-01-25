namespace HexaEngine.Scenes.Serialization
{
    using HexaEngine.Core;
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

            serializer.Serialize(writer, scene);

            writer.Close();

            if (Application.InEditorMode)
            {
                var dir = Path.GetDirectoryName(path) ?? string.Empty;
                var fileName = Path.GetFileNameWithoutExtension(path);
                var editorConfigPath = Path.Combine(dir, fileName + ".editor.json");
                scene.EditorConfig.SaveTo(editorConfigPath);
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

            Scene? scene = (Scene?)serializer.Deserialize(reader, typeof(Scene)) ?? throw new InvalidDataException("scene was null, failed to deserialize");
            scene.Path = path;
            scene.BuildReferences();
            reader.Close();

            if (Application.InEditorMode)
            {
                var dir = Path.GetDirectoryName(path) ?? string.Empty;
                var fileName = Path.GetFileNameWithoutExtension(path);
                var editorConfigPath = Path.Combine(dir, fileName + ".editor.json");
                scene.EditorConfig = Config.LoadFrom(editorConfigPath);
            }

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