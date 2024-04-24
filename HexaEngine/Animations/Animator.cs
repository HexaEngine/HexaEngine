namespace HexaEngine.Animations
{
    using HexaEngine.Components.Physics;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    [EditorComponent(typeof(Animator), "Animator")]
    public class Animator : IComponent
    {
        private bool playing;
        private bool isDirty = true;

        private readonly List<AnimatorParameter> parameters = [];
        private readonly Dictionary<string, AnimatorParameter> nameToParameter = [];

        private readonly List<Animation> animations = [];
        private Animation? lastAnimation;
        private Animation? currentAnimation;
        private DateTime transitionStart;
        private DateTime transitionEnd;
        private readonly AnimatorStateMachine stateMachine = new();

        private SkinnedMeshRendererComponent? renderer;

        /// <summary>
        /// The GUID of the <see cref="Animator"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        [JsonIgnore]
        public bool IsSerializable { get; } = true;

        [JsonIgnore]
        public AnimatorStateMachine StateMachine => stateMachine;

        public event Action<Animation>? Playing;

        public AnimatorParameter? GetParameter(string name)
        {
            if (nameToParameter.TryGetValue(name, out var parameter))
            {
                return parameter;
            }
            return null;
        }

        public bool TryGetParameter(string name, [NotNullWhen(true)] out AnimatorParameter? parameter)
        {
            return nameToParameter.TryGetValue(name, out parameter);
        }

        public bool HasParameter(string name)
        {
            return nameToParameter.ContainsKey(name);
        }

        public bool ContainsParameter(AnimatorParameter parameter)
        {
            return parameters.Contains(parameter);
        }

        public void AddParameter(AnimatorParameter parameter)
        {
            parameters.Add(parameter);
            nameToParameter.Add(parameter.Name, parameter);
            parameter.Value = parameter.DefaultValue;
            isDirty = true;
        }

        public void RemoveParameter(AnimatorParameter parameter)
        {
            nameToParameter.Remove(parameter.Name);
            parameters.Remove(parameter);
            isDirty = true;
        }

        public void SetFloat(string name, float value)
        {
            if (!TryGetParameter(name, out var parameter))
            {
                throw new ArgumentException($"Could not find parameter with the name {name}", nameof(name));
            }

            if (parameter.Type != AnimatorParameterType.Float)
            {
                throw new ArgumentException($"Parameter {name} is not type of float, parameter type: {parameter.Type}", nameof(name));
            }

            parameter.Value = value;
            isDirty = true;
        }

        public void SetInt(string name, int value)
        {
            if (!TryGetParameter(name, out var parameter))
            {
                throw new ArgumentException($"Could not find parameter with the name {name}", nameof(name));
            }

            if (parameter.Type != AnimatorParameterType.Int)
            {
                throw new ArgumentException($"Parameter {name} is not type of int, parameter type: {parameter.Type}", nameof(name));
            }

            parameter.Value = value;
            isDirty = true;
        }

        public void SetBool(string name, bool value)
        {
            if (!TryGetParameter(name, out var parameter))
            {
                throw new ArgumentException($"Could not find parameter with the name {name}", nameof(name));
            }

            if (parameter.Type != AnimatorParameterType.Bool)
            {
                throw new ArgumentException($"Parameter {name} is not type of bool, parameter type: {parameter.Type}", nameof(name));
            }

            parameter.Value = value;
            isDirty = true;
        }

        public void Trigger(string name)
        {
            if (!TryGetParameter(name, out var parameter))
            {
                throw new ArgumentException($"Could not find parameter with the name {name}", nameof(name));
            }

            if (parameter.Type != AnimatorParameterType.Trigger)
            {
                throw new ArgumentException($"Parameter {name} is not type of trigger, parameter type: {parameter.Type}", nameof(name));
            }

            parameter.Value = true;
            isDirty = true;
        }

        public float GetFloat(string name)
        {
            if (!TryGetParameter(name, out var parameter))
            {
                throw new ArgumentException($"Could not find parameter with the name {name}", nameof(name));
            }

            if (parameter.Type != AnimatorParameterType.Float)
            {
                throw new ArgumentException($"Parameter {name} is not type of float, parameter type: {parameter.Type}", nameof(name));
            }

            return (float)parameter.Value;
        }

        public int GetInt(string name)
        {
            if (!TryGetParameter(name, out var parameter))
            {
                throw new ArgumentException($"Could not find parameter with the name {name}", nameof(name));
            }

            if (parameter.Type != AnimatorParameterType.Int)
            {
                throw new ArgumentException($"Parameter {name} is not type of int, parameter type: {parameter.Type}", nameof(name));
            }

            return (int)parameter.Value;
        }

        public bool GetBool(string name)
        {
            if (TryGetParameter(name, out var parameter))
            {
                if (parameter.Type != AnimatorParameterType.Bool)
                {
                    throw new ArgumentException($"Parameter {name} is not type of bool, parameter type: {parameter.Type}", nameof(name));
                }
                return (bool)parameter.Value;
            }
            throw new ArgumentException($"Could not find parameter with the name {name}", nameof(name));
        }

        public Matrix4x4 GetBoneLocal(string name)
        {
            if (renderer == null)
            {
                return Matrix4x4.Identity;
            }

            var boneId = renderer.GetBoneIdByName(name);

            if (boneId == -1)
            {
                return Matrix4x4.Identity;
            }

            return renderer.GetBoneLocal((uint)boneId);
        }

        public Matrix4x4 GetLocal(string name)
        {
            if (renderer == null)
            {
                return Matrix4x4.Identity;
            }

            var nodeId = renderer.GetNodeIdByName(name);

            if (nodeId == -1)
            {
                return Matrix4x4.Identity;
            }

            return renderer.GetLocal((uint)nodeId);
        }

        public void Awake()
        {
            renderer = GameObject.GetComponent<SkinnedMeshRendererComponent>();
            stateMachine.Transition += Transition;
            stateMachine.StateChanged += StateChanged;
        }

        public void Destroy()
        {
            Stop();
        }

        public void Play(string name)
        {
            playing = true;
        }

        public void Pause()
        {
            playing = false;
        }

        public void Resume()
        {
            playing = true;
        }

        public void Stop()
        {
            playing = false;
        }

        private void Transition(AnimatorTransition obj)
        {
            transitionStart = DateTime.Now;
            transitionEnd = transitionStart + TimeSpan.FromSeconds(obj.Transition.Duration);
        }

        private void StateChanged(AnimatorState obj)
        {
            lastAnimation = currentAnimation;
            currentAnimation = (Animation)obj.Motion;
            currentAnimation.Reset();
            playing = true;
            Playing?.Invoke(currentAnimation);
        }

        public void Update(float deltaTime)
        {
            if (!playing)
            {
                return;
            }

            if (isDirty)
            {
                stateMachine.UpdateState(parameters);
            }

            if (currentAnimation == null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            float blend = (float)((now - transitionStart) / (transitionEnd - transitionStart));

            if (blend < 0f || blend >= 1f)
            {
                lastAnimation = null;
            }

            playing = currentAnimation.Tick(deltaTime);
            if (lastAnimation != null)
            {
                for (int i = 0; i < currentAnimation.NodeChannels.Count; i++)
                {
                    var currentChannel = currentAnimation.NodeChannels[i];
                    var local = BlendChannel(lastAnimation, currentAnimation, i, blend);
                    if (currentChannel.Node.IsBone)
                    {
                        renderer.SetBoneLocal(local, currentChannel.Node.Id);
                    }
                    else
                    {
                        renderer.SetLocal(local, currentChannel.Node.Id);
                    }
                }
            }
            else
            {
                for (int i = 0; i < currentAnimation.NodeChannels.Count; i++)
                {
                    var channel = currentAnimation.NodeChannels[i];
                    if (channel.Node.IsBone)
                    {
                        renderer.SetBoneLocal(channel.Local, channel.Node.Id);
                    }
                    else
                    {
                        renderer.SetLocal(channel.Local, channel.Node.Id);
                    }
                }
            }
        }

        private static Matrix4x4 BlendChannel(Animation a, Animation b, int i, float value)
        {
            var channelA = a.NodeChannels[i];
            var channelB = b.NodeChannels[i];

            var localA = channelA.Local;
            var localB = channelB.Local;

            Matrix4x4.Decompose(localA, out Vector3 scaleA, out Quaternion rotationA, out Vector3 translationA);
            Matrix4x4.Decompose(localB, out Vector3 scaleB, out Quaternion rotationB, out Vector3 translationB);

            Vector3 scale = Vector3.Lerp(scaleA, scaleB, value);
            Quaternion rotation = Quaternion.Slerp(rotationA, rotationB, value);
            Vector3 translation = Vector3.Lerp(translationA, translationB, value);

            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
        }
    }
}