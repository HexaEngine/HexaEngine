namespace HexaEngine.Editor.MaterialEditor
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.MaterialEditor.Nodes.Functions;
    using HexaEngine.Editor.MaterialEditor.Nodes.Textures;
    using HexaEngine.Editor.NodeEditor;
    using System;

    public class MaterialNodeConverter
    {
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
                    continue;
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

                    property = new(type.ToString(), type, valueType, Endianness.LittleEndian);
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

                textureNode = new(editor.GetUniqueId(), true, false, null);
                editor.AddNode(textureNode);
                textureNode.Name = texture.Name ?? texture.Type.ToString();
                textureNode.Path = texture.File;
                textureNode.SamplerDescription = texture.GetSamplerDesc();
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
                    output = splitNode.Pins[1];
                    foreach (var pin in PropertyPin.FindPropertyPins(editor, "Roughness"))
                    {
                        if (pin.CanCreateLink(output))
                        {
                            editor.CreateLink(pin, output);
                        }
                    }
                    output = splitNode.Pins[2];
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
                    continue;
                }

                var desc = textureNode.SamplerDescription;

                Core.IO.Binary.Materials.MaterialTexture texture;
                texture.Name = textureNode.Name;
                texture.Type = type;
                texture.File = textureNode.Path;
                texture.Blend = BlendMode.Default;
                texture.Op = TextureOp.Add;
                texture.Mapping = 0;
                texture.UVWSrc = 0;
                texture.U = Core.IO.Binary.Materials.MaterialTexture.Convert(desc.AddressU);
                texture.V = Core.IO.Binary.Materials.MaterialTexture.Convert(desc.AddressV);
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