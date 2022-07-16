namespace HexaEngine.Scenes
{
    using HexaEngine.Mathematics;
    using Silk.NET.Assimp;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;

    public class AssimpSceneLoader
    {
        public static Assimp Assimp = Assimp.GetApi();

        public static unsafe void Load(string path)
        {
            var scene = Assimp.ImportFile(path, (uint)(ImporterFlags.ImporterFlagsSupportBinaryFlavour | ImporterFlags.ImporterFlagsSupportTextFlavour | ImporterFlags.ImporterFlagsSupportCompressedFlavour));
            Assimp.ApplyPostProcessing(scene, (uint)(PostProcessSteps.CalculateTangentSpace | PostProcessSteps.MakeLeftHanded | PostProcessSteps.Triangulate));

            AssimpMaterial[] materials = new AssimpMaterial[scene->MNumMaterials];
            for (int i = 0; i < scene->MNumMaterials; i++)
            {
                Material* mat = scene->MMaterials[i];
                Dictionary<(string, object), object> props = new();
                AssimpMaterialTexture[] textures = new AssimpMaterialTexture[(int)TextureType.TextureTypeUnknown + 1];
                for (int j = 0; j < textures.Length; j++)
                {
                    textures[j].Type = (TextureType)j;
                }
                AssimpMaterial material = new();

                for (int j = 0; j < mat->MNumProperties; j++)
                {
                    MaterialProperty* prop = mat->MProperties[j];
                    Span<byte> buffer = new(prop->MData, (int)prop->MDataLength);
                    string key = prop->MKey;
                    int semantic = (int)prop->MSemantic;
                    switch (key)
                    {
                        case Assimp.MatkeyName:
                            material.Name = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyTwosided:
                            material.Twosided = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyShadingModel:
                            material.ShadingModel = (ShadingMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyEnableWireframe:
                            material.EnableWireframe = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyBlendFunc:
                            material.BlendFunc = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyOpacity:
                            material.Opacity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyTransparencyfactor:
                            material.Transparencyfactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyBumpscaling:
                            material.Bumpscaling = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyShininess:
                            material.Shininess = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyReflectivity:
                            material.Reflectivity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyShininessStrength:
                            material.ShininessStrength = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyRefracti:
                            material.Refracti = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyColorDiffuse:
                            material.ColorDiffuse = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorAmbient:
                            material.ColorAmbient = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorSpecular:
                            material.ColorSpecular = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorEmissive:
                            material.ColorEmissive = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorTransparent:
                            material.ColorTransparent = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyColorReflective:
                            material.ColorReflective = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseColorMap:
                            material.UseColorMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyBaseColor:
                            material.BaseColor = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseMetallicMap:
                            material.UseMetallicMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyMetallicFactor:
                            material.MetallicFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyUseRoughnessMap:
                            material.UseRoughnessMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyRoughnessFactor:
                            material.RoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyAnisotropyFactor:
                            material.AnisotropyFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySpecularFactor:
                            material.SpecularFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyGlossinessFactor:
                            material.GlossinessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySheenColorFactor:
                            material.SheenColorFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeySheenRoughnessFactor:
                            material.SheenRoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyClearcoatFactor:
                            material.ClearcoatFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyClearcoatRoughnessFactor:
                            material.ClearcoatRoughnessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyTransmissionFactor:
                            material.TransmissionFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeThicknessFactor:
                            material.VolumeThicknessFactor = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeAttenuationDistance:
                            material.VolumeAttenuationDistance = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyVolumeAttenuationColor:
                            material.VolumeAttenuationColor = new(MemoryMarshal.Cast<byte, float>(buffer));
                            break;

                        case Assimp.MatkeyUseEmissiveMap:
                            material.UseEmissiveMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyEmissiveIntensity:
                            material.EmissiveIntensity = MemoryMarshal.Cast<byte, float>(buffer)[0];
                            break;

                        case Assimp.MatkeyUseAOMap:
                            material.UseAOMap = buffer[0] == 1;
                            break;

                        case Assimp.MatkeyTextureBase:
                            textures[semantic].File = Encoding.UTF8.GetString(buffer.Slice(4, buffer.Length - 4 - 1));
                            break;

                        case Assimp.MatkeyUvwsrcBase:
                            textures[semantic].UVWSrc = MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexopBase:
                            textures[semantic].Op = (TextureOp)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingBase:
                            textures[semantic].Mapping = MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexblendBase:
                            textures[semantic].Blend = (BlendMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingmodeUBase:
                            textures[semantic].U = (TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyMappingmodeVBase:
                            textures[semantic].V = (TextureMapMode)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;

                        case Assimp.MatkeyTexmapAxisBase:
                            break;

                        case Assimp.MatkeyUvtransformBase:
                            break;

                        case Assimp.MatkeyTexflagsBase:
                            textures[semantic].Flags = (TextureFlags)MemoryMarshal.Cast<byte, int>(buffer)[0];
                            break;
                    }
                }
                material.Textures = textures;
                materials[i] = material;
            }
            fixed (Material** ptr = new Material*[1])
            {
                scene->MMaterials = ptr;
                scene->MNumMaterials = 1;
            }

            AssimpMesh[] meshes = new AssimpMesh[scene->MNumMeshes];
            for (int i = 0; i < scene->MNumMeshes; i++)
            {
                Mesh* msh = scene->MMeshes[i];
                AssimpMesh mesh = new();
                mesh.Material = msh->MMaterialIndex;
                mesh.Name = msh->MName;
                Vertex[] vertices = new Vertex[msh->MNumVertices];
                int[] indices = new int[msh->MNumFaces * 3];
                for (int j = 0; j < msh->MNumFaces; j++)
                {
                    var face = msh->MFaces[j];
                    for (int k = 0; k < 3; k++)
                    {
                        indices[j * 3 + k] = (int)face.MIndices[k];
                    }
                }

                for (int j = 0; j < msh->MNumVertices; j++)
                {
                    Vector3 pos = msh->MVertices[j];
                    Vector3 nor = msh->MNormals[j];
                    Vector3 tex = msh->MTextureCoords[0][j];
                    Vector3 tan = msh->MTangents[j];

                    Vertex vertex = new(pos, tex, nor, tan);
                    vertices[j] = vertex;
                }

                mesh.Vertices = vertices;
                mesh.Indices = indices;
                meshes[i] = mesh;
            }

            for (int i = 0; i < scene->MNumTextures; i++)
            {
                var tex = scene->MTextures[i];
            }
        }

        public struct AssimpMaterial
        {
            public string Name = string.Empty;
            public bool Twosided = false;
            public ShadingMode ShadingModel = ShadingMode.ShadingModeGouraud;
            public bool EnableWireframe = false;
            public bool BlendFunc = false;
            public float Opacity = 1;
            public float Transparencyfactor = 1;
            public float Bumpscaling = 1;
            public float Shininess = 0;
            public float Reflectivity = 0;
            public float ShininessStrength = 1;
            public float Refracti = 1;
            public Vector3 ColorDiffuse = Vector3.Zero;
            public Vector3 ColorAmbient = Vector3.Zero;
            public Vector3 ColorSpecular = Vector3.Zero;
            public Vector3 ColorEmissive = Vector3.Zero;
            public Vector3 ColorTransparent = Vector3.Zero;
            public Vector3 ColorReflective = Vector3.Zero;

            public bool UseColorMap = false;
            public Vector3 BaseColor = Vector3.Zero;
            public bool UseMetallicMap = false;
            public float MetallicFactor = 0f;
            public bool UseRoughnessMap = false;
            public float RoughnessFactor = 0.5f;
            public float AnisotropyFactor = 0f;
            public float SpecularFactor = 0.5f;
            public float GlossinessFactor = 0f;
            public float SheenColorFactor = 0f;
            public float SheenRoughnessFactor = 0.5f;
            public float ClearcoatFactor = 0f;
            public float ClearcoatRoughnessFactor = 0.03f;
            public float TransmissionFactor = 0f;
            public float VolumeThicknessFactor = 0;
            public float VolumeAttenuationDistance = 0;
            public Vector3 VolumeAttenuationColor = Vector3.Zero;
            public bool UseEmissiveMap = false;
            public float EmissiveIntensity = 1;
            public bool UseAOMap = false;
            public AssimpMaterialTexture[] Textures = Array.Empty<AssimpMaterialTexture>();

            public AssimpMaterial()
            {
            }
        }

        public struct AssimpMaterialTexture
        {
            public TextureType Type;
            public string File;
            public BlendMode Blend;
            public TextureOp Op;
            public int Mapping;
            public int UVWSrc;
            public TextureMapMode U;
            public TextureMapMode V;
            public TextureFlags Flags;
        }

        public struct AssimpMesh
        {
            public string Name;
            public Vertex[] Vertices;
            public int[] Indices;
            public uint Material;
        }
    }
}