namespace HexaEngine.Scenes.Managers
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Animations;
    using System.Collections.Generic;

    public class AnimationManager : IDisposable
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(AnimationManager));
        private readonly List<AnimationLibrary> libraries = new();
        private readonly Dictionary<string, AnimationLibrary> pathToLib = new();

        private readonly List<AnimationClip> animations = new();
        private readonly Dictionary<AnimationClip, AnimationLibrary> animationToLib = new();

        private readonly object _lock = new();
        private bool disposedValue;

        public IReadOnlyList<AnimationLibrary> Libraries => libraries;

        public IReadOnlyList<AnimationClip> Animations => animations;

        public int Count => animations.Count;

        public AnimationLibrary GetMaterialLibraryForm(AnimationClip animation)
        {
            lock (_lock)
            {
                return animationToLib[animation];
            }
        }

        public string? GetPathToAnimationLibrary(AnimationLibrary library)
        {
            lock (_lock)
            {
                var result = pathToLib.FirstOrDefault(x => x.Value == library);
                return result.Key;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                libraries.Clear();
                pathToLib.Clear();
                animations.Clear();
                animationToLib.Clear();
            }
        }

        public AnimationLibrary Load(string path)
        {
            AnimationLibrary? library;
            lock (_lock)
            {
                if (pathToLib.TryGetValue(path, out library))
                {
                    return library;
                }

                if (FileSystem.Exists(path))
                {
                    library = AnimationLibrary.Load(path);
                }
                else
                {
                    library = AnimationLibrary.Empty;
                    Logger.Warn($"Warning couldn't find material library {path}");
                }

                libraries.Add(library);
                pathToLib.Add(path, library);

                for (int i = 0; i < library.Animations.Count; i++)
                {
                    animations.Add(library.Animations[i]);
                    animationToLib.Add(library.Animations[i], library);
                }
            }

            return library;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                lock (_lock)
                {
                    libraries.Clear();
                    pathToLib.Clear();
                    animations.Clear();
                    animationToLib.Clear();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}