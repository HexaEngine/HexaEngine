﻿namespace HexaEngine.Core.Scenes.Managers
{
    using BepuPhysics.Collidables;
    using HexaEngine.Core.Animations;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Resources;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public class AnimationManager
    {
        private readonly Dictionary<string, Animation> pathToAnimations = new();
        private readonly List<Animation> animations = new();

        public IReadOnlyList<Animation> Animations => animations;

        public int Count => animations.Count;

        public void Clear()
        {
            lock (animations)
            {
                animations.Clear();
                pathToAnimations.Clear();
            }
        }

        public Animation Load(string path)
        {
            lock (animations)
            {
                if (!pathToAnimations.TryGetValue(path, out var value))
                {
                    value = Animation.Load(path);
                    pathToAnimations.Add(path, value);
                    animations.Add(value);
                }
                return value;
            }
        }

        public void Unload(Animation animation)
        {
            lock (animations)
            {
                if (animations.Contains(animation))
                {
                    animations.Remove(animation);
                    pathToAnimations.Remove(animation.Name + ".anim");
                }
            }
        }
    }
}