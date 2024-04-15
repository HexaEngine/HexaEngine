namespace HexaEngine.Scenes.Serialization
{
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using Newtonsoft.Json.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public class PropertyRenameAndIgnoreSerializerContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> _ignores;
        private readonly Dictionary<Type, Dictionary<string, string>> _renames;

        public PropertyRenameAndIgnoreSerializerContractResolver()
        {
            _ignores = new Dictionary<Type, HashSet<string>>();
            _renames = new Dictionary<Type, Dictionary<string, string>>();
        }

        public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
        {
            if (!_ignores.ContainsKey(type))
                _ignores[type] = new HashSet<string>();

            foreach (var prop in jsonPropertyNames)
                _ignores[type].Add(prop);
        }

        public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
        {
            if (!_renames.ContainsKey(type))
                _renames[type] = new Dictionary<string, string>();

            _renames[type][propertyName] = newJsonPropertyName;
        }

        protected override string ResolveExtensionDataName(string extensionDataName)
        {
            return base.ResolveExtensionDataName(extensionDataName);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldSerialize = i => false;
                property.Ignored = true;
            }

            if (IsRenamed(property.DeclaringType, property.PropertyName, out var newJsonPropertyName))
                property.PropertyName = newJsonPropertyName;

            return property;
        }

        private bool IsIgnored(Type type, string jsonPropertyName)
        {
            if (!_ignores.ContainsKey(type))
                return false;

            return _ignores[type].Contains(jsonPropertyName);
        }

        private bool IsRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
        {
            Dictionary<string, string> renames;

            if (!_renames.TryGetValue(type, out renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
            {
                newJsonPropertyName = null;
                return false;
            }

            return true;
        }
    }

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
            SerializationBinder = new CustomSerializationBinder()
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