﻿namespace HexaEngine.Objects
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Lights.Probes;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes.Components.Renderer;

    [EditorGameObject<Skybox>("Skybox")]
    public class Skybox : GameObject
    {
        public Skybox()
        {
            AddComponent(new SkyboxRendererComponent());
            AddComponent(new IBLLightProbeComponent());
        }

        [JsonConstructor]
        public Skybox(string name)
        {
            Name = name;
        }
    }
}