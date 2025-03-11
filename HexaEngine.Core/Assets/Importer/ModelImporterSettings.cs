namespace HexaEngine.Core.Assets.Importer
{
    using Hexa.NET.Assimp;
    using HexaEngine.Editor.Attributes;

    public class ModelImporterSettings
    {
        public AiPostProcessSteps PostProcessSteps { get; set; } =
            AiPostProcessSteps.FlipUVs |
            AiPostProcessSteps.CalcTangentSpace |
            AiPostProcessSteps.MakeLeftHanded |
            AiPostProcessSteps.FindInvalidData |
            AiPostProcessSteps.FindDegenerates |
            AiPostProcessSteps.ImproveCacheLocality |
            AiPostProcessSteps.Triangulate |
            AiPostProcessSteps.FindInstances |
            AiPostProcessSteps.LimitBoneWeights |
            AiPostProcessSteps.RemoveRedundantMaterials;

        [JsonIgnore]
        [EditorProperty("Optimize Meshes")]
        [EditorCategory("Optimization")]
        public bool OptimizeMeshes
        {
            get => (PostProcessSteps & AiPostProcessSteps.OptimizeMeshes) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.OptimizeMeshes;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.OptimizeMeshes;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Optimize Graph")]
        [EditorCategory("Optimization")]
        public bool OptimizeGraph
        {
            get => (PostProcessSteps & AiPostProcessSteps.OptimizeGraph) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.OptimizeGraph;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.OptimizeGraph;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Split Large Meshes")]
        [EditorCategory("Optimization")]
        public bool SplitLargeMeshes
        {
            get => (PostProcessSteps & AiPostProcessSteps.SplitLargeMeshes) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.SplitLargeMeshes;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.SplitLargeMeshes;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Remove Component")]
        [EditorCategory("Normals and UVs")]
        public bool RemoveComponent
        {
            get => (PostProcessSteps & AiPostProcessSteps.RemoveComponent) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.RemoveComponent;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.RemoveComponent;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Generate Normals")]
        [EditorCategory("Normals and UVs")]
        public bool GenerateNormals
        {
            get => (PostProcessSteps & AiPostProcessSteps.GenNormals) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.GenNormals;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.GenNormals;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Generate Smooth Normals")]
        [EditorCategory("Normals and UVs")]
        public bool GenerateSmoothNormals
        {
            get => (PostProcessSteps & AiPostProcessSteps.GenSmoothNormals) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.GenSmoothNormals;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.GenSmoothNormals;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Calculate Tangent Space")]
        [EditorCategory("Normals and UVs")]
        public bool CalculateTangentSpace
        {
            get => (PostProcessSteps & AiPostProcessSteps.CalcTangentSpace) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.CalcTangentSpace;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.CalcTangentSpace;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Generate UV Coords")]
        [EditorCategory("Normals and UVs")]
        public bool GenerateUVCoords
        {
            get => (PostProcessSteps & AiPostProcessSteps.GenUvCoords) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.GenUvCoords;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.GenUvCoords;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Flip UVs")]
        [EditorCategory("Normals and UVs")]
        public bool FlipUVs
        {
            get => (PostProcessSteps & AiPostProcessSteps.FlipUVs) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.FlipUVs;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.FlipUVs;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Fix In Facing Normals")]
        [EditorCategory("Normals and UVs")]
        public bool FixInFacingNormals
        {
            get => (PostProcessSteps & AiPostProcessSteps.FixInfacingNormals) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.FixInfacingNormals;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.FixInfacingNormals;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Join Identical Vertices")]
        [EditorCategory("Geometry")]
        public bool JoinIdenticalVertices
        {
            get => (PostProcessSteps & AiPostProcessSteps.JoinIdenticalVertices) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.JoinIdenticalVertices;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.JoinIdenticalVertices;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Make Left Handed")]
        [EditorCategory("Geometry")]
        public bool MakeLeftHanded
        {
            get => (PostProcessSteps & AiPostProcessSteps.MakeLeftHanded) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.MakeLeftHanded;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.MakeLeftHanded;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Triangulate")]
        [EditorCategory("Geometry")]
        public bool Triangulate
        {
            get => (PostProcessSteps & AiPostProcessSteps.Triangulate) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.Triangulate;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.Triangulate;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Flip Winding Order")]
        [EditorCategory("Geometry")]
        public bool FlipWindingOrder
        {
            get => (PostProcessSteps & AiPostProcessSteps.FlipWindingOrder) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.FlipWindingOrder;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.FlipWindingOrder;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Debone")]
        [EditorCategory("Geometry")]
        public bool Debone
        {
            get => (PostProcessSteps & AiPostProcessSteps.Debone) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.Debone;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.Debone;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Remove Redundant Materials")]
        [EditorCategory("Misc")]
        public bool RemoveRedundantMaterials
        {
            get => (PostProcessSteps & AiPostProcessSteps.RemoveRedundantMaterials) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= AiPostProcessSteps.RemoveRedundantMaterials;
                }
                else
                {
                    PostProcessSteps &= ~AiPostProcessSteps.RemoveRedundantMaterials;
                }
            }
        }

        [EditorProperty("Import Materials")]
        public bool ImportMaterials { get; set; } = true;

        [EditorProperty("Import Textures")]
        public bool ImportTextures { get; set; } = true;

        [EditorProperty("Import Animation Clips")]
        public bool ImportAnimationClips { get; set; } = true;

        [EditorProperty("Texture Settings")]
        public TextureImporterSettings TextureSettings { get; set; } = new();
    }
}