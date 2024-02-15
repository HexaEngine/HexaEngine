namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Binary.Materials;
    using System;

    public class MaterialImporter : IAssetImporter
    {
        public bool CanImport(string fileExtension)
        {
            return fileExtension == ".material";
        }

        public void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            FileStream? inputStream = null;
            FileStream? outputStream = null;
            try
            {
                inputStream = File.OpenRead(context.SourcePath);
                MaterialFile materialFile = MaterialFile.Read(inputStream);

                context.EmitArtifact(materialFile.Name, AssetType.Material, out outputStream);

                inputStream.Position = 0;
                inputStream.CopyTo(outputStream);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to load material, {context.SourcePath}");
            }
            finally
            {
                inputStream?.Dispose();
                outputStream?.Dispose();
            }
        }
    }
}