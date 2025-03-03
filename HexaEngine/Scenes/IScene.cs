namespace HexaEngine.Scenes
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Queries;
    using HexaEngine.Scenes.Managers;
    using HexaEngine.Scripts;
    using HexaEngine.Weather;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    public interface IScene
    {
        AnimationManager AnimationManager { get; }

        CameraContainer Cameras { get; }

        Camera? CurrentCamera { get; }

        SceneDispatcher Dispatcher { get; }

        DrawLayerManager DrawLayerManager { get; }

        SceneFlags Flags { get; }

        IReadOnlyList<GameObject> GameObjects { get; }

        bool Initialized { get; }

        bool IsSimulating { get; set; }

        bool IsPrefabScene { get; set; }

        LightManager LightManager { get; }

        bool Loaded { get; }

        string Name { get; }

        string? Path { get; set; }

        SceneProfiler Profiler { get; }

        QuerySystem QueryManager { get; }

        RenderSystem RenderManager { get; }

        GameObject Root { get; }

        ScriptManager Scripts { get; }

        FlaggedList<SystemFlags, ISceneSystem> Systems { get; }

        bool UnsavedChanged { get; set; }

        bool Valid { get; }

        SceneVariables Variables { get; }

        WeatherSystem WeatherManager { get; }

        event Action<GameObject>? OnGameObjectAdded;

        event Action<GameObject>? OnGameObjectRemoved;

        void AddChild(GameObject node);

        IEnumerable<GameObject> FindAllByName(string? name);

        IEnumerable<GameObject> FindAllByTag(object? tag);

        GameObject? FindByFullName(string? name);

        GameObject? FindByGuid(Guid? guid);

        GameObject? FindByName(string? name);

        GameObject? FindByTag(object? tag);

        string GetAvailableName(string name);

        IEnumerable<GameObject> GetRange(GameObject start, GameObject end);

        T GetRequiredService<T>() where T : class;

        T? GetService<T>() where T : class;

        /// <summary>
        /// Initializes the scene.
        /// </summary>
        /// <remarks>Only call if you know what you are doing.</remarks>
        void Initialize(SceneInitFlags initFlags);

        /// <summary>
        /// Initializes the scene asynchronously.
        /// Only call if you know what you are doing.
        /// </summary>
        /// <remarks>Only call if you know what you are doing.</remarks>
        /// <returns></returns>
        Task InitializeAsync(SceneInitFlags initFlags);

        void Uninitialize();

        /// <summary>
        /// Loads the scene.
        /// </summary>
        /// <remarks>Only call if you know what you are doing.</remarks>
        void Load(IGraphicsDevice device);

        /// <summary>
        /// Unloads the scene.
        /// </summary>
        /// <remarks>Only call if you know what you are doing.</remarks>
        void Unload();

        void RemoveChild(GameObject node);

        void RestoreState();

        void SaveState();

        GameObject? SelectObject(Ray ray);

        bool TryGetService<T>([NotNullWhen(true)] out T? system) where T : class;

        /// <summary>
        /// Do a Scene Tick.
        /// Only call if you know what you are doing.
        /// </summary>
        /// <remarks>Only call if you know what you are doing.</remarks>
        void Update();

        /// <summary>
        /// Do a Scene Graphics Tick.
        /// Only call if you know what you are doing.
        /// </summary>
        /// <remarks>Only call if you know what you are doing.</remarks>
        void GraphicsUpdate(IGraphicsContext context);

        void Validate();

        void BuildReferences();
    }
}