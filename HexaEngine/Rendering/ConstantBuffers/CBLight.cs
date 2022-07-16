namespace HexaEngine.Rendering.ConstantBuffers
{
    using HexaEngine.Lights;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct CBLight
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDirectionalLightSDs)]
        public CBDirectionalLightSD[] DirectionalLightSDs;

        public int DirectionalLightSDCount;
        public Vector3 padd1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPointLightSDs)]
        public CBPointLightSD[] PointLightSDs;

        public int PointLightSDCount;
        public Vector3 padd2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDirectionalLights)]
        public CBDirectionalLight[] DirectionalLights;

        public int DirectionalLightCount;
        public Vector3 padd3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPointLights)]
        public CBPointLight[] PointLights;

        public int PointLightCount;
        public Vector3 padd4;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSpotlightSDs)]
        public CBSpotlightSD[] SpotlightSDs;

        public int SpotlightSDCount;
        public Vector3 padd5;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSpotlights)]
        public CBSpotlight[] Spotlights;

        public int SpotlightCount;
        public Vector3 padd6;

        public const int MaxDirectionalLightSDs = 1;
        public const int MaxPointLightSDs = 8;
        public const int MaxSpotlightSDs = 8;
        public const int MaxDirectionalLights = 4;
        public const int MaxPointLights = 32;
        public const int MaxSpotlights = 32;

        public CBLight(Light[] lights)
        {
            DirectionalLightSDs = new CBDirectionalLightSD[MaxDirectionalLightSDs];
            DirectionalLightSDCount = 0;
            DirectionalLights = new CBDirectionalLight[MaxDirectionalLights];
            DirectionalLightCount = 0;
            PointLightSDs = new CBPointLightSD[MaxPointLightSDs];
            PointLightSDCount = 0;
            PointLights = new CBPointLight[MaxPointLights];
            PointLightCount = 0;
            SpotlightSDs = new CBSpotlightSD[MaxSpotlightSDs];
            SpotlightSDCount = 0;
            Spotlights = new CBSpotlight[MaxSpotlights];
            SpotlightCount = 0;
            padd1 = default;
            padd2 = default;
            padd3 = default;
            padd4 = default;
            padd5 = default;
            padd6 = default;

            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light is DirectionalLight directional)
                {
                    if (directional.CastShadows && DirectionalLightSDCount != MaxDirectionalLightSDs)
                    {
                        DirectionalLightSDs[DirectionalLightSDCount] = new(directional);
                        DirectionalLightSDCount++;
                    }
                    else if (DirectionalLightCount != MaxDirectionalLights)
                    {
                        DirectionalLights[DirectionalLightCount] = new(directional);
                        DirectionalLightCount++;
                    }
                }
                if (light is PointLight point)
                {
                    if (point.CastShadows && PointLightSDCount != MaxPointLightSDs)
                    {
                        PointLightSDs[PointLightSDCount] = new(point);
                        PointLightSDCount++;
                    }
                    else if (PointLightCount != MaxPointLights)
                    {
                        PointLights[PointLightCount] = new(point);
                        PointLightCount++;
                    }
                }
                if (light is Spotlight spotlight)
                {
                    if (spotlight.CastShadows && SpotlightSDCount != MaxSpotlightSDs)
                    {
                        SpotlightSDs[SpotlightSDCount] = new(spotlight);
                        SpotlightSDCount++;
                    }
                    else if (SpotlightCount != MaxSpotlights)
                    {
                        Spotlights[SpotlightCount] = new(spotlight);
                        SpotlightCount++;
                    }
                }
            }
        }

        public CBLight(IReadOnlyList<Light> lights)
        {
            DirectionalLightSDs = new CBDirectionalLightSD[MaxDirectionalLightSDs];
            DirectionalLightSDCount = 0;
            DirectionalLights = new CBDirectionalLight[MaxDirectionalLights];
            DirectionalLightCount = 0;
            PointLightSDs = new CBPointLightSD[MaxPointLightSDs];
            PointLightSDCount = 0;
            PointLights = new CBPointLight[MaxPointLights];
            PointLightCount = 0;
            SpotlightSDs = new CBSpotlightSD[MaxSpotlightSDs];
            SpotlightSDCount = 0;
            Spotlights = new CBSpotlight[MaxSpotlights];
            SpotlightCount = 0;
            padd1 = default;
            padd2 = default;
            padd3 = default;
            padd4 = default;
            padd5 = default;
            padd6 = default;

            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                if (light is DirectionalLight directional)
                {
                    if (directional.CastShadows && DirectionalLightSDCount != MaxDirectionalLightSDs)
                    {
                        DirectionalLightSDs[DirectionalLightSDCount] = new(directional);
                        DirectionalLightSDCount++;
                    }
                    else if (DirectionalLightCount != MaxDirectionalLights)
                    {
                        DirectionalLights[DirectionalLightCount] = new(directional);
                        DirectionalLightCount++;
                    }
                }
                if (light is PointLight point)
                {
                    if (point.CastShadows && PointLightSDCount != MaxPointLightSDs)
                    {
                        PointLightSDs[PointLightSDCount] = new(point);
                        PointLightSDCount++;
                    }
                    else if (PointLightCount != MaxPointLights)
                    {
                        PointLights[PointLightCount] = new(point);
                        PointLightCount++;
                    }
                }
                if (light is Spotlight spotlight)
                {
                    if (spotlight.CastShadows && SpotlightSDCount != MaxSpotlightSDs)
                    {
                        SpotlightSDs[SpotlightSDCount] = new(spotlight);
                        SpotlightSDCount++;
                    }
                    else if (SpotlightCount != MaxSpotlights)
                    {
                        Spotlights[SpotlightCount] = new(spotlight);
                        SpotlightCount++;
                    }
                }
            }
        }
    }
}