namespace HexaEngine.Scripts
{
    using Hexa.NET.Logging;
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Serialization;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public delegate void AwakeDelegate();

    public delegate void UpdateDelegate();

    public delegate void FixedUpdateDelegate();

    public delegate void DestroyDelegate();

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.ScriptBehaviour")]
    [Guid("340856B8-4341-4615-B109-26DB6D6536DE")]
    [EditorComponent<ScriptComponent>("Script Behaviour")]
    public class ScriptComponent : IScriptComponent
    {
        private GameObject gameObject = null!;
        private ScriptFlags flags;
        private object? instance;
        private PropertyInfo[] properties = null!;
        private AssetRef scriptRef;
        private string? scriptTypeName;

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicMethods)]
        private Type? scriptType;

        private Dictionary<string, object?> propertyValues = new();
        private AwakeDelegate? awakeDelegate;
        private UpdateDelegate? updateDelegate;
        private FixedUpdateDelegate? fixedUpdateDelegate;
        private DestroyDelegate? destroyDelegate;
        private PropertyInfo? gameObjectProp;
        private bool awaked;
        private Guid guid = Guid.NewGuid();

        /// <summary>
        /// The GUID of the <see cref="ScriptComponent"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid
        {
            get => guid;
            set => guid = value;
        }

        public bool Enabled { get; set; }

        [JsonIgnore]
        public bool IsSerializable { get; } = true;

        [EditorProperty("Script", AssetType.Script)]
        public AssetRef ScriptRef
        {
            get => scriptRef;
            set
            {
                scriptRef = value;
                scriptType = null;
                scriptTypeName = value.GetMetadata()?.Name;

                DestroyInstance();
                CreateInstance();
            }
        }

        [JsonIgnore]
        public Dictionary<string, object?> PropertyValues
        {
            get
            {
                if (properties != null && instance != null)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var prop = properties[i];
                        if (prop.GetCustomAttribute<EditorPropertyAttribute>() != null)
                        {
                            if (prop.PropertyType.IsClass)
                            {
                                continue;
                            }
                            if (prop.CanWrite && prop.CanWrite)
                            {
                                propertyValues[prop.Name] = prop.GetValue(instance);
                            }
                        }
                    }
                }

                return propertyValues;
            }
            set
            {
                if (properties != null && instance != null)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var prop = properties[i];
                        if (prop.GetCustomAttribute<EditorPropertyAttribute>() != null)
                        {
                            if (prop.PropertyType.IsClass)
                            {
                                continue;
                            }
                            if (prop.CanWrite && prop.CanWrite && propertyValues.TryGetValue(prop.Name, out var propValue))
                            {
                                prop.SetValue(instance, propValue);
                            }
                        }
                    }
                }

                propertyValues = value;
            }
        }

        private static readonly JsonSerializerSettings serializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
        };

        public string PropertyValuesString
        {
            get
            {
                return JsonConvert.SerializeObject(PropertyValues, Formatting.None, serializerSettings);
            }
            set
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, object?>>(value, serializerSettings);
                PropertyValues = values ?? new();
            }
        }

        [JsonIgnore]
        public object? Instance { get => instance; protected internal set => instance = value; }

        [JsonIgnore]
        public ScriptFlags Flags => flags;

        [JsonIgnore]
        public GameObject GameObject
        {
            get => gameObject;
            set
            {
                gameObject = value;
                if (instance != null && gameObjectProp != null)
                {
                    gameObjectProp.SetValue(instance, value);
                }
            }
        }

        [JsonIgnore]
        public Type? ScriptType => scriptType;

        [JsonIgnore]
        public int ExecutionOrderIndex { get; set; }

        public event Action<IHasFlags<ScriptFlags>, ScriptFlags>? FlagsChanged;

        public void ScriptCreate()
        {
            if (awaked)
                return;

            awaked = true;
            CreateInstance();
        }

        public void ScriptLoad()
        {
            List<string> toRemove = new(propertyValues.Keys);
            var scene = GameObject.GetScene();
            properties ??= [];
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];

                if (prop == gameObjectProp) // skip
                    continue;

                if (prop.GetCustomAttribute<EditorPropertyAttribute>() != null && prop.CanWrite && prop.CanWrite)
                {
                    if (propertyValues.TryGetValue(prop.Name, out object? value))
                    {
                        if (prop.PropertyType.IsClass && value is Guid guid)
                        {
                            value = ResolveReference(scene, prop.PropertyType, guid);
                        }
                        if (prop.PropertyType.IsInstanceOfType(value))
                        {
                            value = propertyValues[prop.Name] = value;
                        }
                        else if (prop.PropertyType == typeof(float))
                        {
                            value = propertyValues[prop.Name] = (float)(double)value!;
                        }
                        else if (prop.PropertyType == typeof(uint))
                        {
                            value = propertyValues[prop.Name] = (uint)(long)value!;
                        }
                        else if (prop.PropertyType == typeof(int))
                        {
                            value = propertyValues[prop.Name] = (int)(long)value!;
                        }
                        else if (prop.PropertyType.IsEnum)
                        {
                            value = propertyValues[prop.Name] = Enum.ToObject(prop.PropertyType, (int)(long)value!);
                        }
                        else if (!prop.PropertyType.IsInstanceOfType(value))
                        {
                            value = propertyValues[prop.Name] = prop.GetValue(value);
                        }

                        prop.SetValue(instance, value);
                        toRemove.Remove(prop.Name);
                    }
                    else
                    {
                        propertyValues.Add(prop.Name, prop.GetValue(instance));
                    }
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                propertyValues.Remove(toRemove[i]);
            }
        }

        public void ScriptAwake()
        {
            if (Application.InEditMode)
            {
                return;
            }

            if (instance != null && gameObjectProp != null && awakeDelegate != null)
            {
                try
                {
                    awakeDelegate();
                }
                catch (Exception e)
                {
                    LoggerFactory.General.Log(e);
                }
            }
        }

        private static object? ResolveReference(Scene scene, Type type, Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            if (type.IsAssignableTo(typeof(GameObject)))
            {
                return scene.FindByGuid(guid);
            }
            if (type.IsAssignableTo(typeof(IComponent)))
            {
                return scene.FindComponentByGuid(guid);
            }
            if (type.IsAssignableTo(typeof(ScriptBehaviour)))
            {
                return ((ScriptComponent?)scene.FindComponentByGuid(guid))?.Instance;
            }

            return null;
        }

        public void Awake()
        {
            ScriptAssemblyManager.AssembliesUnloaded += AssembliesUnloaded;
            ScriptAssemblyManager.AssemblyLoaded += AssemblyLoaded;
        }

        private void AssembliesUnloaded(object? sender, EventArgs? e)
        {
            if (scriptType == null)
            {
                return;
            }

            scriptType = null;
            DestroyInstance();
        }

        private void AssemblyLoaded(object? sender, Assembly e)
        {
            CreateInstance();
            ScriptLoad();
        }

        public void Update()
        {
            if (!awaked)
            {
                return;
            }
            if (Application.InEditMode || updateDelegate == null)
            {
                return;
            }

            try
            {
                updateDelegate();
            }
            catch (Exception e)
            {
                LoggerFactory.General.Log(e);
            }
        }

        public void FixedUpdate()
        {
            if (!awaked)
            {
                return;
            }
            if (Application.InEditMode || fixedUpdateDelegate == null)
            {
                return;
            }

            try
            {
                fixedUpdateDelegate();
            }
            catch (Exception e)
            {
                LoggerFactory.General.Log(e);
            }
        }

        public void Destroy()
        {
            if (!awaked)
            {
                return;
            }
            awaked = false;
            ScriptAssemblyManager.AssembliesUnloaded -= AssembliesUnloaded;
            ScriptAssemblyManager.AssemblyLoaded -= AssemblyLoaded;
            if (Application.InEditMode || destroyDelegate == null)
            {
                DestroyInstance();
                return;
            }

            try
            {
                destroyDelegate();
            }
            catch (Exception e)
            {
                LoggerFactory.General.Log(e);
            }

            DestroyInstance();
        }

        public void CreateInstance()
        {
            lock (this)
            {
                if (scriptTypeName == null)
                {
                    return;
                }

                scriptType = ScriptAssemblyManager.GetType(scriptTypeName);

                if (scriptType == null)
                {
                    LoggerFactory.General.Error($"Couldn't load script: {scriptTypeName}");
                    return;
                }

                try
                {
                    instance = Activator.CreateInstance(scriptType)!;
                    var methods = scriptType.GetMethods();
                    flags = ScriptFlags.None;
                    for (int i = 0; i < methods.Length; i++)
                    {
                        var method = methods[i];
                        switch (method.Name)
                        {
                            case "Awake":
                                flags |= ScriptFlags.Awake;
                                awakeDelegate = method.CreateDelegate<AwakeDelegate>(instance);
                                break;

                            case "Update":
                                flags |= ScriptFlags.Update;
                                updateDelegate = method.CreateDelegate<UpdateDelegate>(instance);
                                break;

                            case "FixedUpdate":
                                flags |= ScriptFlags.FixedUpdate;
                                fixedUpdateDelegate = method.CreateDelegate<FixedUpdateDelegate>(instance);
                                break;

                            case "Destroy":
                                flags |= ScriptFlags.Destroy;
                                destroyDelegate = method.CreateDelegate<DestroyDelegate>(instance);
                                break;

                            default:
                                continue;
                        }

                        gameObjectProp = scriptType.GetProperty(nameof(GameObject))!;
                        gameObjectProp.SetValue(instance, gameObject);
                    }

                    FlagsChanged?.Invoke(this, flags);

                    properties = scriptType.GetProperties();
                }
                catch (Exception e)
                {
                    LoggerFactory.General.Log(e);
                }
            }
        }

        public void DestroyInstance()
        {
            lock (this)
            {
                instance = null;
                awakeDelegate = null;
                updateDelegate = null;
                fixedUpdateDelegate = null;
                destroyDelegate = null;
                gameObjectProp = null;
            }
        }
    }
}