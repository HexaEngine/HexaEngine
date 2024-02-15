namespace HexaEngine.Scenes
{
    using HexaEngine.Mathematics;
    using Newtonsoft.Json.Bson;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class Prefab : EntityNotifyBase, IReadOnlyPrefab
    {
        private readonly Guid guid = Guid.NewGuid();
        private string name = string.Empty;
        private readonly Transform transform = new();
        private readonly List<IComponent> components = [];
        private readonly List<GameObject> children = [];

        private string? fullName;

        [JsonConstructor]
        public Prefab(Guid guid, string name, Transform transform, List<IComponent> components, List<GameObject> children)
        {
            this.guid = guid;
            this.name = name;
            this.transform = transform;
            this.components = components;
            this.children = children;
        }

        public Prefab()
        {
        }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid
        {
            get => guid;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                fullName = null;
            }
        }

        /// <summary>
        /// Gets the full name of the Prefab, which is a unique combination of its name and Guid.
        /// The full name is lazily generated when accessed for the first time and will be
        /// reinitialized if either the name or Guid property is modified.
        /// </summary>
        /// <value>
        /// The full name of the Prefab.
        /// </value>
        [JsonIgnore]
        public string FullName
        {
            get
            {
                return fullName ??= $"{name}##{Guid}";
            }
        }

        public Transform Transform => transform;

        public virtual List<IComponent> Components => components;

        public virtual List<GameObject> Children => children;

        IReadOnlyList<IComponent> IReadOnlyPrefab.Components => components;

        IReadOnlyList<GameObject> IReadOnlyPrefab.Children => children;

        IReadOnlyTransform IReadOnlyPrefab.Transform => transform;

        private static readonly JsonSerializerSettings settings = new()
        {
            ConstructorHandling = ConstructorHandling.Default,
            DefaultValueHandling = DefaultValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
        };

        private static readonly JsonSerializer serializer = JsonSerializer.Create(settings);

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

            return prefab;
        }

        public static bool TryDeserialize(string path, [NotNullWhen(true)] out Prefab? prefab, [NotNullWhen(false)] out Exception? exception)
        {
            try
            {
                prefab = Deserialize(path) ?? throw new InvalidDataException("Prefab was null, failed to deserialize");
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