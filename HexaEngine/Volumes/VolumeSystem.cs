namespace HexaEngine.Volumes
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Diagnostics;
    using System.Numerics;

    public struct VolumeTransition : IEquatable<VolumeTransition>
    {
        public long Start;
        public long Duration;
        public IVolume? From;
        public IVolume? To;
        public VolumeTransitionMode Mode;

        public VolumeTransition(long start, long duration, Volume? from, Volume? to, VolumeTransitionMode mode)
        {
            Start = start;
            Duration = duration;
            From = from;
            To = to;
            Mode = mode;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is VolumeTransition transition && Equals(transition);
        }

        public readonly bool Equals(VolumeTransition other)
        {
            return Start == other.Start &&
                   Duration.Equals(other.Duration) &&
                   EqualityComparer<IVolume?>.Default.Equals(From, other.From) &&
                   EqualityComparer<IVolume?>.Default.Equals(To, other.To);
        }

        public readonly float GetBlendValue(long now)
        {
            long elapsed = now - Start;
            float blendValue = elapsed / (float)Duration;
            return MathUtil.Clamp01(blendValue);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Start, Duration, From, To);
        }

        public static bool operator ==(VolumeTransition left, VolumeTransition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VolumeTransition left, VolumeTransition right)
        {
            return !(left == right);
        }
    }

    public delegate void VolumeTransitionEventHandler(VolumeTransition transition);

    public delegate void VolumeTransitionTickEventHandler(VolumeTransition transition, float value);

    public class DelegateList<T> where T : Delegate
    {
        private readonly List<T> handlers = new();

        public IReadOnlyList<T> Handlers => handlers;

        public int Count => handlers.Count;

        public T this[int index]
        {
            get => handlers[index];
        }

        public void AddHandler(T handler)
        {
            lock (handlers)
            {
                handlers.Add(handler);
            }
        }

        public void RemoveHandler(T handler)
        {
            lock (handlers)
            {
                handlers.Remove(handler);
            }
        }

        public void Clear()
        {
            handlers.Clear();
        }
    }

    public class VolumeSystem : ISceneSystem
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(VolumeSystem));
        private PostProcessingManager postManager;
        private readonly ObjectTypeQuery<Volume> volumes = new();
        private Volume? activeVolume;

        private readonly List<VolumeTransition> transitions = [];

        public string Name { get; } = "VolumeSystem";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.Destroy | SystemFlags.Update;

        public Volume? ActiveVolume => activeVolume;

        private readonly DelegateList<VolumeTransitionEventHandler> transitionStartList = new();
        private readonly DelegateList<VolumeTransitionEventHandler> transitionEndList = new();
        private readonly DelegateList<VolumeTransitionEventHandler> transitionAbortedList = new();
        private readonly DelegateList<VolumeTransitionTickEventHandler> transitionTickList = new();

        public event VolumeTransitionEventHandler TransitionStart
        {
            add => transitionStartList.AddHandler(value);
            remove => transitionStartList.RemoveHandler(value);
        }

        public event VolumeTransitionEventHandler TransitionEnd
        {
            add => transitionEndList.AddHandler(value);
            remove => transitionEndList.RemoveHandler(value);
        }

        public event VolumeTransitionEventHandler TransitionAborted
        {
            add => transitionAbortedList.AddHandler(value);
            remove => transitionAbortedList.RemoveHandler(value);
        }

        public event VolumeTransitionTickEventHandler TransitionTick
        {
            add => transitionTickList.AddHandler(value);
            remove => transitionTickList.RemoveHandler(value);
        }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(volumes);

            volumes.OnAdded += OnAdded;
            volumes.OnRemoved += OnRemoved;

            postManager = PostProcessingManager.Current ?? throw new Exception("Cannot initialize without post fx");

            //SetDefaults();

            bool hasGlobal = false;
            for (int i = 0; i < volumes.Count; i++)
            {
                var volume = volumes[i];
                ((IVolume)volume).Init(postManager);
                if (volume.Mode == VolumeMode.Global)
                {
                    if (hasGlobal)
                    {
                        Logger.Warn("Multiple global Volumes where found, first found will be used.");
                    }
                    hasGlobal = true;
                }
                else
                {
                    for (int j = i + 1; j < volumes.Count; j++)
                    {
                        bool intersects = false;
                        var other = volumes[j];
                        switch (other.Shape)
                        {
                            case VolumeShape.Box:
                                switch (volume.Shape)
                                {
                                    case VolumeShape.Box:
                                        intersects = other.BoundingBox.Intersects(volume.BoundingBox);
                                        break;

                                    case VolumeShape.Sphere:
                                        intersects = other.BoundingBox.Intersects(volume.BoundingSphere);
                                        break;
                                }
                                break;

                            case VolumeShape.Sphere:
                                switch (volume.Shape)
                                {
                                    case VolumeShape.Box:
                                        intersects = other.BoundingSphere.Intersects(volume.BoundingBox);
                                        break;

                                    case VolumeShape.Sphere:
                                        intersects = other.BoundingSphere.Intersects(volume.BoundingSphere);
                                        break;
                                }
                                break;
                        }

                        if (intersects)
                        {
                            Logger.Warn($"Volume '{volume}' and Volume '{other}' are overlapping, this can cause visual issues.");
                        }
                    }
                }
            }
        }

        private void OnRemoved(Volume volume)
        {
        }

        private void OnAdded(Volume volume)
        {
            ((IVolume)volume).Init(postManager);
        }

        public void TransitionTo(long now, int duration, VolumeTransitionMode transitionMode, Volume? from, Volume? to)
        {
            int index = -1;
            VolumeTransition existingTransition = default;
            for (int i = 0; i < transitions.Count; i++)
            {
                var trans = transitions[i];
                if (trans.From == to && trans.To == from)
                {
                    index = i;
                    existingTransition = trans;
                    break;
                }
            }

            if (existingTransition != default)
            {
                float reverseBlend = 1 - existingTransition.GetBlendValue(now);
                long remainingTicks = (long)(existingTransition.Duration * reverseBlend);
                now -= remainingTicks;
                transitions.RemoveAt(index);
                for (int i = 0; i < transitionAbortedList.Count; i++)
                {
                    transitionAbortedList[i](existingTransition);
                }
            }

            long durationInTicks = (long)(duration / 1000.0f * Stopwatch.Frequency);

            if (transitionMode == VolumeTransitionMode.Constant)
            {
                durationInTicks = 0; // simply set the duration to 0 this simply disables blending.
            }

            VolumeTransition transition = new(now, durationInTicks, from, to, transitionMode);

            transitions.Add(transition);
            for (int i = 0; i < transitionStartList.Count; i++)
            {
                transitionStartList[i](transition);
            }
        }

        public void Update(float delta)
        {
            var camera = CameraManager.Current;

            if (camera == null)
            {
                return;
            }

            Vector3 camPos = camera.Transform.GlobalPosition;

            Volume? global = null;
            Volume? local = null;
            for (int i = 0; i < volumes.Count; i++)
            {
                var volume = volumes[i];

                switch (volume.Mode)
                {
                    case VolumeMode.Global:
                        global ??= volume;
                        break;

                    case VolumeMode.Local:
                        break;
                }

                bool intersects = false;
                switch (volume.Shape)
                {
                    case VolumeShape.Box:
                        var box = BoundingBox.Transform(volume.BoundingBox, volume.Transform);
                        intersects = box.Contains(camPos) != ContainmentType.Disjoint;
                        break;

                    case VolumeShape.Sphere:
                        var sphere = BoundingSphere.Transform(volume.BoundingSphere, volume.Transform);
                        intersects = sphere.Contains(camPos) != ContainmentType.Disjoint;
                        break;
                }

                if (intersects)
                {
                    local = volume;
                    break;
                }
            }

            Volume? newActiveVolume = local ?? global;

            long now = Stopwatch.GetTimestamp();

            if (newActiveVolume != activeVolume)
            {
                TransitionTo(now, newActiveVolume?.TransitionDuration ?? 0, newActiveVolume?.TransitionMode ?? 0, activeVolume, newActiveVolume);
                activeVolume = newActiveVolume;
            }

            for (int i = 0; i < transitions.Count; i++)
            {
                var transition = transitions[i];
                var blend = transition.GetBlendValue(now);

                for (int j = 0; j < transitionTickList.Count; i++)
                {
                    transitionTickList[j](transition, blend);
                }

                bool remove;
                if (transition.To == null)
                {
                    postManager.ResetSettings();
                    remove = true;
                }
                else
                {
                    if (transition.From == null)
                    {
                        transition.To.Apply(postManager);
                        remove = true;
                    }
                    else
                    {
                        transition.To.Apply(postManager, transition.From, blend, transition.Mode);
                        remove = blend == 1;
                    }
                }

                if (remove)
                {
                    transitions.RemoveAt(i);
                    i--;

                    for (int j = 0; j < transitionEndList.Count; i++)
                    {
                        transitionEndList[j](transition);
                    }
                }
            }
        }

        public void Destroy()
        {
            volumes.Dispose();
            postManager = null;
            transitionStartList.Clear();
            transitionEndList.Clear();
            transitionAbortedList.Clear();
            transitionTickList.Clear();
        }
    }
}