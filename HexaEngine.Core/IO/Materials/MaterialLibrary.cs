namespace HexaEngine.Core.IO.Materials
{
    using System.Text;
    using HexaEngine.Core.IO.Meshes;

    public class MaterialLibrary
    {
        private MaterialLibraryHeader header;
        public MaterialData[] Materials;

        public MaterialLibrary(string path)
        {
            var fs = FileSystem.Open(path);
            header.Read(fs);
            Materials = new MaterialData[header.MaterialCount];
            Encoding encoding = Encoding.UTF8;
            for (int i = 0; i < header.MaterialCount; i++)
            {
                MaterialData material;
                material.Name = fs.ReadString(encoding);
                material.BaseColor = fs.ReadVector3();
                material.Opacity = fs.ReadFloat();
                material.Specular = fs.ReadFloat();
                material.SpecularTint = fs.ReadFloat();
                material.SpecularColor = fs.ReadVector3();
                material.Ao = fs.ReadFloat();
                material.Metalness = fs.ReadFloat();
                material.Roughness = fs.ReadFloat();
                material.Cleancoat = fs.ReadFloat();
                material.CleancoatGloss = fs.ReadFloat();
                material.Sheen = fs.ReadFloat();
                material.SheenTint = fs.ReadFloat();
                material.Anisotropic = fs.ReadFloat();
                material.Subsurface = fs.ReadFloat();
                material.SubsurfaceColor = fs.ReadVector3();
                material.Emissive = fs.ReadVector3();

                material.BaseColorTextureMap = fs.ReadString(encoding);
                material.NormalTextureMap = fs.ReadString(encoding);
                material.DisplacementTextureMap = fs.ReadString(encoding);
                material.SpecularTextureMap = fs.ReadString(encoding);
                material.SpecularColorTextureMap = fs.ReadString(encoding);
                material.RoughnessTextureMap = fs.ReadString(encoding);
                material.MetalnessTextureMap = fs.ReadString(encoding);
                material.RoughnessMetalnessTextureMap = fs.ReadString(encoding);
                material.AoTextureMap = fs.ReadString(encoding);
                material.CleancoatTextureMap = fs.ReadString(encoding);
                material.CleancoatGlossTextureMap = fs.ReadString(encoding);
                material.SheenTextureMap = fs.ReadString(encoding);
                material.SheenTintTextureMap = fs.ReadString(encoding);
                material.AnisotropicTextureMap = fs.ReadString(encoding);
                material.SubsurfaceTextureMap = fs.ReadString(encoding);
                material.SubsurfaceColorTextureMap = fs.ReadString(encoding);
                material.EmissiveTextureMap = fs.ReadString(encoding);
                Materials[i] = material;
            }
        }

        public MaterialLibrary(Stream stream)
        {
            header.Read(stream);
            Materials = new MaterialData[header.MaterialCount];
        }

        public MaterialLibrary(MaterialData[] materials)
        {
            header.MaterialCount = (uint)materials.LongLength;
            Materials = materials;
        }

        public void Write(string path)
        {
            var fs = File.Create(path);
            Write(fs);
            fs.Close();
        }

        public void Write(Stream stream)
        {
            header.MaterialCount = (uint)Materials.LongLength;
            header.Write(stream);

            Encoding encoding = Encoding.UTF8;
            for (int i = 0; i < Materials.Length; i++)
            {
                MaterialData material = Materials[i];
                stream.WriteString(material.Name, encoding);
                stream.WriteVector3(material.BaseColor);
                stream.WriteFloat(material.Opacity);
                stream.WriteFloat(material.Specular);
                stream.WriteFloat(material.SpecularTint);
                stream.WriteVector3(material.SpecularColor);
                stream.WriteFloat(material.Ao);
                stream.WriteFloat(material.Metalness);
                stream.WriteFloat(material.Roughness);
                stream.WriteFloat(material.Cleancoat);
                stream.WriteFloat(material.CleancoatGloss);
                stream.WriteFloat(material.Sheen);
                stream.WriteFloat(material.SheenTint);
                stream.WriteFloat(material.Anisotropic);
                stream.WriteFloat(material.Subsurface);
                stream.WriteVector3(material.SubsurfaceColor);
                stream.WriteVector3(material.Emissive);

                stream.WriteString(material.BaseColorTextureMap, encoding);
                stream.WriteString(material.NormalTextureMap, encoding);
                stream.WriteString(material.DisplacementTextureMap, encoding);
                stream.WriteString(material.SpecularTextureMap, encoding);
                stream.WriteString(material.SpecularColorTextureMap, encoding);
                stream.WriteString(material.RoughnessTextureMap, encoding);
                stream.WriteString(material.MetalnessTextureMap, encoding);
                stream.WriteString(material.RoughnessMetalnessTextureMap, encoding);
                stream.WriteString(material.AoTextureMap, encoding);
                stream.WriteString(material.CleancoatTextureMap, encoding);
                stream.WriteString(material.CleancoatGlossTextureMap, encoding);
                stream.WriteString(material.SheenTextureMap, encoding);
                stream.WriteString(material.SheenTintTextureMap, encoding);
                stream.WriteString(material.AnisotropicTextureMap, encoding);
                stream.WriteString(material.SubsurfaceTextureMap, encoding);
                stream.WriteString(material.SubsurfaceColorTextureMap, encoding);
                stream.WriteString(material.EmissiveTextureMap, encoding);
            }
        }
    }
}