namespace HexaEngine.Core.Lights
{
    using System.Collections.Generic;
    using System.Numerics;

    public unsafe struct CBLight
    {
        public static readonly int DirectionalLightSDPointerOffset = 0;
        public static readonly int PointLightSDPointerOffset = MaxDirectionalLightSDs * sizeof(CBDirectionalLightSD) + sizeof(Vector4);
        public static readonly int SpotlightSDPointerOffset = PointLightSDPointerOffset + MaxPointLightSDs * sizeof(CBPointLightSD) + sizeof(Vector4);
        public static readonly int DirectionalLightPointerOffset = SpotlightSDPointerOffset + MaxSpotlightSDs * sizeof(CBSpotlightSD) + sizeof(Vector4);
        public static readonly int PointLightPointerOffset = DirectionalLightPointerOffset + MaxDirectionalLights * sizeof(CBDirectionalLight) + sizeof(Vector4);
        public static readonly int SpotlightPointerOffset = PointLightPointerOffset + MaxPointLights * sizeof(CBPointLight) + sizeof(Vector4);

        public CBDirectionalLightSD DirectionalLightSD1;
        public int DirectionalLightSDCount;
        public Vector3 padd1;

        public CBPointLightSD PointLightSD1;
        public CBPointLightSD PointLightSD2;
        public CBPointLightSD PointLightSD3;
        public CBPointLightSD PointLightSD4;
        public CBPointLightSD PointLightSD5;
        public CBPointLightSD PointLightSD6;
        public CBPointLightSD PointLightSD7;
        public CBPointLightSD PointLightSD8;

        public int PointLightSDCount;
        public Vector3 padd2;

        public CBSpotlightSD SpotlightSD1;
        public CBSpotlightSD SpotlightSD2;
        public CBSpotlightSD SpotlightSD3;
        public CBSpotlightSD SpotlightSD4;
        public CBSpotlightSD SpotlightSD5;
        public CBSpotlightSD SpotlightSD6;
        public CBSpotlightSD SpotlightSD7;
        public CBSpotlightSD SpotlightSD8;

        public int SpotlightSDCount;
        public Vector3 padd5;

        public CBDirectionalLight DirectionalLight1;
        public CBDirectionalLight DirectionalLight2;
        public CBDirectionalLight DirectionalLight3;
        public CBDirectionalLight DirectionalLight4;

        public int DirectionalLightCount;
        public Vector3 padd3;

        public CBPointLight PointLight1;
        public CBPointLight PointLight2;
        public CBPointLight PointLight3;
        public CBPointLight PointLight4;
        public CBPointLight PointLight5;
        public CBPointLight PointLight6;
        public CBPointLight PointLight7;
        public CBPointLight PointLight8;
        public CBPointLight PointLight9;
        public CBPointLight PointLight10;
        public CBPointLight PointLight11;
        public CBPointLight PointLight12;
        public CBPointLight PointLight13;
        public CBPointLight PointLight14;
        public CBPointLight PointLight15;
        public CBPointLight PointLight16;
        public CBPointLight PointLight17;
        public CBPointLight PointLight18;
        public CBPointLight PointLight19;
        public CBPointLight PointLight20;
        public CBPointLight PointLight21;
        public CBPointLight PointLight22;
        public CBPointLight PointLight23;
        public CBPointLight PointLight24;
        public CBPointLight PointLight25;
        public CBPointLight PointLight26;
        public CBPointLight PointLight27;
        public CBPointLight PointLight28;
        public CBPointLight PointLight29;
        public CBPointLight PointLight30;
        public CBPointLight PointLight31;
        public CBPointLight PointLight32;

        public int PointLightCount;
        public Vector3 padd4;

        public CBSpotlight Spotlight1;
        public CBSpotlight Spotlight2;
        public CBSpotlight Spotlight3;
        public CBSpotlight Spotlight4;
        public CBSpotlight Spotlight5;
        public CBSpotlight Spotlight6;
        public CBSpotlight Spotlight7;
        public CBSpotlight Spotlight8;
        public CBSpotlight Spotlight9;
        public CBSpotlight Spotlight10;
        public CBSpotlight Spotlight11;
        public CBSpotlight Spotlight12;
        public CBSpotlight Spotlight13;
        public CBSpotlight Spotlight14;
        public CBSpotlight Spotlight15;
        public CBSpotlight Spotlight16;
        public CBSpotlight Spotlight17;
        public CBSpotlight Spotlight18;
        public CBSpotlight Spotlight19;
        public CBSpotlight Spotlight20;
        public CBSpotlight Spotlight21;
        public CBSpotlight Spotlight22;
        public CBSpotlight Spotlight23;
        public CBSpotlight Spotlight24;
        public CBSpotlight Spotlight25;
        public CBSpotlight Spotlight26;
        public CBSpotlight Spotlight27;
        public CBSpotlight Spotlight28;
        public CBSpotlight Spotlight29;
        public CBSpotlight Spotlight30;
        public CBSpotlight Spotlight31;
        public CBSpotlight Spotlight32;

        public int SpotlightCount;
        public Vector3 padd6;

        public const int MaxDirectionalLightSDs = 1;
        public const int MaxPointLightSDs = 8;
        public const int MaxSpotlightSDs = 8;
        public const int MaxDirectionalLights = 4;
        public const int MaxPointLights = 32;
        public const int MaxSpotlights = 32;

        public CBLight()
        {
            DirectionalLightSDCount = 0;
            DirectionalLightCount = 0;
            PointLightSDCount = 0;
            PointLightCount = 0;
            SpotlightSDCount = 0;
            SpotlightCount = 0;
            padd1 = default;
            padd2 = default;
            padd3 = default;
            padd4 = default;
            padd5 = default;
            padd6 = default;
        }

        public static unsafe void Update(CBLight* cb, IReadOnlyList<Light> lights)
        {
            var directionalLightSDCount = 0;
            var directionalLightCount = 0;
            var pointLightSDCount = 0;
            var pointLightCount = 0;
            var spotlightSDCount = 0;
            var spotlightCount = 0;

            byte* ptr = (byte*)cb;
            CBDirectionalLightSD* DirectionalLightSDs = (CBDirectionalLightSD*)(ptr + DirectionalLightSDPointerOffset);
            CBDirectionalLight* DirectionalLights = (CBDirectionalLight*)(ptr + DirectionalLightPointerOffset);
            CBPointLightSD* PointLightSDs = (CBPointLightSD*)(ptr + PointLightSDPointerOffset);
            CBPointLight* PointLights = (CBPointLight*)(ptr + PointLightPointerOffset);
            CBSpotlightSD* SpotlightSDs = (CBSpotlightSD*)(ptr + SpotlightSDPointerOffset);
            CBSpotlight* Spotlights = (CBSpotlight*)(ptr + SpotlightPointerOffset);

            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                if (light is DirectionalLight directional)
                {
                    if (directional.CastShadows && directionalLightSDCount != MaxDirectionalLightSDs)
                    {
                        DirectionalLightSDs[directionalLightSDCount].Update(directional);
                        directionalLightSDCount++;
                    }
                    else if (directionalLightCount != MaxDirectionalLights)
                    {
                        DirectionalLights[directionalLightCount].Update(directional);
                        directionalLightCount++;
                    }
                }
                if (light is PointLight point)
                {
                    if (point.CastShadows && pointLightSDCount != MaxPointLightSDs)
                    {
                        PointLightSDs[pointLightSDCount].Update(point);
                        pointLightSDCount++;
                    }
                    else if (pointLightCount != MaxPointLights)
                    {
                        PointLights[pointLightCount].Update(point);
                        pointLightCount++;
                    }
                }
                if (light is Spotlight spotlight)
                {
                    if (spotlight.CastShadows && spotlightSDCount != MaxSpotlightSDs)
                    {
                        SpotlightSDs[spotlightSDCount].Update(spotlight);
                        spotlightSDCount++;
                    }
                    else if (spotlightCount != MaxSpotlights)
                    {
                        Spotlights[spotlightCount].Update(spotlight);
                        spotlightCount++;
                    }
                }
            }

            cb->DirectionalLightSDCount = directionalLightSDCount;
            cb->DirectionalLightCount = directionalLightCount;
            cb->PointLightSDCount = pointLightSDCount;
            cb->PointLightCount = pointLightCount;
            cb->SpotlightSDCount = spotlightSDCount;
            cb->SpotlightCount = spotlightCount;
        }

        public CBSpotlightSD* GetSpotlightSDs()
        {
            fixed (void* ptr = &this)
            {
                return (CBSpotlightSD*)((byte*)ptr + SpotlightSDPointerOffset);
            }
        }

        public CBDirectionalLightSD* GetDirectionalLightSDs()
        {
            fixed (void* ptr = &this)
            {
                return (CBDirectionalLightSD*)ptr;
            }
        }
    }
}