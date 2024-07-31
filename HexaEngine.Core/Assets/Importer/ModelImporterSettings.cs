namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Editor.Attributes;
    using Silk.NET.Assimp;

    public class ModelImporterSettings
    {
        public PostProcessSteps PostProcessSteps { get; set; } =
            PostProcessSteps.FlipUVs |
            PostProcessSteps.CalculateTangentSpace |
            PostProcessSteps.MakeLeftHanded |
            PostProcessSteps.FindInvalidData |
            PostProcessSteps.FindDegenerates |
            PostProcessSteps.ImproveCacheLocality |
            PostProcessSteps.Triangulate |
            PostProcessSteps.FindInstances |
            PostProcessSteps.LimitBoneWeights;

        [JsonIgnore]
        [EditorProperty("Optimize Meshes")]
        [EditorCategory("Optimization")]
        public bool OptimizeMeshes
        {
            get => (PostProcessSteps & PostProcessSteps.OptimizeMeshes) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.OptimizeMeshes;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.OptimizeMeshes;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Optimize Graph")]
        [EditorCategory("Optimization")]
        public bool OptimizeGraph
        {
            get => (PostProcessSteps & PostProcessSteps.OptimizeGraph) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.OptimizeGraph;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.OptimizeGraph;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Split Large Meshes")]
        [EditorCategory("Optimization")]
        public bool SplitLargeMeshes
        {
            get => (PostProcessSteps & PostProcessSteps.SplitLargeMeshes) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.SplitLargeMeshes;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.SplitLargeMeshes;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Remove Component")]
        [EditorCategory("Normals and UVs")]
        public bool RemoveComponent
        {
            get => (PostProcessSteps & PostProcessSteps.RemoveComponent) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.RemoveComponent;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.RemoveComponent;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Generate Normals")]
        [EditorCategory("Normals and UVs")]
        public bool GenerateNormals
        {
            get => (PostProcessSteps & PostProcessSteps.GenerateNormals) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.GenerateNormals;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.GenerateNormals;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Generate Smooth Normals")]
        [EditorCategory("Normals and UVs")]
        public bool GenerateSmoothNormals
        {
            get => (PostProcessSteps & PostProcessSteps.GenerateSmoothNormals) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.GenerateSmoothNormals;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.GenerateSmoothNormals;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Calculate Tangent Space")]
        [EditorCategory("Normals and UVs")]
        public bool CalculateTangentSpace
        {
            get => (PostProcessSteps & PostProcessSteps.CalculateTangentSpace) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.CalculateTangentSpace;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.CalculateTangentSpace;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Generate UV Coords")]
        [EditorCategory("Normals and UVs")]
        public bool GenerateUVCoords
        {
            get => (PostProcessSteps & PostProcessSteps.GenerateUVCoords) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.GenerateUVCoords;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.GenerateUVCoords;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Flip UVs")]
        [EditorCategory("Normals and UVs")]
        public bool FlipUVs
        {
            get => (PostProcessSteps & PostProcessSteps.FlipUVs) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.FlipUVs;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.FlipUVs;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Fix In Facing Normals")]
        [EditorCategory("Normals and UVs")]
        public bool FixInFacingNormals
        {
            get => (PostProcessSteps & PostProcessSteps.FixInFacingNormals) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.FixInFacingNormals;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.FixInFacingNormals;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Join Identical Vertices")]
        [EditorCategory("Geometry")]
        public bool JoinIdenticalVertices
        {
            get => (PostProcessSteps & PostProcessSteps.JoinIdenticalVertices) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.JoinIdenticalVertices;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.JoinIdenticalVertices;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Make Left Handed")]
        [EditorCategory("Geometry")]
        public bool MakeLeftHanded
        {
            get => (PostProcessSteps & PostProcessSteps.MakeLeftHanded) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.MakeLeftHanded;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.MakeLeftHanded;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Triangulate")]
        [EditorCategory("Geometry")]
        public bool Triangulate
        {
            get => (PostProcessSteps & PostProcessSteps.Triangulate) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.Triangulate;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.Triangulate;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Flip Winding Order")]
        [EditorCategory("Geometry")]
        public bool FlipWindingOrder
        {
            get => (PostProcessSteps & PostProcessSteps.FlipWindingOrder) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.FlipWindingOrder;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.FlipWindingOrder;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Debone")]
        [EditorCategory("Geometry")]
        public bool Debone
        {
            get => (PostProcessSteps & PostProcessSteps.Debone) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.Debone;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.Debone;
                }
            }
        }

        [JsonIgnore]
        [EditorProperty("Remove Redundant Materials")]
        [EditorCategory("Misc")]
        public bool RemoveRedundantMaterials
        {
            get => (PostProcessSteps & PostProcessSteps.RemoveRedundantMaterials) != 0;
            set
            {
                if (value)
                {
                    PostProcessSteps |= PostProcessSteps.RemoveRedundantMaterials;
                }
                else
                {
                    PostProcessSteps &= ~PostProcessSteps.RemoveRedundantMaterials;
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