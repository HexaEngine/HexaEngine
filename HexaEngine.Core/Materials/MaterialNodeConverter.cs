namespace HexaEngine.Materials
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Metadata;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Nodes.Functions;
    using HexaEngine.Materials.Nodes.Textures;
    using System;

    public class MaterialNodeConverter
    {
        public const string MetadataVersionKey = "MatNodes.Version";
        public const string MetadataKey = "MatNodes.Data";
        public const string Version = "1.0.0.1";

        public const string MetadataSurfaceVersionKey = "MatSurface.Version";
        public const string MetadataSurfaceKey = "MatSurface.Data";
        public const string SurfaceVersion = "1.0.0.0";

        public static void Convert(MaterialData material, ILogger logger)
        {
            NodeEditor editor = new();
            InputNode geometryNode = new(editor.GetUniqueId(), false, false);
            BRDFShadingModelNode outputNode = new(editor.GetUniqueId(), false, false);
            editor.AddNode(geometryNode);
            editor.AddNode(outputNode);
            editor.Initialize();

            ExtractProperties(material, editor);
            ExtractTextures(material, editor);

            InsertProperties(material, editor);
            InsertTextures(material, editor);

            ShaderGenerator generator = new();
            try
            {
                IOSignature inputSig = new("Pixel",
            new SignatureDef("color", new(VectorType.Float4)),
            new SignatureDef("pos", new(VectorType.Float4)),
            new SignatureDef("uv", new(VectorType.Float3)),
            new SignatureDef("normal", new(VectorType.Float3)),
            new SignatureDef("tangent", new(VectorType.Float3)),
            new SignatureDef("binormal", new(VectorType.Float3)));

                IOSignature outputSig = new("Material",
                    new SignatureDef("baseColor", new(VectorType.Float4)),
                    new SignatureDef("normal", new(VectorType.Float3)),
                    new SignatureDef("roughness", new(ScalarType.Float)),
                    new SignatureDef("metallic", new(ScalarType.Float)),
                    new SignatureDef("reflectance", new(ScalarType.Float)),
                new SignatureDef("ao", new(ScalarType.Float)),
                    new SignatureDef("emissive", new(VectorType.Float4)));

                string result = generator.Generate(outputNode, editor.Nodes, "setupMaterial", false, false, inputSig, outputSig);

                material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataSurfaceVersionKey).Value = SurfaceVersion;
                material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataSurfaceKey).Value = result;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to generate shader code");
                logger.Log(ex);
            }

            material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataVersionKey).Value = Version;
            material.Metadata.GetOrAdd<MetadataStringEntry>(MetadataKey).Value = editor.Serialize();
        }

        public static void ExtractProperties(MaterialData material, NodeEditor editor)
        {
            for (int i = 0; i < material.Properties.Count; i++)
            {
                var property = material.Properties[i];

                foreach (var pin in PropertyPin.FindPropertyPins(editor, property.Name))
                {
                    if (property.ValueType == MaterialValueType.Float)
                    {
                        var vec = property.AsFloat();
                        pin.ValueX = vec;
                    }
                    if (property.ValueType == MaterialValueType.Float2)
                    {
                        var vec = property.AsFloat2();
                        pin.ValueX = vec.X;
                        pin.ValueY = vec.Y;
                    }
                    if (property.ValueType == MaterialValueType.Float3)
                    {
                        var vec = property.AsFloat3();
                        pin.ValueX = vec.X;
                        pin.ValueY = vec.Y;
                        pin.ValueZ = vec.Z;
                    }
                    if (property.ValueType == MaterialValueType.Float4)
                    {
                        var vec = property.AsFloat4();
                        pin.ValueX = vec.X;
                        pin.ValueY = vec.Y;
                        pin.ValueZ = vec.Z;
                        pin.ValueW = vec.W;
                    }
                }
            }
        }

        public static void InsertProperties(MaterialData material, NodeEditor editor)
        {
            var outputNode = editor.GetNode<BRDFShadingModelNode>();
            for (int i = 0; i < outputNode.Pins.Count; i++)
            {
                var pin = outputNode.Pins[i];

                if (pin.Kind != PinKind.Input || pin is not PropertyPin propertyPin)
                {
                    continue;
                }

                if (!Enum.TryParse<MaterialPropertyType>(propertyPin.PropertyName, true, out var type))
                {
                    type = MaterialPropertyType.Unknown;
                }

                var idx = material.GetPropertyIndex(type);
                MaterialProperty property;

                if (idx == -1)
                {
                    MaterialValueType valueType = type switch
                    {
                        MaterialPropertyType.Unknown => MaterialValueType.Unknown,
                        MaterialPropertyType.ColorDiffuse => MaterialValueType.Float4,
                        MaterialPropertyType.ColorAmbient => MaterialValueType.Float4,
                        MaterialPropertyType.ColorSpecular => MaterialValueType.Float4,
                        MaterialPropertyType.ColorTransparent => MaterialValueType.Float4,
                        MaterialPropertyType.ColorReflective => MaterialValueType.Float4,
                        MaterialPropertyType.BaseColor => MaterialValueType.Float4,
                        MaterialPropertyType.Opacity => MaterialValueType.Float,
                        MaterialPropertyType.Specular => MaterialValueType.Float,
                        MaterialPropertyType.SpecularTint => MaterialValueType.Float,
                        MaterialPropertyType.Glossiness => MaterialValueType.Float,
                        MaterialPropertyType.AmbientOcclusion => MaterialValueType.Float,
                        MaterialPropertyType.Metallic => MaterialValueType.Float,
                        MaterialPropertyType.Roughness => MaterialValueType.Float,
                        MaterialPropertyType.Cleancoat => MaterialValueType.Float,
                        MaterialPropertyType.CleancoatGloss => MaterialValueType.Float,
                        MaterialPropertyType.Sheen => MaterialValueType.Float,
                        MaterialPropertyType.SheenTint => MaterialValueType.Float,
                        MaterialPropertyType.Anisotropy => MaterialValueType.Float,
                        MaterialPropertyType.Subsurface => MaterialValueType.Float,
                        MaterialPropertyType.SubsurfaceColor => MaterialValueType.Float3,
                        MaterialPropertyType.Transmission => MaterialValueType.Float3,
                        MaterialPropertyType.Emissive => MaterialValueType.Float3,
                        MaterialPropertyType.EmissiveIntensity => MaterialValueType.Float,
                        MaterialPropertyType.VolumeThickness => MaterialValueType.Float,
                        MaterialPropertyType.VolumeAttenuationDistance => MaterialValueType.Float,
                        MaterialPropertyType.VolumeAttenuationColor => MaterialValueType.Float4,
                        MaterialPropertyType.TwoSided => MaterialValueType.Bool,
                        MaterialPropertyType.ShadingMode => MaterialValueType.Int32,
                        MaterialPropertyType.EnableWireframe => MaterialValueType.Bool,
                        MaterialPropertyType.BlendFunc => MaterialValueType.Int32,
                        MaterialPropertyType.Transparency => MaterialValueType.Float,
                        MaterialPropertyType.BumpScaling => MaterialValueType.Float,
                        MaterialPropertyType.Shininess => MaterialValueType.Float,
                        MaterialPropertyType.ShininessStrength => MaterialValueType.Float,
                        MaterialPropertyType.Reflectance => MaterialValueType.Float,
                        MaterialPropertyType.IOR => MaterialValueType.Float,
                        MaterialPropertyType.DisplacementStrength => MaterialValueType.Float,
                        _ => MaterialValueType.Unknown,
                    };

                    if (valueType == MaterialValueType.Unknown)
                    {
                        continue;
                    }

                    property = new(propertyPin.PropertyName, type, valueType, Endianness.LittleEndian);
                    material.Properties.Add(property);
                }
                else
                {
                    property = material.Properties[idx];
                }

                if (property.ValueType == MaterialValueType.Float)
                {
                    property.SetFloat(propertyPin.ValueX);
                }
                if (property.ValueType == MaterialValueType.Float2)
                {
                    property.SetFloat2(propertyPin.Vector2);
                }
                if (property.ValueType == MaterialValueType.Float3)
                {
                    property.SetFloat3(propertyPin.Vector3);
                }
                if (property.ValueType == MaterialValueType.Float4)
                {
                    property.SetFloat4(propertyPin.Vector4);
                }
            }
        }

        public static void ExtractTextures(MaterialData material, NodeEditor editor)
        {
            for (int i = 0; i < material.Textures.Count; i++)
            {
                var texture = material.Textures[i];

                if (texture.File == Guid.Empty)
                {
                    continue;
                }

                var textureNode = TextureFileNode.FindTextureFileNode(editor, texture.File);
                if (textureNode != null)
                {
                    continue;
                }

                textureNode = new(editor.GetUniqueId(), true, false);
                editor.AddNode(textureNode);
                textureNode.Name = texture.Name ?? texture.Type.ToString();
                textureNode.Path = texture.File;
                textureNode.Filter = texture.Filter;
                textureNode.U = texture.U;
                textureNode.V = texture.V;
                textureNode.W = texture.W;
                textureNode.MipLODBias = texture.MipLODBias;
                textureNode.MaxAnisotropy = texture.MaxAnisotropy;
                textureNode.BorderColor = texture.BorderColor;
                textureNode.MinLOD = texture.MinLOD;
                textureNode.MaxLOD = texture.MaxLOD;
                var output = textureNode.Pins[0];

                if (texture.Type == MaterialTextureType.Normal)
                {
                    NormalMapNode normalMapNode = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(normalMapNode);
                    editor.CreateLink(normalMapNode.Pins[1], output);
                    output = normalMapNode.Pins[0];
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Normal"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    continue;
                }

                if (texture.Type == MaterialTextureType.RoughnessMetallic)
                {
                    SplitNode splitNode = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(splitNode);
                    editor.CreateLink(splitNode.Pins[0], output);
                    output = splitNode.Pins[2]; // g
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Roughness"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    output = splitNode.Pins[3]; // b
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Metallic"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    continue;
                }

                if (texture.Type == MaterialTextureType.AmbientOcclusionRoughnessMetallic)
                {
                    SplitNode splitNode = new(editor.GetUniqueId(), true, false);
                    editor.AddNode(splitNode);
                    editor.CreateLink(splitNode.Pins[0], output);
                    output = splitNode.Pins[1]; // r
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "AO"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    output = splitNode.Pins[2]; // g
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Roughness"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    output = splitNode.Pins[3]; // b
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Metallic"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    continue;
                }

                foreach (string aliasName in texture.GetNameAlias())
                {
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, aliasName))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                }
            }
        }

        public static void InsertTextures(MaterialData material, NodeEditor editor)
        {
            material.Textures.Clear();
            foreach (var textureNode in editor.GetNodes<TextureFileNode>())
            {
                if (textureNode.Path == Guid.Empty)
                {
                    continue;
                }

                var connection = textureNode.OutColor.FindLink<BRDFShadingModelNode>(PinKind.Input);

                if (connection is not PropertyPin propertyPin)
                {
                    continue;
                }

                if (!Enum.TryParse<MaterialTextureType>(propertyPin.PropertyName, true, out var type))
                {
                    type = MaterialTextureType.Unknown;
                }

                MaterialTexture texture;
                texture.Name = textureNode.Name;
                texture.Type = type;
                texture.File = textureNode.Path;
                texture.UVWSrc = 0;
                texture.Filter = textureNode.Filter;
                texture.U = textureNode.U;
                texture.V = textureNode.V;
                texture.W = textureNode.W;
                texture.MipLODBias = textureNode.MipLODBias;
                texture.MaxAnisotropy = textureNode.MaxAnisotropy;
                texture.BorderColor = textureNode.BorderColor;
                texture.MinLOD = textureNode.MinLOD;
                texture.MaxLOD = textureNode.MaxLOD;
                texture.Flags = TextureFlags.None;

                int index = material.GetTextureIndex(type);

                if (index != -1)
                {
                    material.Textures[index] = texture;
                }
                else
                {
                    material.Textures.Add(texture);
                }
            }
        }
    }
}