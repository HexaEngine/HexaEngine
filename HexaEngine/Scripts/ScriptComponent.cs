namespace HexaEngine.Scripts
{
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public delegate void AwakeDelegate();

    public delegate void UpdateDelegate();

    public delegate void FixedUpdateDelegate();

    public delegate void DestroyDelegate();

    [Guid("340856B8-4341-4615-B109-26DB6D6536DE")]
    [EditorComponent<ScriptComponent>("Script Behaviour")]
    public class ScriptComponent : IScriptComponent
    {
        private GameObject gameObject;
        private ScriptFlags flags;
        private object? instance;
        private PropertyInfo[] properties;
        private AssetRef scriptRef;
        private string? scriptTypeName;
        private Type? scriptType;
        private Dictionary<string, object?> propertyValues = new();
        private AwakeDelegate? awakeDelegate;
        private UpdateDelegate? updateDelegate;
        private FixedUpdateDelegate? fixedUpdateDelegate;
        private DestroyDelegate? destroyDelegate;
        private PropertyInfo? gameObjectProp;

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
                            if (prop.CanWrite && prop.CanWrite)
                            {
                                prop.SetValue(instance, propertyValues[prop.Name]);
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
        public object? Instance { get => instance; set => instance = value; }

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

        public event Action<IHasFlags<ScriptFlags>, ScriptFlags>? FlagsChanged;

        public void Awake()
        {
            ScriptAssemblyManager.AssembliesUnloaded += AssembliesUnloaded;
            ScriptAssemblyManager.AssemblyLoaded += AssemblyLoaded;
            CreateInstance();
            if (Application.InEditMode)
            {
                return;
            }

            if (instance != null && gameObjectProp != null && awakeDelegate != null)
            {
                try
                {
                    gameObjectProp.SetValue(instance, gameObject);
                    awakeDelegate();
                }
                catch (Exception e)
                {
                    LoggerFactory.General.Log(e);
                }
            }
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
        }

        public void Update()
        {
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

        public Task CreateInstanceAsync()
        {
            return Task.Run(CreateInstance);
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
                    instance = Activator.CreateInstance(scriptType);
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

                        gameObjectProp = scriptType.GetProperty(nameof(GameObject));
                    }

                    FlagsChanged?.Invoke(this, flags);

                    properties = scriptType.GetProperties();

                    List<string> toRemove = new(propertyValues.Keys);

                    for (int i = 0; i < properties.Length; i++)
                    {
                        var prop = properties[i];

                        if (prop == gameObjectProp) // skip
                            continue;

                        if (prop.GetCustomAttribute<EditorPropertyAttribute>() != null && prop.CanWrite && prop.CanWrite)
                        {
                            if (propertyValues.TryGetValue(prop.Name, out object? value))
                            {
                                if (prop.PropertyType.IsInstanceOfType(value))
                                {
                                    value = propertyValues[prop.Name] = value;
                                }
                                else if (prop.PropertyType == typeof(float))
                                {
                                    value = propertyValues[prop.Name] = (float)(double)value;
                                }
                                else if (prop.PropertyType == typeof(uint))
                                {
                                    value = propertyValues[prop.Name] = (uint)(long)value;
                                }
                                else if (prop.PropertyType == typeof(int))
                                {
                                    value = propertyValues[prop.Name] = (int)(long)value;
                                }
                                else if (prop.PropertyType.IsEnum)
                                {
                                    value = propertyValues[prop.Name] = Enum.ToObject(prop.PropertyType, (int)(long)value);
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