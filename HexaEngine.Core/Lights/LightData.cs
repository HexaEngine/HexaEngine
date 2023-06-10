namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Scenes;
    using System.Numerics;

    public struct LightData
    {
        /**
        * Position for point and spot lights (World space).
        */
        public Vector4 PositionWS;
        //--------------------------------------------------------------( 16 bytes )
        /**
        * Direction for spot and directional lights (World space).
        */
        public Vector4 DirectionWS;
        //--------------------------------------------------------------( 16 bytes )
        /**
        * Position for point and spot lights (View space).
        */
        public Vector4 PositionVS;
        //--------------------------------------------------------------( 16 bytes )
        /**
        * Direction for spot and directional lights (View space).
        */
        public Vector4 DirectionVS;
        //--------------------------------------------------------------( 16 bytes )
        /**
        * Color of the light. Diffuse and specular colors are not seperated.
        */
        public Vector4 Color;
        //--------------------------------------------------------------( 16 bytes )
        /**
        * The half angle of the spotlight cone.
        */
        public float SpotlightAngle;
        /**
        * The range of the light.
        */
        public float Range;

        /**
         * The intensity of the light.
         */
        public float Intensity;

        /**
        * Disable or enable the light.
        */
        public uint Enabled;
        //--------------------------------------------------------------( 16 bytes )

        /**
         * Is the light selected in the editor?
         */
        public uint Selected;

        /**
        * The type of the light.
        */
        public uint Type;
        public Vector2 Padding;

        public LightData(Light light, Camera camera) : this()
        {
            PositionWS = new(light.Transform.GlobalPosition, 1);
            PositionVS = Vector4.Transform(PositionWS, camera.Transform.View);
            DirectionWS = new(light.Transform.Forward, 1);
            DirectionVS = Vector4.Transform(DirectionWS, camera.Transform.View);
            Color = light.Color;
            Range = light.Range;
            Intensity = light.Intensity;
            Enabled = light.IsEnabled ? 1u : 0u;
            Selected = light.IsEditorSelected ? 1u : 0u;
            Type = (uint)light.LightType;
            if (light is Spotlight spotlight)
            {
                SpotlightAngle = spotlight.ConeAngle / 2;
            }
        }

        //--------------------------------------------------------------( 16 bytes )
        //--------------------------------------------------------------( 16 * 7 = 112 bytes )
    }
}